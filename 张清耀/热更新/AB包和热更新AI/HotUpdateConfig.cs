using System;
using System.Collections.Generic;

namespace HotUpdate
{
    /// <summary>
    /// 版本信息类 - 描述当前版本的所有资源包信息
    /// 游戏公司版本：支持依赖关系、分片信息
    /// </summary>
    [Serializable]
    public class VersionInfo
    {
        /// <summary>版本号（用于显示给用户）</summary>
        public string Version = "1.0.0";

        /// <summary>资源版本号（用于判断是否需要更新）</summary>
        public string ResVersion = "1.0.0";

        /// <summary>当前版本包含的所有AB包列表</summary>
        public List<AssetBundleInfo> AssetBundles = new List<AssetBundleInfo>();

        /// <summary>版本发布时间</summary>
        public long Timestamp;
    }

    /// <summary>
    /// AB包信息类 - 描述单个AssetBundle的详细信息
    /// 游戏公司版本：支持依赖关系、分片下载、加密
    /// </summary>
    [Serializable]
    public class AssetBundleInfo
    {
        /// <summary>AB包文件名</summary>
        public string Name;

        /// <summary>文件MD5哈希值 - 用于判断文件是否变化</summary>
        public string Hash;

        /// <summary>文件大小（字节）</summary>
        public long Size;

        /// <summary>下载URL路径</summary>
        public string Url;

        /// <summary>依赖的其他AB包名称列表</summary>
        public List<string> Dependencies = new List<string>();

        /// <summary>是否需要加密</summary>
        public bool IsEncrypted;

        /// <summary>分片大小（0表示不分片，单位：字节）</summary>
        public int ShardSize;

        /// <summary>文件类型: 0=普通, 1=场景, 2=资源包</summary>
        public int Type;

        /// <summary>资源标签（用于按需加载）</summary>
        public List<string> Tags = new List<string>();
    }

    /// <summary>
    /// 热更新配置类 - 游戏公司版本
    /// </summary>
    [Serializable]
    public class HotUpdateConfig
    {
        /// <summary>更新服务器地址</summary>
        public string ServerUrl = "http://localhost:8080/";

        /// <summary>版本信息文件名</summary>
        public string VersionFileName = "version.json";

        /// <summary>AB包存放的子文件夹名称</summary>
        public string AssetBundleFolder = "AssetBundles";

        /// <summary>网络请求超时时间（秒）</summary>
        public int Timeout = 60;

        /// <summary>下载请求超时时间（秒）</summary>
        public int DownloadTimeout = 300;

        /// <summary>最大重试次数</summary>
        public int MaxRetryCount = 3;

        /// <summary>重试间隔（毫秒）</summary>
        public int RetryInterval = 1000;

        /// <summary>最大并发下载数</summary>
        public int MaxConcurrentDownloads = 3;

        /// <summary>单个AB包最大分片大小（字节），0=不分片</summary>
        public int ShardSize = 5 * 1024 * 1024;  // 5MB

        /// <summary>是否使用加密传输</summary>
        public bool UseEncryption = false;

        /// <summary>加密密钥（16字节）</summary>
        public string EncryptionKey = "1234567890123456";

        /// <summary>是否允许WiFi下自动更新</summary>
        public bool WifiOnly = false;

        /// <summary>是否显示下载进度</summary>
        public bool ShowProgress = true;

        /// <summary>本地缓存最大大小（字节），0=不限</summary>
        public long MaxCacheSize = 0;

        /// <summary>自动清理过期缓存</summary>
        public bool AutoCleanCache = true;

        /// <summary>缓存有效期（秒）</summary>
        public int CacheExpireTime = 86400 * 7;  // 7天

        /// <summary>灰度更新比例（0-100，0=全量更新）</summary>
        public int GrayScalePercent = 100;

        /// <summary>灰度更新用户ID列表</summary>
        public List<string> GrayScaleUserIds = new List<string>();

        /// <summary>使用本地文件模式（测试用，不需要HTTP服务器）</summary>
        public bool UseLocalFile = false;

        /// <summary>本地version.json路径（相对StreamingAssets或绝对路径）</summary>
        public string LocalVersionPath = "version.json";

        /// <summary>本地AB包文件夹路径</summary>
        public string LocalAssetBundlePath = "AssetBundles";
    }

    /// <summary>
    /// 根版本信息 - 从version.json获取，指向当前版本
    /// </summary>
    [Serializable]
    public class RootVersionInfo
    {
        /// <summary>当前版本号</summary>
        public string CurrentVersion;

        /// <summary>服务器地址</summary>
        public string ServerUrl;

        /// <summary>manifest.json路径模板，{version}会被替换</summary>
        public string ManifestPath = "/{version}/manifest.json";

        /// <summary>更新时间</summary>
        public string UpdateTime;
    }

    /// <summary>
    /// 下载任务信息
    /// </summary>
    public class DownloadTask
    {
        /// <summary>任务ID</summary>
        public string TaskId;

        /// <summary>AB包信息</summary>
        public AssetBundleInfo BundleInfo;

        /// <summary>下载URL</summary>
        public string Url;

        /// <summary>本地保存路径</summary>
        public string LocalPath;

        /// <summary>已下载字节数</summary>
        public long DownloadedBytes;

        /// <summary>总字节数</summary>
        public long TotalBytes;

        /// <summary>下载状态</summary>
        public DownloadState State;

        /// <summary>重试次数</summary>
        public int RetryCount;

        /// <summary>错误信息</summary>
        public string ErrorMessage;

        /// <summary>下载速度（字节/秒）</summary>
        public long Speed;
    }

    /// <summary>
    /// 下载状态枚举
    /// </summary>
    public enum DownloadState
    {
        Pending,      // 等待中
        Downloading,  // 下载中
        Completed,    // 已完成
        Failed,       // 失败
        Cancelled     // 已取消
    }
}
