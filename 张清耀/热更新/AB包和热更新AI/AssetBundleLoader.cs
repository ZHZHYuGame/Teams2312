using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HotUpdate
{
    /// <summary>
    /// AB包资源加载器 - 游戏公司版本
    /// 功能:
    /// 1. 引用计数管理
    /// 2. 依赖包自动加载
    /// 3. 内存缓存
    /// 4. 异步加载
    /// 5. 智能卸载
    /// </summary>
    public class AssetBundleLoader : MonoBehaviour
    {
        public static AssetBundleLoader Instance { get; private set; }

        // 已加载的AB包缓存
        private readonly Dictionary<string, LoadedBundle> _loadedBundles = new Dictionary<string, LoadedBundle>();
        
        // 正在加载的任务
        private readonly Dictionary<string, List<Action<AssetBundle>> > _loadingCallbacks = new Dictionary<string, List<Action<AssetBundle>>>();

        // 事件
        public event Action<string, AssetBundle> OnBundleLoaded;
        public event Action<string> OnBundleUnloaded;
        public event Action<string, float> OnLoadProgress;

        /// <summary>
        /// 已加载的AB包信息
        /// </summary>
        private class LoadedBundle
        {
            public AssetBundle Bundle;
            public int RefCount;
            public string BundleName;
            public List<string> Dependencies = new List<string>();
            public DateTime LoadTime;
            public long MemorySize;
        }

        public static void Create()
        {
            if (Instance == null)
            {
                var go = new GameObject("[AssetBundleLoader]");
                DontDestroyOnLoad(go);
                Instance = go.AddComponent<AssetBundleLoader>();
            }
        }

        /// <summary>
        /// 同步加载AB包
        /// </summary>
        public AssetBundle LoadBundle(string bundleName)
        {
            // 已加载，增加引用计数
            if (_loadedBundles.ContainsKey(bundleName))
            {
                _loadedBundles[bundleName].RefCount++;
                return _loadedBundles[bundleName].Bundle;
            }

            string path = FindBundlePath(bundleName);
            if (string.IsNullOrEmpty(path))
            {
                UnityEngine.Debug.LogError($"[AssetBundleLoader] AB包不存在: {bundleName}");
                return null;
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(path);
            if (bundle == null)
            {
                UnityEngine.Debug.LogError($"[AssetBundleLoader] 加载失败: {bundleName}");
                return null;
            }

            // 缓存
            _loadedBundles[bundleName] = new LoadedBundle
            {
                Bundle = bundle,
                RefCount = 1,
                BundleName = bundleName,
                LoadTime = DateTime.Now
            };

            OnBundleLoaded?.Invoke(bundleName, bundle);
            UnityEngine.Debug.Log($"[AssetBundleLoader] 加载成功: {bundleName}");

            return bundle;
        }

        /// <summary>
        /// 异步加载AB包
        /// </summary>
        public void LoadBundleAsync(string bundleName, Action<AssetBundle> callback)
        {
            // 已加载
            if (_loadedBundles.ContainsKey(bundleName))
            {
                _loadedBundles[bundleName].RefCount++;
                callback?.Invoke(_loadedBundles[bundleName].Bundle);
                return;
            }

            // 正在加载
            if (_loadingCallbacks.ContainsKey(bundleName))
            {
                _loadingCallbacks[bundleName].Add(callback);
                return;
            }

            // 开始加载
            var callbacks = new List<Action<AssetBundle>> { callback };
            _loadingCallbacks[bundleName] = callbacks;
            StartCoroutine(LoadBundleAsyncCoroutine(bundleName));
        }

        private IEnumerator LoadBundleAsyncCoroutine(string bundleName)
        {
            string path = FindBundlePath(bundleName);
            if (string.IsNullOrEmpty(path))
            {
                // 加载失败
                var callbacks = _loadingCallbacks[bundleName];
                _loadingCallbacks.Remove(bundleName);
                foreach (var cb in callbacks)
                {
                    cb?.Invoke(null);
                }
                yield break;
            }

            var request = AssetBundle.LoadFromFileAsync(path);
            while (!request.isDone)
            {
                OnLoadProgress?.Invoke(bundleName, request.progress);
                yield return null;
            }

            if (request.assetBundle == null)
            {
                UnityEngine.Debug.LogError($"[AssetBundleLoader] 异步加载失败: {bundleName}");
                var callbacks = _loadingCallbacks[bundleName];
                _loadingCallbacks.Remove(bundleName);
                foreach (var cb in callbacks)
                {
                    cb?.Invoke(null);
                }
                yield break;
            }

            // 缓存
            _loadedBundles[bundleName] = new LoadedBundle
            {
                Bundle = request.assetBundle,
                RefCount = 1,
                BundleName = bundleName,
                LoadTime = DateTime.Now
            };

            OnBundleLoaded?.Invoke(bundleName, request.assetBundle);

            // 回调所有等待的调用者
            var cbList = _loadingCallbacks[bundleName];
            _loadingCallbacks.Remove(bundleName);
            foreach (var cb in cbList)
            {
                cb?.Invoke(request.assetBundle);
            }
        }

        /// <summary>
        /// 加载AB包并自动加载依赖
        /// </summary>
        public void LoadBundleWithDependencies(string bundleName, Action<AssetBundle> callback)
        {
            StartCoroutine(LoadBundleWithDependenciesCoroutine(bundleName, callback));
        }

        private IEnumerator LoadBundleWithDependenciesCoroutine(string bundleName, Action<AssetBundle> callback)
        {
            var loaded = new HashSet<string>();
            yield return LoadBundleRecursive(bundleName, loaded);
            
            if (_loadedBundles.ContainsKey(bundleName))
            {
                callback?.Invoke(_loadedBundles[bundleName].Bundle);
            }
            else
            {
                callback?.Invoke(null);
            }
        }

        private IEnumerator LoadBundleRecursive(string bundleName, HashSet<string> loaded)
        {
            if (loaded.Contains(bundleName)) yield break;
            loaded.Add(bundleName);

            // 加载依赖
            var bundleInfo = GetBundleInfo(bundleName);
            if (bundleInfo != null && bundleInfo.Dependencies != null)
            {
                foreach (var dep in bundleInfo.Dependencies)
                {
                    yield return LoadBundleRecursive(dep, loaded);
                }
            }

            // 加载自身
            bool done = false;
            LoadBundleAsync(bundleName, _ => done = true);
            yield return new WaitUntil(() => done);
        }

        /// <summary>
        /// 加载资源（从AB包中）
        /// </summary>
        public T LoadAsset<T>(string bundleName, string assetName) where T : UnityEngine.Object
        {
            var bundle = LoadBundle(bundleName);
            if (bundle == null) return null;
            return bundle.LoadAsset<T>(assetName);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public void LoadAssetAsync<T>(string bundleName, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            LoadBundleAsync(bundleName, bundle =>
            {
                if (bundle == null)
                {
                    callback?.Invoke(null);
                    return;
                }
                
                var request = bundle.LoadAssetAsync<T>(assetName);
                StartCoroutine(WaitForAssetLoad(request, callback));
            });
        }

        private IEnumerator WaitForAssetLoad<T>(AssetBundleRequest request, Action<T> callback) where T : UnityEngine.Object
        {
            yield return request;
            callback?.Invoke(request.asset as T);
        }

        /// <summary>
        /// 释放AB包（减少引用计数）
        /// </summary>
        public void ReleaseBundle(string bundleName, bool unloadAll = false)
        {
            if (!_loadedBundles.ContainsKey(bundleName)) return;

            var loaded = _loadedBundles[bundleName];
            loaded.RefCount--;

            if (loaded.RefCount <= 0 || unloadAll)
            {
                UnloadBundle(bundleName, unloadAll);
            }
        }

        /// <summary>
        /// 强制卸载AB包
        /// </summary>
        public void UnloadBundle(string bundleName, bool unloadAll = false)
        {
            if (!_loadedBundles.ContainsKey(bundleName)) return;

            var loaded = _loadedBundles[bundleName];
            
            // 先卸载依赖
            foreach (var dep in loaded.Dependencies)
            {
                if (_loadedBundles.ContainsKey(dep))
                {
                    _loadedBundles[dep].RefCount--;
                    if (_loadedBundles[dep].RefCount <= 0)
                    {
                        _loadedBundles[dep].Bundle.Unload(unloadAll);
                        _loadedBundles.Remove(dep);
                        OnBundleUnloaded?.Invoke(dep);
                    }
                }
            }

            // 卸载自身
            loaded.Bundle.Unload(unloadAll);
            _loadedBundles.Remove(bundleName);
            OnBundleUnloaded?.Invoke(bundleName);
            
            UnityEngine.Debug.Log($"[AssetBundleLoader] 卸载: {bundleName}");
        }

        /// <summary>
        /// 卸载所有AB包
        /// </summary>
        public void UnloadAllBundles(bool unloadAll = false)
        {
            foreach (var name in new List<string>(_loadedBundles.Keys))
            {
                _loadedBundles[name].Bundle.Unload(unloadAll);
                OnBundleUnloaded?.Invoke(name);
            }
            _loadedBundles.Clear();
        }

        /// <summary>
        /// 获取已加载的AB包数量
        /// </summary>
        public int GetLoadedBundleCount()
        {
            return _loadedBundles.Count;
        }

        /// <summary>
        /// 获取指定AB包的引用计数
        /// </summary>
        public int GetRefCount(string bundleName)
        {
            return _loadedBundles.ContainsKey(bundleName) ? _loadedBundles[bundleName].RefCount : 0;
        }

        /// <summary>
        /// 检查AB包是否已加载
        /// </summary>
        public bool IsBundleLoaded(string bundleName)
        {
            return _loadedBundles.ContainsKey(bundleName);
        }

        /// <summary>
        /// 获取内存使用量（估算）
        /// </summary>
        public long GetMemoryUsage()
        {
            long total = 0;
            foreach (var loaded in _loadedBundles.Values)
            {
                total += loaded.MemorySize;
            }
            return total;
        }

        private string FindBundlePath(string bundleName)
        {
            // 1. 优先从本地缓存（热更新后的文件）
            string localPath = Path.Combine(Application.persistentDataPath, bundleName);
            if (File.Exists(localPath))
            {
                return localPath;
            }

            // 2. 从StreamingAssets
            string streamingPath = Path.Combine(Application.streamingAssetsPath, bundleName);
            if (File.Exists(streamingPath))
            {
                return streamingPath;
            }

            // 3. 尝试带路径的名称
            string pathWithFolder = Path.Combine(Application.streamingAssetsPath, "prefab", bundleName);
            if (File.Exists(pathWithFolder))
            {
                return pathWithFolder;
            }

            return null;
        }

        private AssetBundleInfo GetBundleInfo(string bundleName)
        {
            var serverBundles = HotUpdateManager.Instance?.GetServerBundles();
            if (serverBundles != null && serverBundles.ContainsKey(bundleName))
            {
                return serverBundles[bundleName];
            }

            var localBundles = HotUpdateManager.Instance?.GetLocalBundles();
            if (localBundles != null && localBundles.ContainsKey(bundleName))
            {
                return localBundles[bundleName];
            }

            return null;
        }

        private void OnDestroy()
        {
            UnloadAllBundles(false);
        }
    }
}
