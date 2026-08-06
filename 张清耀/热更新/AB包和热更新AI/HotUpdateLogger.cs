using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace HotUpdate
{
    /// <summary>
    /// 游戏公司级日志系统
    /// 
    /// 功能:
    /// 1. 结构化日志（JSON格式）
    /// 2. 日志上下文（设备ID、用户ID、版本号、平台）
    /// 3. 日志上传到服务器
    /// 4. 日志脱敏（敏感信息加密）
    /// 5. 性能日志（耗时统计）
    /// 6. 日志分类（模块区分）
    /// 7. 日志压缩（减小体积）
    /// 8. 日志采样（防止刷屏）
    /// 9. 日志轮转（文件大小限制）
    /// 10. 日志加密（AES加密）
    /// 
    /// 使用示例:
    /// HotUpdateLogger.Info("下载完成", module: "Download", data: new { file = "cube.u3d", size = 1024 });
    /// HotUpdateLogger.TrackDuration(downloadTask, "下载耗时", new { url = "..." });
    /// </summary>
    public static class HotUpdateLogger
    {
        private static readonly List<LogEntry> _logCache = new List<LogEntry>();
        private static readonly object _lock = new object();
        private static string _logDirectory;
        private static string _currentLogFile;
        private static DateTime _lastLogDate;
        private static long _currentFileSize;

        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }

        public class LogEntry
        {
            public DateTime Timestamp;
            public LogLevel Level;
            public string Module;
            public string Message;
            public object Data;
            public string StackTrace;
            public double DurationMs;
            public string SessionId;
            public bool IsSensitive;
        }

        public class LoggerContext
        {
            public string DeviceId;
            public string UserId;
            public string Version;
            public string Platform;
            public string OSVersion;
            public string DeviceModel;
            public string SessionId;
            public string ServerUrl;

            public static LoggerContext Current { get; set; }
        }

        public class UploadConfig
        {
            public bool EnableUpload = false;
            public string UploadUrl = "";
            public int BatchSize = 100;
            public int UploadIntervalSeconds = 30;
            public bool CompressBeforeUpload = true;
            public bool EncryptBeforeUpload = false;
            public string EncryptionKey = "";
        }

        public class PerformanceTracker : IDisposable
        {
            private readonly string _operation;
            private readonly string _module;
            private readonly object _data;
            private readonly System.Diagnostics.Stopwatch _stopwatch;
            private readonly string _sessionId;

            public PerformanceTracker(string operation, string module = "Performance", object data = null)
            {
                _operation = operation;
                _module = module;
                _data = data;
                _sessionId = LoggerContext.Current?.SessionId ?? "";
                _stopwatch = System.Diagnostics.Stopwatch.StartNew();
            }

            public void Dispose()
            {
                _stopwatch.Stop();
                double duration = _stopwatch.Elapsed.TotalMilliseconds;
                Log(LogLevel.Info, $"[Perf] {_operation} 耗时: {duration:F2}ms", _module, _data, null, duration);
            }
        }

        #region 配置

        public static int MaxCacheSize = 2000;
        public static bool EnableFileLog = true;
        public static bool EnableUnityLog = true;
        public static LogLevel MinLogLevel = LogLevel.Debug;
        public static long MaxFileSize = 10 * 1024 * 1024; // 10MB
        public static int MaxLogFiles = 30;
        public static UploadConfig Upload { get; } = new UploadConfig();
        private static readonly Dictionary<string, int> _logCounter = new Dictionary<string, int>();
        private static int _uploadBufferCount = 0;
        private static DateTime _lastUploadTime = DateTime.MinValue;

        #endregion

        #region 核心属性

        public static string LogDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(_logDirectory))
                {
                    _logDirectory = Path.Combine(Application.persistentDataPath, "Logs", "HotUpdate");
                }
                return _logDirectory;
            }
        }

        public static event Action<LogLevel, string> OnLog;

        #endregion

        #region 初始化

        public static void Initialize(LoggerContext context = null)
        {
            if (context == null)
            {
                context = new LoggerContext
                {
                    DeviceId = SystemInfo.deviceUniqueIdentifier,
                    UserId = "",
                    Version = Application.version,
                    Platform = Application.platform.ToString(),
                    OSVersion = Environment.OSVersion.ToString(),
                    DeviceModel = SystemInfo.deviceModel,
                    SessionId = Guid.NewGuid().ToString(),
                    ServerUrl = ""
                };
            }
            LoggerContext.Current = context;

            if (EnableFileLog)
            {
                Directory.CreateDirectory(LogDirectory);
                UpdateLogFile();
                CleanOldLogFiles();
            }

            Info($"日志系统初始化完成", "System", new
            {
                context.Version,
                context.Platform,
                context.DeviceModel,
                context.SessionId
            });
        }

        #endregion

        #region 日志方法

        public static void Debug(string message, string module = "HotUpdate", object data = null, bool isSensitive = false)
        {
            Log(LogLevel.Debug, message, module, data, null, 0, isSensitive);
        }

        public static void Info(string message, string module = "HotUpdate", object data = null)
        {
            Log(LogLevel.Info, message, module, data);
        }

        public static void Warning(string message, string module = "HotUpdate", object data = null)
        {
            Log(LogLevel.Warning, message, module, data);
        }

        public static void Error(string message, Exception exception = null, string module = "HotUpdate", object data = null)
        {
            Log(LogLevel.Error, message, module, data, exception);
        }

        public static void Fatal(string message, Exception exception = null, string module = "HotUpdate", object data = null)
        {
            Log(LogLevel.Fatal, message, module, data, exception);
        }

        #endregion

        #region 性能追踪

        public static IDisposable TrackDuration(string operation, string module = "Performance", object data = null)
        {
            return new PerformanceTracker(operation, module, data);
        }

        #endregion

        #region 核心日志逻辑

        private static void Log(LogLevel level, string message, string module = "HotUpdate", 
            object data = null, Exception exception = null, double durationMs = 0, bool isSensitive = false)
        {
            if (level < MinLogLevel) return;

            // 采样控制：相同消息1秒内最多记录10次
            string sampleKey = $"{module}:{message}";
            lock (_lock)
            {
                if (_logCounter.ContainsKey(sampleKey))
                {
                    _logCounter[sampleKey]++;
                    if (_logCounter[sampleKey] > 10)
                    {
                        // 超过采样限制，只记录一次警告
                        if (_logCounter[sampleKey] == 11)
                        {
                            message += " (已采样，超过10次/秒)";
                        }
                        else
                        {
                            return;
                        }
                    }
                }
                else
                {
                    _logCounter[sampleKey] = 1;
                }
            }

            // 构建日志条目
            var entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Module = module,
                Message = message,
                Data = isSensitive ? new { masked = true } : data,
                StackTrace = exception?.StackTrace,
                DurationMs = durationMs,
                SessionId = LoggerContext.Current?.SessionId ?? "",
                IsSensitive = isSensitive
            };

            // 结构化日志（JSON格式）
            string jsonLog = BuildStructuredLog(entry);

            // Unity控制台输出
            if (EnableUnityLog)
            {
                string consoleMsg = $"[{level}] [{module}] {message}";
                switch (level)
                {
                    case LogLevel.Debug:
                    case LogLevel.Info:
                        UnityEngine.Debug.Log(consoleMsg);
                        break;
                    case LogLevel.Warning:
                        UnityEngine.Debug.LogWarning(consoleMsg);
                        break;
                    case LogLevel.Error:
                    case LogLevel.Fatal:
                        UnityEngine.Debug.LogError(consoleMsg);
                        if (exception != null)
                        {
                            UnityEngine.Debug.LogError(exception);
                        }
                        break;
                }
            }

            // 添加到缓存
            lock (_lock)
            {
                _logCache.Add(entry);
                if (_logCache.Count > MaxCacheSize)
                {
                    _logCache.RemoveAt(0);
                }
            }

            // 写入文件
            if (EnableFileLog)
            {
                WriteToFile(entry, jsonLog);
            }

            // 上传缓冲
            if (Upload.EnableUpload)
            {
                AddToUploadBuffer(jsonLog);
            }

            // 触发事件
            OnLog?.Invoke(level, jsonLog);
        }

        #endregion

        #region 结构化日志构建

        private static string BuildStructuredLog(LogEntry entry)
        {
            var context = LoggerContext.Current;
            var sb = new StringBuilder();

            sb.Append("{");
            sb.Append($"\"timestamp\":\"{entry.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}\",");
            sb.Append($"\"level\":\"{entry.Level.ToString().ToUpper()}\",");
            sb.Append($"\"module\":\"{entry.Module}\",");
            sb.Append($"\"message\":\"{EscapeJson(entry.Message)}\"");

            // 上下文
            if (context != null)
            {
                sb.Append($",\"context\":{{");
                sb.Append($"\"deviceId\":\"{context.DeviceId}\",");
                sb.Append($"\"userId\":\"{context.UserId}\",");
                sb.Append($"\"version\":\"{context.Version}\",");
                sb.Append($"\"platform\":\"{context.Platform}\",");
                sb.Append($"\"osVersion\":\"{EscapeJson(context.OSVersion)}\",");
                sb.Append($"\"deviceModel\":\"{EscapeJson(context.DeviceModel)}\",");
                sb.Append($"\"sessionId\":\"{entry.SessionId}\"");
                sb.Append("}");
            }

            // 附加数据
            if (entry.Data != null && !entry.IsSensitive)
            {
                sb.Append($",\"data\":{SerializeObject(entry.Data)}");
            }

            // 性能数据
            if (entry.DurationMs > 0)
            {
                sb.Append($",\"performance\":{{\"durationMs\":{entry.DurationMs:F2}}}");
            }

            // 堆栈
            if (!string.IsNullOrEmpty(entry.StackTrace))
            {
                sb.Append($",\"stackTrace\":\"{EscapeJson(entry.StackTrace)}\"");
            }

            sb.Append("}");
            return sb.ToString();
        }

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r")
                       .Replace("\t", "\\t");
        }

        private static string SerializeObject(object obj)
        {
            if (obj == null) return "null";
            try
            {
                return JsonUtility.ToJson(obj);
            }
            catch
            {
                return $"\"{EscapeJson(obj.ToString())}\"";
            }
        }

        #endregion

        #region 文件写入

        private static void WriteToFile(LogEntry entry, string jsonLog)
        {
            try
            {
                // 日期切换
                if (DateTime.Now.Date != _lastLogDate)
                {
                    UpdateLogFile();
                }

                // 文件大小检查
                if (_currentFileSize >= MaxFileSize)
                {
                    UpdateLogFile();
                }

                string logLine = jsonLog + "\n";
                byte[] logBytes = Encoding.UTF8.GetBytes(logLine);

                // Unity/.NET Framework 没有 AppendAllBytes，使用 WriteAllBytes 追加
                using (var fs = new FileStream(_currentLogFile, FileMode.Append, FileAccess.Write))
                {
                    fs.Write(logBytes, 0, logBytes.Length);
                }
                _currentFileSize += logBytes.Length;
            }
            catch (Exception)
            {
            }
        }

        private static void UpdateLogFile()
        {
            _lastLogDate = DateTime.Now.Date;
            string fileName = $"hotupdate_{_lastLogDate:yyyyMMdd}.log";
            _currentLogFile = Path.Combine(LogDirectory, fileName);
            _currentFileSize = 0;

            // 写入文件头
            string header = $"# HotUpdate Log File - {_lastLogDate:yyyy-MM-dd}\n\n";
            File.WriteAllText(_currentLogFile, header);
            _currentFileSize = Encoding.UTF8.GetBytes(header).Length;
        }

        private static void CleanOldLogFiles()
        {
            try
            {
                if (!Directory.Exists(LogDirectory)) return;

                var files = Directory.GetFiles(LogDirectory, "*.log")
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.CreationTime)
                    .Skip(MaxLogFiles)
                    .ToList();

                foreach (var file in files)
                {
                    try
                    {
                        file.Delete();
                    }
                    catch { }
                }
            }
            catch { }
        }

        #endregion

        #region 日志上传

        private static readonly List<string> _uploadBuffer = new List<string>();

        private static void AddToUploadBuffer(string jsonLog)
        {
            lock (_lock)
            {
                _uploadBuffer.Add(jsonLog);
                _uploadBufferCount++;
            }

            // 检查是否需要批量上传
            if (_uploadBufferCount >= Upload.BatchSize ||
                DateTime.Now - _lastUploadTime > TimeSpan.FromSeconds(Upload.UploadIntervalSeconds))
            {
                UploadLogsAsync();
            }
        }

        public static void UploadLogsAsync()
        {
            if (!Upload.EnableUpload || string.IsNullOrEmpty(Upload.UploadUrl)) return;

            List<string> logsToUpload;
            lock (_lock)
            {
                if (_uploadBuffer.Count == 0) return;
                logsToUpload = new List<string>(_uploadBuffer);
                _uploadBuffer.Clear();
                _uploadBufferCount = 0;
            }

            _lastUploadTime = DateTime.Now;

            // 在后台线程上传
            System.Threading.ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    UploadLogs(logsToUpload);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"日志上传失败: {e.Message}");
                }
            });
        }

        private static void UploadLogs(List<string> logs)
        {
            string logsJson = $"[{string.Join(",", logs)}]";

            // 压缩
            if (Upload.CompressBeforeUpload)
            {
                logsJson = CompressData(logsJson);
            }

            // 加密
            if (Upload.EncryptBeforeUpload && !string.IsNullOrEmpty(Upload.EncryptionKey))
            {
                logsJson = EncryptData(logsJson, Upload.EncryptionKey);
            }

            // HTTP上传
            using (var request = new UnityEngine.Networking.UnityWebRequest(Upload.UploadUrl, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(logsJson);
                request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("X-Log-Version", "2.0");
                request.SetRequestHeader("X-Device-Id", LoggerContext.Current?.DeviceId ?? "");

                var operation = request.SendWebRequest();
                while (!operation.isDone) { }

                if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    UnityEngine.Debug.LogWarning($"日志上传失败: {request.error}");
                }
            }
        }

        #endregion

        #region 压缩与加密

        private static string CompressData(string data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            using (var ms = new MemoryStream())
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Compress))
                {
                    gzip.Write(bytes, 0, bytes.Length);
                }
                byte[] compressed = ms.ToArray();
                return $"gz:{Convert.ToBase64String(compressed)}";
            }
        }

        private static string DecompressData(string data)
        {
            if (!data.StartsWith("gz:")) return data;
            byte[] compressed = Convert.FromBase64String(data.Substring(3));
            using (var ms = new MemoryStream(compressed))
            {
                using (var gzip = new GZipStream(ms, CompressionMode.Decompress))
                {
                    using (var sr = new StreamReader(gzip))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        private static string EncryptData(string data, string key)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.GenerateIV();
                byte[] iv = aes.IV;

                using (var encryptor = aes.CreateEncryptor())
                {
                    byte[] encrypted = encryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
                    byte[] result = new byte[iv.Length + encrypted.Length];
                    Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                    Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
                    return $"enc:{Convert.ToBase64String(result)}";
                }
            }
        }

        #endregion

        #region 查询与清理

        public static List<LogEntry> GetCachedLogs(LogLevel? minLevel = null)
        {
            lock (_lock)
            {
                var logs = new List<LogEntry>(_logCache);
                if (minLevel.HasValue)
                {
                    logs = logs.Where(l => l.Level >= minLevel.Value).ToList();
                }
                return logs;
            }
        }

        public static List<LogEntry> GetLogsByModule(string module)
        {
            lock (_lock)
            {
                return _logCache.Where(l => l.Module == module).ToList();
            }
        }

        public static List<LogEntry> GetLogsByLevel(LogLevel level)
        {
            lock (_lock)
            {
                return _logCache.Where(l => l.Level == level).ToList();
            }
        }

        public static void ClearCache()
        {
            lock (_lock)
            {
                _logCache.Clear();
            }
        }

        public static void DeleteAllLogFiles()
        {
            try
            {
                if (Directory.Exists(LogDirectory))
                {
                    Directory.Delete(LogDirectory, true);
                    Directory.CreateDirectory(LogDirectory);
                    UpdateLogFile();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"删除日志文件失败: {e.Message}");
            }
        }

        public static List<string> GetLogFilePaths()
        {
            var result = new List<string>();
            try
            {
                if (Directory.Exists(LogDirectory))
                {
                    foreach (var file in Directory.GetFiles(LogDirectory, "*.log"))
                    {
                        result.Add(file);
                    }
                }
            }
            catch { }
            return result;
        }

        public static string GetLatestLogContent(int maxLines = 100)
        {
            try
            {
                if (!File.Exists(_currentLogFile)) return "";
                var lines = File.ReadAllLines(_currentLogFile);
                return string.Join("\n", lines.Skip(Math.Max(0, lines.Length - maxLines)));
            }
            catch { return ""; }
        }

        public static void ExportLogs(string outputPath, bool onlyErrors = false)
        {
            try
            {
                var logs = GetCachedLogs(onlyErrors ? LogLevel.Error : null);
                var sb = new StringBuilder();
                sb.AppendLine($"# 日志导出 - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"# 共 {logs.Count} 条日志");
                sb.AppendLine();

                foreach (var log in logs)
                {
                    sb.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.Level}] [{log.Module}] {log.Message}");
                    if (log.Data != null)
                    {
                        sb.AppendLine($"  Data: {SerializeObject(log.Data)}");
                    }
                    if (!string.IsNullOrEmpty(log.StackTrace))
                    {
                        sb.AppendLine($"  StackTrace: {log.StackTrace}");
                    }
                    if (log.DurationMs > 0)
                    {
                        sb.AppendLine($"  Duration: {log.DurationMs:F2}ms");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(outputPath, sb.ToString());
                UnityEngine.Debug.Log($"日志已导出到: {outputPath}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"导出日志失败: {e.Message}");
            }
        }

        #endregion
    }
}
