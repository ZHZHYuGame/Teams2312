using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using HotUpdate;

/// <summary>
/// 热更新测试脚本 - 游戏公司版本
/// 测试功能:
/// 1. 版本检查与更新
/// 2. 多线程下载
/// 3. 依赖包加载
/// 4. 引用计数管理
/// 5. 日志系统
/// </summary>
public class HotUpdateTest : MonoBehaviour
{
    [Header("服务器配置")]
    [Tooltip("服务器地址")]
    public string serverUrl = "http://127.0.0.1:9999/";
    
    [Tooltip("版本信息文件名")]
    public string versionFileName = "version.json";
    
    [Tooltip("AB包文件夹名")]
    public string assetBundleFolder = "AssetBundles";
    
    [Tooltip("超时时间（秒）")]
    public int timeout = 30;

    [Header("模式选择")]
    [Tooltip("使用本地文件模式（勾选=本地，不勾选=HTTP）")]
    public bool useLocalFile = false;
    
    [Tooltip("本地version.json路径（相对于StreamingAssets）")]
    public string localVersionPath = "TestHotUpdate/version.json";
    
    [Tooltip("本地AB包文件夹路径（相对于StreamingAssets）")]
    public string localAssetBundlePath = "TestHotUpdate/AssetBundles";

    [Header("下载配置")]
    [Tooltip("最大重试次数")]
    public int maxRetryCount = 3;
    
    [Tooltip("最大并发下载数")]
    public int maxConcurrentDownloads = 3;

    [Header("UI显示")]
    public Text statusText;
    public Text detailText;
    public Slider progressBar;
    public Text progressText;

    [Header("测试的AB包")]
    [Tooltip("测试加载的AB包名称列表")]
    public string[] testBundleNames = { "cube.u3d", "111.u3d" };
    
    [Tooltip("AB包内的资源名称列表（与testBundleNames对应）")]
    public string[] testAssetNames = { "Cube", "111" };

    private void Start()
    {
        InitHotUpdate();
    }

    private void InitHotUpdate()
    {
        // 初始化日志
        HotUpdateLogger.Initialize();
        HotUpdateLogger.Info("热更新测试初始化");
        
        // 创建管理器
        HotUpdateManager.Create();
        AssetBundleLoader.Create();
        
        // 配置
        var config = new HotUpdateConfig
        {
            ServerUrl = serverUrl,
            VersionFileName = versionFileName,
            AssetBundleFolder = assetBundleFolder,
            Timeout = timeout,
            MaxRetryCount = maxRetryCount,
            MaxConcurrentDownloads = maxConcurrentDownloads,
            ShowProgress = true,
            UseLocalFile = useLocalFile,
            LocalVersionPath = localVersionPath,
            LocalAssetBundlePath = localAssetBundlePath
        };
        HotUpdateManager.Instance.SetConfig(config);
        
        // 监听事件
        HotUpdateManager.Instance.OnDownloadProgress += OnDownloadProgress;
        HotUpdateManager.Instance.OnUpdateComplete += OnUpdateComplete;
        HotUpdateManager.Instance.OnError += OnError;
        HotUpdateManager.Instance.OnTaskComplete += OnTaskComplete;
        HotUpdateManager.Instance.OnSingleTaskProgress += OnSingleTaskProgress;
        
        AssetBundleLoader.Instance.OnBundleLoaded += OnBundleLoaded;
        AssetBundleLoader.Instance.OnBundleUnloaded += OnBundleUnloaded;
        
        UpdateStatus("热更新管理器已初始化");
        UpdateDetail("点击按钮开始测试");
    }

    #region UI按钮回调

    public void OnClickCheckUpdate()
    {
        UpdateStatus("正在检查更新...");
        UpdateDetail("请求服务器版本信息...");
        
        HotUpdateManager.Instance.CheckUpdate(hasUpdate =>
        {
            if (hasUpdate)
            {
                UpdateStatus("发现新版本！");
                UpdateDetail($"服务器版本: {HotUpdateManager.Instance.ServerVersion?.ResVersion}");
            }
            else
            {
                UpdateStatus("已是最新版本");
                UpdateDetail($"当前版本: {HotUpdateManager.Instance.LocalVersion?.ResVersion}");
            }
        });
    }

    public void OnClickStartUpdate()
    {
        UpdateStatus("开始下载更新...");
        UpdateDetail($"并发数: {maxConcurrentDownloads}, 重试次数: {maxRetryCount}");
        HotUpdateManager.Instance.StartUpdate();
    }

    public void OnClickCancelDownload()
    {
        HotUpdateManager.Instance.CancelDownload();
        UpdateStatus("已取消下载");
    }

    public void OnClickLoadAsset()
    {
        StartCoroutine(LoadAssetBundleCoroutine());
    }

    public void OnClickLoadAssetWithDependencies()
    {
        UpdateStatus("加载资源（含依赖）...");
        StartCoroutine(LoadWithDependenciesCoroutine());
    }

    public void OnClickReleaseAsset()
    {
        if (testBundleNames != null && testBundleNames.Length > 0)
        {
            AssetBundleLoader.Instance.ReleaseBundle(testBundleNames[0]);
            UpdateStatus($"释放AB包: {testBundleNames[0]}");
        }
    }

    public void OnClickUnloadAll()
    {
        AssetBundleLoader.Instance.UnloadAllBundles();
        UpdateStatus("已卸载所有AB包");
        UpdateDetail("内存已释放");
    }

