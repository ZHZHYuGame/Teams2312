using System.Collections.Generic;
using UnityEngine;

namespace HotUpdate
{
    /// <summary>
    /// 资源加载器 - 单例模式
    /// 负责从本地缓存或内置资源中加载AB包和资源
    /// 加载优先级: 热更缓存(PersistentDataPath) > 内置资源(StreamingAssets)
    /// </summary>
    public class AssetLoader : MonoBehaviour
    {
        // 单例实例
        private static AssetLoader _instance;
        public static AssetLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[AssetLoader]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<AssetLoader>();
                }
                return _instance;
            }
        }

        // 已加载的热更AB包缓存（从PersistentDataPath加载）
        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        // 内置AB包缓存（从StreamingAssets加载）
        private readonly Dictionary<string, AssetBundle> _builtinBundles = new Dictionary<string, AssetBundle>();

        /// <summary>
        /// 初始化 - 加载内置的AssetBundle清单
        /// 应在游戏启动时调用一次
        /// </summary>
        public void Init()
        {
            LoadBuiltinBundles();
        }

        /// <summary>
        /// 加载内置AB包（StreamingAssets中的原始包）
        /// 这些包不会被热更新覆盖
        /// </summary>
        private void LoadBuiltinBundles()
        {
            // 加载主Manifest文件
            var mainAB = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles"));
            if (mainAB == null) return;

            // 解析Manifest获取所有AB包列表
            var manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            if (manifest == null) return;

            // 加载所有内置AB包到内存
            var allBundles = manifest.GetAllAssetBundles();
            foreach (var bundleName in allBundles)
            {
                var bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleName));
                if (bundle != null)
                {
                    _builtinBundles[bundleName] = bundle;
                }
            }
        }

        /// <summary>
        /// 加载指定的AB包
        /// 优先从热更缓存加载，找不到则从内置资源加载
        /// </summary>
        /// <param name="bundleName">AB包名称</param>
        public void LoadBundle(string bundleName)
        {
            // 已加载则跳过
            if (_loadedBundles.ContainsKey(bundleName)) return;

            string localPath = System.IO.Path.Combine(Application.persistentDataPath, bundleName);
            string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, "AssetBundles", bundleName);

            // 优先使用本地热更缓存
            string path = System.IO.File.Exists(localPath) ? localPath : streamingPath;
            var bundle = AssetBundle.LoadFromFile(path);
            if (bundle != null)
            {
                _loadedBundles[bundleName] = bundle;
            }
        }

        /// <summary>
        /// 从AB包中加载指定类型的资源
        /// 加载顺序: 热更缓存 → 内置资源 → 动态加载
        /// </summary>
        /// <typeparam name="T">资源类型（Texture2D, AudioClip, GameObject等）</typeparam>
        /// <param name="bundleName">AB包名称</param>
        /// <param name="assetName">资源名称</param>
        /// <returns>资源对象，未找到返回null</returns>
        public T LoadAsset<T>(string bundleName, string assetName) where T : Object
        {
            // 1. 从已加载的热更包中查找
            if (_loadedBundles.ContainsKey(bundleName))
            {
                return _loadedBundles[bundleName].LoadAsset<T>(assetName);
            }

            // 2. 从内置包中查找
            if (_builtinBundles.ContainsKey(bundleName))
            {
                return _builtinBundles[bundleName].LoadAsset<T>(assetName);
            }

            // 3. 动态加载并查找
            LoadBundle(bundleName);
            if (_loadedBundles.ContainsKey(bundleName))
            {
                return _loadedBundles[bundleName].LoadAsset<T>(assetName);
            }

            return null;
        }

        /// <summary>
        /// 卸载指定的AB包
        /// </summary>
        /// <param name="bundleName">AB包名称</param>
        /// <param name="unloadAllLoaded">是否同时卸载从该包加载的所有资源</param>
        public void UnloadBundle(string bundleName, bool unloadAllLoaded = false)
        {
            if (_loadedBundles.ContainsKey(bundleName))
            {
                _loadedBundles[bundleName].Unload(unloadAllLoaded);
                _loadedBundles.Remove(bundleName);
            }
        }

        /// <summary>
        /// 卸载所有已加载的热更AB包
        /// </summary>
        /// <param name="unloadAllLoaded">是否同时卸载所有加载的资源</param>
        public void UnloadAll(bool unloadAllLoaded = false)
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                bundle.Unload(unloadAllLoaded);
            }
            _loadedBundles.Clear();
        }

        /// <summary>
        /// 对象销毁时自动清理
        /// </summary>
        private void OnDestroy()
        {
            UnloadAll(true);
        }
    }
}
