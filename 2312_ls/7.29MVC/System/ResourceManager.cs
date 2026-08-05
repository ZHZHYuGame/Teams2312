using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源管理器 - 负责加载和缓存Resources目录下的资源
/// </summary>
public class ResourceManager : Singleton<ResourceManager>
{
    private Dictionary<string, Object> resCache = new Dictionary<string, Object>();

    /// <summary>
    /// 加载资源（带缓存）
    /// </summary>
    public T LoadRes<T>(string folderPath, string fileName) where T : Object
    {
        string fullPath = folderPath + "/" + fileName;

        if (resCache.ContainsKey(fullPath))
        {
            return resCache[fullPath] as T;
        }

        Object obj = Resources.Load<T>(fullPath);
        if (obj == null)
        {
            Debug.LogError(string.Format("找不到资源: {0}", fullPath));
            return null;
        }

        resCache.Add(fullPath, obj);
        return obj as T;
    }

    /// <summary>
    /// 加载资源（不缓存）
    /// </summary>
    public T LoadResWithoutCache<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    /// <summary>
    /// 清除指定缓存
    /// </summary>
    public void UnloadRes(string path)
    {
        if (resCache.ContainsKey(path))
        {
            resCache.Remove(path);
        }
    }

    /// <summary>
    /// 清除所有缓存
    /// </summary>
    public void ClearAllCache()
    {
        resCache.Clear();
        Resources.UnloadUnusedAssets();
    }
}
