using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PathOpener
{
    [MenuItem("Tool/打开持久化目录")]
    public static void OpenPersistentDataPath()
    {
        string path = Application.persistentDataPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        EditorUtility.RevealInFinder(path);
        UnityEngine.Debug.Log($"已打开: {path}");
    }

    [MenuItem("Tool/打开StreamingAssets目录")]
    public static void OpenStreamingAssetsPath()
    {
        string path = Application.streamingAssetsPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        EditorUtility.RevealInFinder(path);
        UnityEngine.Debug.Log($"已打开: {path}");
    }

    [MenuItem("Tool/打开项目根目录")]
    public static void OpenProjectRoot()
    {
        string path = Directory.GetParent(Application.dataPath).FullName;
        EditorUtility.RevealInFinder(path);
        UnityEngine.Debug.Log($"已打开: {path}");
    }

    [MenuItem("Tool/打开服务器目录")]
    public static void OpenServerDir()
    {
        string path = System.IO.Path.GetFullPath(
            System.IO.Path.Combine(Application.dataPath, "../ServerTest")
        );
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        EditorUtility.RevealInFinder(path);
        UnityEngine.Debug.Log($"已打开: {path}");
    }

    [MenuItem("Tool/打印所有路径")]
    public static void PrintAllPaths()
    {
        UnityEngine.Debug.Log("===== Unity 路径信息 =====");
        UnityEngine.Debug.Log($"dataPath:          {Application.dataPath}");
        UnityEngine.Debug.Log($"persistentDataPath: {Application.persistentDataPath}");
        UnityEngine.Debug.Log($"streamingAssetsPath: {Application.streamingAssetsPath}");
        UnityEngine.Debug.Log($"temporaryCachePath: {Application.temporaryCachePath}");
        UnityEngine.Debug.Log($"projectPath:       {Directory.GetParent(Application.dataPath).FullName}");
        UnityEngine.Debug.Log("============================");
    }
}
