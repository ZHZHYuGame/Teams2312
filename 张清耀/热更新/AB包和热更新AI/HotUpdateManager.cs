using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HotUpdate
{
    /// <summary>
    /// 热更新管理器 - 游戏公司版本
    /// 负责管理客户端热更新的完整流程，包括版本检查、资源下载、缓存管理等
    /// 
    /// 主要功能:
    /// 1. 版本检查与对比 - 对比本地版本与服务器版本，判断是否需要更新
    /// 2. 依赖包自动收集 - 递归收集所有依赖的AB包，确保依赖完整
    /// 3. 多线程并发下载 - 支持同时下载多个AB包，提高下载速度
    /// 4. 自动重试机制 - 下载失败时自动重试，提高成功率
    /// 5. 加密支持 - 支持对下载的AB包进行解密
    /// 6. Hash校验 - 验证下载文件的完整性，防止传输损坏
    /// 7. 灰度更新 - 支持按比例或指定用户进行灰度测试
    /// 8. 缓存管理 - 清理过期缓存，节省存储空间
    /// 
    /// 使用示例:
    /// HotUpdateManager.Create();
    /// HotUpdateManager.Instance.CheckUpdate(hasUpdate => { ... });
    /// HotUpdateManager.Instance.StartUpdate();
    /// </summary>
    public class HotUpdateManager : MonoBehaviour
    {
        /// <summary>
        /// 单例实例 - 全局唯一的热更新管理器
        /// </summary>
        public static HotUpdateManager Instance { get; private set; }

        /// <summary>
        /// 热更新配置 - 包含服务器地址、超时时间、重试次数等参数
        /// </summary>
        public HotUpdateConfig Config { get; private set; } = new HotUpdateConfig();

        /// <summary>
        /// 本地版本信息 - 记录当前客户端已有的AB包列表
        /// </summary>
        public VersionInfo LocalVersion { get; private set; }

        /// <summary>
        /// 服务器版本信息 - 从服务器获取的最新版本信息
        /// </summary>
        public VersionInfo ServerVersion { get; private set; }

        /// <summary>
        /// 全局下载进度事件 - 参数为0~1的进度值
        /// 用于UI显示总体下载进度
        /// </summary>
        public event Action<float> OnDownloadProgress;

        /// <summary>
        /// 更新完成事件 - 参数为是否成功
        /// 下载全部完成后触发
        /// </summary>
        public event Action<bool> OnUpdateComplete;

        /// <summary>
        /// 错误事件 - 参数为错误信息
        /// 发生错误时触发，用于日志记录和UI提示
        /// </summary>
        public event Action<string> OnError;

        /// <summary>
        /// 单个任务完成事件 - 参数为完成的下载任务
        /// 每个AB包下载完成时触发
        /// </summary>
        public event Action<DownloadTask> OnTaskComplete;

        /// <summary>
        /// 单个任务进度事件 - 参数为(AB包名称, 进度0~1)
        /// 每个AB包的下载进度变化时触发
        /// </summary>
        public event Action<string, float> OnSingleTaskProgress;

        /// <summary>
        /// 是否正在更新 - 防止重复触发更新流程
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// 是否正在下载 - 用于判断是否可以取消下载
        /// </summary>
        private bool _isDownloading;

        /// <summary>
        /// 下载任务列表 - 所有待下载和正在下载的任务
        /// </summary>
        private readonly List<DownloadTask> _downloadTasks = new List<DownloadTask>();

        /// <summary>
        /// 已完成的任务数量 - 用于计算总体进度
        /// </summary>
        private int _completedTasks;

        /// <summary>
        /// 总任务数量 - 用于计算总体进度
        /// </summary>
        private int _totalTasks;

        /// <summary>
        /// 当前服务器版本号 - 从根version.json获取
        /// </summary>
        private string _currentServerVersion;

        /// <summary>
        /// 创建热更新管理器实例
        /// 建议在游戏启动时调用一次，保持全局唯一
        /// </summary>
        public static void Create()
        {
            if (Instance == null)
            {
                // 创建一个新的GameObject并添加管理器组件
                var go = new GameObject("[HotUpdateManager]");
                DontDestroyOnLoad(go);  // 场景切换时不销毁
                Instance = go.AddComponent<HotUpdateManager>();
            }
        }

        /// <summary>
        /// 设置热更新配置
        /// 必须在检查更新之前调用
        /// </summary>
        /// <param name="config">热更新配置对象</param>
        public void SetConfig(HotUpdateConfig config)
        {
            Config = config;
        }

        /// <summary>
        /// 检查服务器是否有新版本
        /// 异步执行，通过回调返回结果
        /// </summary>
        /// <param name="hasUpdate">回调参数：true表示有更新，false表示无更新或失败</param>
        public void CheckUpdate(Action<bool> hasUpdate)
        {
            StartCoroutine(CheckUpdateCoroutine(hasUpdate));
        }

        /// <summary>
        /// 检查更新的协程 - 游戏公司标准两阶段检查
        /// 流程：
        /// 1. 加载本地版本信息
        /// 2. 获取根 version.json（获取当前版本号）
        /// 3. 获取 {version}/manifest.json（获取该版本的资源清单）
        /// 4. 对比版本号判断是否需要更新
        /// </summary>
        private IEnumerator CheckUpdateCoroutine(Action<bool> hasUpdate)
        {
            // 加载本地版本
            LocalVersion = LoadLocalVersion();
            bool isFirstLaunch = (LocalVersion == null);
            
            // 首次启动时创建默认版本
            if (isFirstLaunch)
            {
                LocalVersion = new VersionInfo
                {
                    Version = "0.0.0",
                    ResVersion = "0.0.0",
                    AssetBundles = new List<AssetBundleInfo>()
                };
            }

            string serverUrl = Config.ServerUrl.TrimEnd('/');
            string currentVersion = null;

            // ========== 第一阶段：获取根 version.json ==========
            string rootJsonText = null;
            
            if (Config.UseLocalFile)
            {
                string localPath = ResolveLocalPath(Config.LocalVersionPath);
                if (!File.Exists(localPath))
                {
                    OnError?.Invoke($"本地版本文件不存在: {localPath}");
                    hasUpdate?.Invoke(false);
                    yield break;
                }
                rootJsonText = File.ReadAllText(localPath);
            }
            else
            {
                string url = $"{serverUrl}/{Config.VersionFileName}";
                UnityEngine.Debug.Log($"[HotUpdate] 请求根版本: {url}");
                
                using (var request = UnityWebRequest.Get(url))
                {
                    request.timeout = Config.Timeout;
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        string errorMsg;
                        if (request.result == UnityWebRequest.Result.ConnectionError)
                        {
                            errorMsg = $"无法连接到服务器，请检查：\n" +
                                      $"1. 服务器是否已启动？\n" +
                                      $"2. URL是否正确？({url})\n" +
                                      $"3. 防火墙是否阻止了连接？\n" +
                                      $"原始错误: {request.error}";
                        }
                        else if (request.result == UnityWebRequest.Result.ProtocolError)
                        {
                            errorMsg = $"服务器返回错误: HTTP {request.responseCode}，请检查URL是否正确";
                        }
                        else if (request.result == UnityWebRequest.Result.DataProcessingError)
                        {
                            errorMsg = $"数据处理错误: {request.error}";
                        }
                        else
                        {
                            errorMsg = $"检查更新失败: {request.error}";
                        }
                        UnityEngine.Debug.LogError($"[HotUpdate] {errorMsg}");
                        OnError?.Invoke(errorMsg);
                        hasUpdate?.Invoke(false);
                        yield break;
                    }
                    rootJsonText = request.downloadHandler.text;
                }
            }

            // 解析根版本信息
            RootVersionInfo rootInfo;
            try
            {
                rootInfo = JsonUtility.FromJson<RootVersionInfo>(rootJsonText);
                currentVersion = rootInfo.CurrentVersion;
                _currentServerVersion = currentVersion;
                UnityEngine.Debug.Log($"[HotUpdate] 服务器当前版本: {currentVersion}");
            }
            catch (Exception e)
            {
                OnError?.Invoke($"解析根版本信息失败: {e.Message}");
                hasUpdate?.Invoke(false);
                yield break;
            }

            if (string.IsNullOrEmpty(currentVersion))
            {
                OnError?.Invoke("根版本信息中没有CurrentVersion");
                hasUpdate?.Invoke(false);
                yield break;
            }

            // 对比版本号
            bool needUpdate = currentVersion != LocalVersion.ResVersion;
            UnityEngine.Debug.Log($"[HotUpdate] 本地版本: {LocalVersion.ResVersion}, 服务器版本: {currentVersion}, 需要更新: {needUpdate}");

            if (!needUpdate)
            {
                hasUpdate?.Invoke(false);
                yield break;
            }

            // ========== 第二阶段：获取该版本的 manifest.json ==========
            string manifestJsonText = null;
            string manifestPath = rootInfo.ManifestPath.Replace("{version}", currentVersion);
            string manifestUrl = $"{serverUrl}{manifestPath}";
            
            UnityEngine.Debug.Log($"[HotUpdate] 请求清单: {manifestUrl}");

            if (Config.UseLocalFile)
            {
                string localManifestPath = ResolveLocalPath($"{currentVersion}/manifest.json");
                if (!File.Exists(localManifestPath))
                {
                    OnError?.Invoke($"本地清单文件不存在: {localManifestPath}");
                    hasUpdate?.Invoke(false);
                    yield break;
                }
                manifestJsonText = File.ReadAllText(localManifestPath);
            }
            else
            {
                using (var request = UnityWebRequest.Get(manifestUrl))
                {
                    request.timeout = Config.Timeout;
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        OnError?.Invoke($"获取清单失败: {request.error}");
                        hasUpdate?.Invoke(false);
                        yield break;
                    }
                    manifestJsonText = request.downloadHandler.text;
                }
            }

            // 解析清单为服务器版本信息
            try
            {
                ServerVersion = JsonUtility.FromJson<VersionInfo>(manifestJsonText);
                hasUpdate?.Invoke(true);
            }
            catch (Exception e)
            {
                OnError?.Invoke($"解析清单失败: {e.Message}");
                hasUpdate?.Invoke(false);
            }
        }

        /// <summary>
        /// 解析本地路径（支持相对StreamingAssets或绝对路径）
        /// </summary>
        private string ResolveLocalPath(string path)
        {
            // 如果是绝对路径，直接返回
            if (Path.IsPathRooted(path))
            {
                return path;
            }
            // 否则相对于StreamingAssets
            return Path.Combine(Application.streamingAssetsPath, path);
        }

        /// <summary>
        /// 开始执行热更新
        /// 需要先调用CheckUpdate确认有新版本
        /// </summary>
        public void StartUpdate()
        {
            // 防止重复触发
            if (_isUpdating) return;
            
            // 检查是否已完成版本检查
            if (ServerVersion == null || LocalVersion == null)
            {
                OnError?.Invoke("请先检查更新");
                return;
            }
            
            _isUpdating = true;
            StartCoroutine(UpdateCoroutine());
        }

        /// <summary>
        /// 更新主协程
        /// 流程：
        /// 1. 对比本地和服务器的AB包列表
        /// 2. 找出需要下载的包（新增或Hash变化）
        /// 3. 递归收集依赖包
        /// 4. 创建下载任务列表
        /// 5. 并发下载所有任务
        /// 6. 更新本地版本信息
        /// </summary>
        private IEnumerator UpdateCoroutine()
        {
            // 清空上一次的下载任务
            _downloadTasks.Clear();
            _completedTasks = 0;

            // 构建本地AB包索引（以名称为Key）
            var localBundles = new Dictionary<string, AssetBundleInfo>();
            foreach (var ab in LocalVersion.AssetBundles)
            {
                localBundles[ab.Name] = ab;
            }

            // 收集需要下载的包（新增或Hash不同）
            var needDownload = new Dictionary<string, AssetBundleInfo>();
            foreach (var serverAb in ServerVersion.AssetBundles)
            {
                // 判断条件：本地不存在 或 Hash不同（内容有更新）
                bool need = !localBundles.ContainsKey(serverAb.Name) || 
                           localBundles[serverAb.Name].Hash != serverAb.Hash;
                if (need)
                {
                    needDownload[serverAb.Name] = serverAb;
                    // 递归收集依赖包（即使依赖包本身不需要更新也要下载）
                    CollectDependencies(serverAb, serverAb.Dependencies, needDownload, ServerVersion);
                }
            }

            // 如果没有需要下载的包，直接完成
            if (needDownload.Count == 0)
            {
                SaveLocalVersion(ServerVersion);
                OnUpdateComplete?.Invoke(true);
                _isUpdating = false;
                yield break;
            }

            // 创建下载任务列表
            foreach (var kvp in needDownload)
            {
                string serverUrl = Config.ServerUrl.TrimEnd('/');
                // 优先使用version.json中指定的URL，否则使用默认路径
                string url = string.IsNullOrEmpty(kvp.Value.Url) 
                    ? $"{serverUrl}/{Config.AssetBundleFolder}/{kvp.Value.Name}"
                    : kvp.Value.Url;
                
                // 创建下载任务
                _downloadTasks.Add(new DownloadTask
                {
                    TaskId = Guid.NewGuid().ToString(),  // 唯一任务ID
                    BundleInfo = kvp.Value,              // AB包信息
                    Url = url,                           // 下载URL
                    LocalPath = Path.Combine(Application.persistentDataPath, kvp.Value.Name),  // 本地保存路径
                    TotalBytes = kvp.Value.Size,         // 文件大小（用于进度显示）
                    State = DownloadState.Pending,       // 初始状态：等待中
                    RetryCount = 0                       // 重试次数
                });
            }

            _totalTasks = _downloadTasks.Count;
            UnityEngine.Debug.Log($"[HotUpdate] 需要下载的包数量: {_totalTasks}");

            // 执行并发下载
            yield return StartCoroutine(ConcurrentDownloadCoroutine());

            // 检查下载结果
            bool allSuccess = _downloadTasks.All(t => t.State == DownloadState.Completed);
            if (allSuccess)
            {
                // 全部成功：保存本地版本
                SaveLocalVersion(ServerVersion);
                OnUpdateComplete?.Invoke(true);
            }
            else
            {
                // 有失败：统计失败数量
                int failedCount = _downloadTasks.Count(t => t.State == DownloadState.Failed);
                OnError?.Invoke($"有 {failedCount} 个包下载失败");
                OnUpdateComplete?.Invoke(false);
            }

            _isUpdating = false;
        }

        /// <summary>
        /// 并发下载协程
        /// 控制最大并发数，同时下载多个AB包
        /// </summary>
        private IEnumerator ConcurrentDownloadCoroutine()
        {
            _isDownloading = true;
            int maxConcurrency = Config.MaxConcurrentDownloads;  // 最大并发数
            int runningCount = 0;

            // 遍历所有待下载任务
            foreach (var task in _downloadTasks)
            {
                // 等待有空闲的下载线程
                while (runningCount >= maxConcurrency)
                {
                    yield return null;
                    // 统计当前正在下载的任务数
                    runningCount = _downloadTasks.Count(t => t.State == DownloadState.Downloading);
                }

                // 启动下载
                if (task.State == DownloadState.Pending)
                {
                    StartCoroutine(DownloadSingleTaskCoroutine(task));
                    runningCount++;
                }
            }

            // 等待所有任务完成（包括成功、失败、取消）
            while (_downloadTasks.Any(t => t.State == DownloadState.Downloading || t.State == DownloadState.Pending))
            {
                yield return null;
            }

            _isDownloading = false;
        }

        /// <summary>
        /// 单个任务下载协程（含重试机制）
        /// </summary>
        /// <param name="task">要下载的任务</param>
        private IEnumerator DownloadSingleTaskCoroutine(DownloadTask task)
        {
            task.State = DownloadState.Downloading;

            // 重试循环：最多重试MaxRetryCount次
            for (int retry = 0; retry <= Config.MaxRetryCount; retry++)
            {
                if (retry > 0)
                {
                    task.RetryCount = retry;
                    // 重试前等待指定时间
                    yield return new WaitForSeconds(Config.RetryInterval / 1000f);
                }

                // 执行下载
                yield return StartCoroutine(DownloadFileCoroutine(task));

                // 判断下载结果
                if (task.State == DownloadState.Completed)
                {
                    OnTaskComplete?.Invoke(task);
                    break;
                }
                else if (task.State == DownloadState.Failed && retry == Config.MaxRetryCount)
                {
                    // 最后一次重试仍失败
                    OnError?.Invoke($"下载失败 {task.BundleInfo.Name}: {task.ErrorMessage}");
                }
            }

            // 更新总体进度
            UpdateGlobalProgress();
        }

        /// <summary>
        /// 下载单个文件的协程
        /// 支持：HTTP下载 或 本地文件复制
        /// </summary>
        private IEnumerator DownloadFileCoroutine(DownloadTask task)
        {
            byte[] data = null;

            if (Config.UseLocalFile)
            {
                // 本地模式：直接复制文件
                string sourcePath = ResolveLocalPath(Path.Combine(Config.LocalAssetBundlePath, task.BundleInfo.Name));
                
                if (!File.Exists(sourcePath))
                {
                    task.State = DownloadState.Failed;
                    task.ErrorMessage = $"源文件不存在: {sourcePath}";
                    yield break;
                }

                // 模拟进度（本地复制很快）
                float progress = 0;
                while (progress < 1f)
                {
                    progress += 0.2f;
                    progress = Mathf.Min(progress, 1f);
                    task.DownloadedBytes = (long)(task.TotalBytes * progress);
                    OnSingleTaskProgress?.Invoke(task.BundleInfo.Name, progress);
                    yield return new WaitForSeconds(0.05f);  // 模拟下载延迟
                }

                data = File.ReadAllBytes(sourcePath);
            }
            else
            {
                // HTTP模式：从服务器下载
                using (var request = new UnityWebRequest(task.Url))
                {
                    request.timeout = Config.Timeout;
                    request.downloadHandler = new DownloadHandlerBuffer();

                    float lastProgress = 0;
                    request.SendWebRequest();

                    while (!request.isDone)
                    {
                        // 使用 downloadedBytes 属性，避免 data 为 null
                        task.DownloadedBytes = (long)request.downloadedBytes;
                        if (task.TotalBytes > 0)
                        {
                            float progress = (float)task.DownloadedBytes / task.TotalBytes;
                            if (progress - lastProgress > 0.01f)
                            {
                                lastProgress = progress;
                                OnSingleTaskProgress?.Invoke(task.BundleInfo.Name, progress);
                            }
                        }
                        yield return null;
                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        task.State = DownloadState.Failed;
                        task.ErrorMessage = request.error;
                        yield break;
                    }

                    data = request.downloadHandler.data;
                }
            }

            // 解密（如果需要）
            if (Config.UseEncryption && task.BundleInfo.IsEncrypted && data != null)
            {
                data = DecryptData(data, Config.EncryptionKey);
            }

            // Hash验证
            if (data != null && !string.IsNullOrEmpty(task.BundleInfo.Hash))
            {
                string actualHash = ComputeHash(data);
                if (actualHash != task.BundleInfo.Hash)
                {
                    task.State = DownloadState.Failed;
                    task.ErrorMessage = "文件Hash验证失败";
                    yield break;
                }
            }

            // 保存到本地
            try
            {
                string dir = Path.GetDirectoryName(task.LocalPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                
                File.WriteAllBytes(task.LocalPath, data);
                task.DownloadedBytes = data.Length;
                task.State = DownloadState.Completed;
            }
            catch (Exception e)
            {
                task.State = DownloadState.Failed;
                task.ErrorMessage = e.Message;
            }
        }

        /// <summary>
        /// 递归收集依赖包
        /// 从指定的AB包开始，递归收集所有依赖的AB包
        /// </summary>
        /// <param name="bundle">AB包信息</param>
        /// <param name="dependencies">依赖的包名列表</param>
        /// <param name="needDownload">需要下载的包集合（引用传递）</param>
        /// <param name="version">服务器版本信息（用于查找依赖包详情）</param>
        private void CollectDependencies(AssetBundleInfo bundle, List<string> dependencies, 
            Dictionary<string, AssetBundleInfo> needDownload, VersionInfo version)
        {
            // 没有依赖则直接返回
            if (dependencies == null || dependencies.Count == 0) return;

            foreach (string depName in dependencies)
            {
                // 避免重复收集
                if (!needDownload.ContainsKey(depName))
                {
                    // 在服务器版本中查找依赖包的详细信息
                    var depInfo = version.AssetBundles.FirstOrDefault(b => b.Name == depName);
                    if (depInfo != null)
                    {
                        needDownload[depName] = depInfo;
                        // 递归收集依赖的依赖
                        CollectDependencies(depInfo, depInfo.Dependencies, needDownload, version);
                    }
                }
            }
        }

        /// <summary>
        /// 更新全局下载进度
        /// 计算已完成任务数占总任务数的比例
        /// </summary>
        private void UpdateGlobalProgress()
        {
            _completedTasks = _downloadTasks.Count(t => t.State == DownloadState.Completed);
            float progress = _totalTasks > 0 ? (float)_completedTasks / _totalTasks : 0;
            OnDownloadProgress?.Invoke(progress);
        }

        /// <summary>
        /// 计算数据的MD5 Hash值
        /// 用于验证下载文件的完整性
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <returns>MD5 Hash字符串（32位小写十六进制）</returns>
        private string ComputeHash(byte[] data)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(data);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// 解密数据（简化版XOR解密）
        /// 实际项目建议使用AES加密
        /// </summary>
        /// <param name="encryptedData">加密数据</param>
        /// <param name="key">密钥</param>
        /// <returns>解密后的数据</returns>
        private byte[] DecryptData(byte[] encryptedData, string key)
        {
            // 使用XOR解密（简单示例，生产环境请使用AES等加密算法）
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] result = new byte[encryptedData.Length];
            for (int i = 0; i < encryptedData.Length; i++)
            {
                result[i] = (byte)(encryptedData[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return result;
        }

        /// <summary>
        /// 判断用户是否在灰度更新范围内
        /// 支持两种灰度方式：
        /// 1. 按比例灰度（通过用户ID的Hash值确定）
        /// 2. 指定用户灰度（白名单）
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>true表示该用户应该更新</returns>
        public bool IsGrayScaleUser(string userId)
        {
            // 灰度100%：全量更新
            if (Config.GrayScalePercent >= 100) return true;
            // 灰度0%：不更新
            if (Config.GrayScalePercent <= 0) return false;
            // 白名单用户直接通过
            if (Config.GrayScaleUserIds.Contains(userId)) return true;
            
            // 按比例灰度：通过用户ID的Hash值判断
            int hash = Math.Abs(userId?.GetHashCode() ?? 0);
            return hash % 100 < Config.GrayScalePercent;
        }

        /// <summary>
        /// 清理本地缓存
        /// 删除所有已下载的AB包文件，保留版本信息文件
        /// </summary>
        public void ClearCache()
        {
            string cachePath = Application.persistentDataPath;
            if (Directory.Exists(cachePath))
            {
                try
                {
                    var files = Directory.GetFiles(cachePath);
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileName(file);
                        // 保留版本信息文件
                        if (fileName != Config.VersionFileName)
                        {
                            File.Delete(file);
                        }
                    }
                    // 重置版本信息
                    LocalVersion = null;
                    ServerVersion = null;
                    UnityEngine.Debug.Log("[HotUpdate] 缓存已清理");
                }
                catch (Exception e)
                {
                    OnError?.Invoke($"清理缓存失败: {e.Message}");
                }
            }
        }

        /// <summary>
        /// 取消正在进行的下载
        /// 将所有下载中或等待中的任务标记为取消状态
        /// </summary>
        public void CancelDownload()
        {
            if (_isDownloading)
            {
                foreach (var task in _downloadTasks)
                {
                    if (task.State == DownloadState.Downloading || task.State == DownloadState.Pending)
                    {
                        task.State = DownloadState.Cancelled;
                    }
                }
            }
        }

        /// <summary>
        /// 获取本地已下载的AB包列表
        /// </summary>
        /// <returns>AB包信息字典（Key为包名）</returns>
        public Dictionary<string, AssetBundleInfo> GetLocalBundles()
        {
            var result = new Dictionary<string, AssetBundleInfo>();
            if (LocalVersion != null)
            {
                foreach (var ab in LocalVersion.AssetBundles)
                {
                    result[ab.Name] = ab;
                }
            }
            return result;
        }

        /// <summary>
        /// 获取服务器上的AB包列表
        /// </summary>
        /// <returns>AB包信息字典（Key为包名）</returns>
        public Dictionary<string, AssetBundleInfo> GetServerBundles()
        {
            var result = new Dictionary<string, AssetBundleInfo>();
            if (ServerVersion != null)
            {
                foreach (var ab in ServerVersion.AssetBundles)
                {
                    result[ab.Name] = ab;
                }
            }
            return result;
        }

        /// <summary>
        /// 加载本地版本信息
        /// 从persistentDataPath读取version.json文件
        /// </summary>
        /// <returns>本地版本信息，不存在返回null</returns>
        private VersionInfo LoadLocalVersion()
        {
            string localVersionFile = Path.Combine(Application.persistentDataPath, Config.VersionFileName);
            if (File.Exists(localVersionFile))
            {
                try
                {
                    string json = File.ReadAllText(localVersionFile);
                    return JsonUtility.FromJson<VersionInfo>(json);
                }
                catch (Exception e)
                {
                    OnError?.Invoke($"读取本地版本失败: {e.Message}");
                }
            }
            return null;
        }

        /// <summary>
        /// 保存版本信息到本地
        /// 更新成功后调用，记录最新的版本状态
        /// </summary>
        /// <param name="version">要保存的版本信息</param>
        private void SaveLocalVersion(VersionInfo version)
        {
            string localVersionFile = Path.Combine(Application.persistentDataPath, Config.VersionFileName);
            try
            {
                // 确保保存正确的版本号
                if (!string.IsNullOrEmpty(_currentServerVersion))
                {
                    version.Version = _currentServerVersion;
                    version.ResVersion = _currentServerVersion;
                }
                string json = JsonUtility.ToJson(version, true);  // 格式化JSON
                File.WriteAllText(localVersionFile, json);
                UnityEngine.Debug.Log($"[HotUpdate] 本地版本已保存: {version.ResVersion}");
            }
            catch (Exception e)
            {
                OnError?.Invoke($"保存版本信息失败: {e.Message}");
            }
        }
    }
}