    public void OnClickShowLoadedInfo()
    {
        int count = AssetBundleLoader.Instance.GetLoadedBundleCount();
        long memory = AssetBundleLoader.Instance.GetMemoryUsage();
        var bundles = HotUpdateManager.Instance.GetLocalBundles();
        
        string info = $"已加载AB包: {count} 个\n";
        info += $"内存使用（估算）: {FormatSize(memory)}\n";
        info += $"本地版本包数量: {bundles.Count}\n";
        
        foreach (var name in bundles.Keys)
        {
            int refCount = AssetBundleLoader.Instance.GetRefCount(name);
            info += $"  - {name} (引用: {refCount})\n";
        }
        
        UpdateDetail(info);
    }

    public void OnClickClearCache()
    {
        HotUpdateManager.Instance.ClearCache();
        AssetBundleLoader.Instance.UnloadAllBundles();
        UpdateStatus("缓存已清理");
        UpdateDetail("所有本地AB包和版本信息已删除");
    }

    public void OnClickExportLogs()
    {
        string exportPath = Path.Combine(Application.persistentDataPath, $"hotupdate_logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
        HotUpdateLogger.ExportLogs(exportPath);
        UpdateStatus($"日志已导出");
        UpdateDetail($"路径: {exportPath}");
    }

    #endregion

    #region 事件回调

    private void OnDownloadProgress(float progress)
    {
        if (progressBar != null)
        {
            progressBar.value = progress;
        }
        if (progressText != null)
        {
            progressText.text = $"总进度: {progress * 100:F1}%";
        }
        UpdateStatus($"下载进度: {progress * 100:F1}%");
    }

    private void OnSingleTaskProgress(string bundleName, float progress)
    {
        if (progressText != null)
        {
            progressText.text = $"{bundleName}: {progress * 100:F1}%";
        }
    }

    private void OnUpdateComplete(bool success)
    {
        if (success)
        {
            UpdateStatus("热更新完成！");
            UpdateDetail("点击加载资源测试");
        }
        else
        {
            UpdateStatus("热更新失败");
            UpdateDetail("请查看错误日志");
        }
    }

    private void OnTaskComplete(DownloadTask task)
    {
        UpdateDetail($"完成下载: {task.BundleInfo.Name} ({FormatSize(task.TotalBytes)})");
    }

    private void OnError(string error)
    {
        UpdateStatus($"错误: {error}");
        UpdateDetail($"时间: {DateTime.Now:HH:mm:ss}");
        UnityEngine.Debug.LogError($"[HotUpdateTest] {error}");
    }

    private void OnBundleLoaded(string bundleName, AssetBundle bundle)
    {
        UpdateDetail($"已加载: {bundleName}");
    }

    private void OnBundleUnloaded(string bundleName)
    {
        UpdateDetail($"已卸载: {bundleName}");
    }

    #endregion

    #region 加载AB包

    private IEnumerator LoadAssetBundleCoroutine()
    {
        if (testBundleNames == null || testBundleNames.Length == 0)
        {
            UpdateStatus("没有配置要加载的AB包");
            yield break;
        }

        int loadedCount = 0;
        var loadedAssets = new List<GameObject>();
        
        for (int i = 0; i < testBundleNames.Length; i++)
        {
            string bundleName = testBundleNames[i];
            string assetName = testAssetNames[i];
            
            UpdateStatus($"正在加载 {bundleName}... ({i + 1}/{testBundleNames.Length})");
            
            GameObject asset = null;
            bool done = false;
            
            AssetBundleLoader.Instance.LoadAssetAsync<GameObject>(bundleName, assetName, obj =>
            {
                asset = obj;
                done = true;
            });
            
            yield return new WaitUntil(() => done);
            
            if (asset != null)
            {
                Vector3 pos = new Vector3(i * 3, 0, 0);
                var obj = Instantiate(asset, pos, Quaternion.identity);
                loadedAssets.Add(obj);
                loadedCount++;
                UpdateDetail($"加载成功: {assetName} → 位置: {pos}");
            }
            else
            {
                UpdateDetail($"加载失败: {bundleName}");
            }
        }
        
        UpdateStatus($"加载完成！成功 {loadedCount}/{testBundleNames.Length}");
        UpdateDetail($"已实例化 {loadedAssets.Count} 个对象");
    }

    private IEnumerator LoadWithDependenciesCoroutine()
    {
        if (testBundleNames == null || testBundleNames.Length == 0)
        {
            UpdateStatus("没有配置要加载的AB包");
            yield break;
        }

        string firstBundle = testBundleNames[0];
        bool done = false;
        
        AssetBundleLoader.Instance.LoadBundleWithDependencies(firstBundle, bundle =>
        {
            done = true;
        });
        
        yield return new WaitUntil(() => done);
        
        int refCount = AssetBundleLoader.Instance.GetRefCount(firstBundle);
        UpdateStatus($"加载完成: {firstBundle}");
        UpdateDetail($"引用计数: {refCount}");
    }

    #endregion

    #region 辅助方法

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        UnityEngine.Debug.Log($"[HotUpdateTest] {message}");
    }

    private void UpdateDetail(string message)
    {
        if (detailText != null)
        {
            detailText.text = message;
        }
    }

    private string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    #endregion
}
