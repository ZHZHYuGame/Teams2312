using UnityEngine;

namespace MVC.Core
{
    /// <summary>
    /// MonoBehaviour 泛型单例基类。
    /// 保证全局唯一实例，跨场景不销毁。
    /// </summary>
    public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _appIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (_appIsQuitting) return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            var go = new GameObject(typeof(T).Name);
                            _instance = go.AddComponent<T>();
                        }
                    }
                    return _instance;
                }
            }
        }

        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = (T)this;
            DontDestroyOnLoad(gameObject);
            OnAwake();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnApplicationQuit()
        {
            _appIsQuitting = true;
        }
    }
}