using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AB包打包工具
/// 直接挂到编辑器菜单下，方便策划/美术自己打资源
/// </summary>
public class AssetBundleMgr : Editor
{
    [MenuItem("Tools/资源生成AB包")]
    public static void CreateAssetBundle()
    {
        // 每次打包前先把StreamingAssets清空，防止旧文件干扰（虽然现在不用S目录了，但保留习惯）
        Res_Delete();

        // 关键：必须把所有旧AB名清掉，否则会残留一堆无用的bundle
        ClearAllAssetBundleNames();

        // 重新为Resources下的prefab和mat设置AB名
        SetAssetBundleNames();

        // 输出路径写死了，后面项目上正式服再改成配置或者动态获取
        string res_Out_Path = "E:/unityHub/UNG/GameRes";

        // 执行真正的构建，压缩用ChunkBased，体积和加载速度均衡
        BuildPipeline.BuildAssetBundles(res_Out_Path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);

        // 构建完后根据生成的u3d文件生成清单和MD5，这样MD5才是最终文件的，不会出错
        GenerateResList(res_Out_Path);

        // 保存当前版本号，方便热更新对比
        SaveGameVersion(res_Out_Path);

        // 自动弹窗打开目录，方便直接拷贝到服务器
        Process.Start(res_Out_Path);

        // 刷新一下资源数据库，防止后续操作引用旧数据
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/打开OpenStreamAssetsPath目录")]
    public static void OpenStreamAssetsPath()
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
            Directory.CreateDirectory(Application.streamingAssetsPath);
        Process.Start(Application.streamingAssetsPath);
    }

    [MenuItem("Tools/打开OpenPersistentDataPath目录")]
    public static void OpenPersistentDataPath()
    {
        Process.Start(Application.persistentDataPath);
    }

    /// <summary>
    /// 清除所有资源的AB名，避免历史遗留导致打出多余bundle
    /// 每次打包前调用，省心
    /// </summary>
    private static void ClearAllAssetBundleNames()
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        int clearedCount = 0;
        foreach (string path in allAssetPaths)
        {
            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer != null && !string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.assetBundleName = null;
                importer.assetBundleVariant = null;
                clearedCount++;
            }
        }
        UnityEngine.Debug.Log($"已清除 {clearedCount} 个资源的AB名");

        // 清理空名字的残留引用
        AssetDatabase.RemoveUnusedAssetBundleNames();
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 只对Resources下的.prefab和.mat设置AB名，其他资源暂时不处理
    /// 这里不再带版本号，直接使用 prefab/xxx 或 mat/xxx，便于覆盖更新
    /// </summary>
    private static void SetAssetBundleNames()
    {
        string res_Pack_Path = $"{Application.dataPath}/Resources";
        if (!Directory.Exists(res_Pack_Path))
        {
            UnityEngine.Debug.LogWarning("Resources目录不存在，跳过设置");
            return;
        }

        string[] res_All_Files = Directory.GetFiles(res_Pack_Path, "*.*", SearchOption.AllDirectories);
        string[] extendArr = new string[] { ".meta" };
        string[] res_File_Path_Arr = res_All_Files.Where(f => !extendArr.Contains(Path.GetExtension(f).ToLower())).ToArray();

        int setCount = 0;
        foreach (var filePath in res_File_Path_Arr)
        {
            string changePath = filePath.Replace(@"\", "/");
            string splitAssetPath = changePath.Replace(Application.dataPath, "Assets");
            string extensionName = Path.GetExtension(filePath);

            AssetImporter ai = AssetImporter.GetAtPath(splitAssetPath);
            if (ai == null) continue;

            string bundleName = "";
            if (extensionName == ".prefab")
                bundleName = "prefab/" + Path.GetFileNameWithoutExtension(filePath);
            else if (extensionName == ".mat")
                bundleName = "mat/" + Path.GetFileNameWithoutExtension(filePath);
            else
                continue;

            ai.assetBundleName = bundleName;
            ai.assetBundleVariant = "u3d";
            setCount++;
        }
        UnityEngine.Debug.Log($"已设置 {setCount} 个资源的AB名");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 清空StreamingAssets目录（其实现在没用，但留着以防后面改主意）
    /// </summary>
    static void Res_Delete()
    {
        string res_Out_Path = $"{Application.streamingAssetsPath}";
        if (Directory.Exists(res_Out_Path))
        {
            string[] res_All_File = Directory.GetFiles(res_Out_Path, "*.*", SearchOption.AllDirectories);
            foreach (var file in res_All_File)
                File.Delete(file);
        }
    }

    /// <summary>
    /// 遍历输出目录下所有.u3d，计算其MD5并生成清单文件
    /// 清单格式：相对路径|MD5，每行一个
    /// 注意：这里路径不带版本号，且清单名固定为 ResList_Config.txt
    /// </summary>
    static void GenerateResList(string outputPath)
    {
        string[] bundleFiles = Directory.GetFiles(outputPath, "*.u3d", SearchOption.AllDirectories);
        StringBuilder sb = new StringBuilder();
        foreach (string fullPath in bundleFiles)
        {
            string md5 = GetMD5(fullPath);
            string relativePath = Path.GetRelativePath(outputPath, fullPath).Replace('\\', '/');
            sb.AppendLine($"{relativePath}|{md5}");
        }
        string listPath = outputPath + "/ResList_Config.txt";
        File.WriteAllText(listPath, sb.ToString());
        UnityEngine.Debug.Log($"清单生成成功: {listPath}");
    }

    /// <summary>
    /// 保存版本号，客户端启动时拉取这个对比
    /// </summary>
    static void SaveGameVersion(string outputPath)
    {
        string save_Path = outputPath + "/Version.txt";
        File.WriteAllText(save_Path, Application.version);
    }

    /// <summary>
    /// 工具方法：计算文件MD5，重试或异常时有用
    /// </summary>
    public static string GetMD5(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("文件不存在", filePath);
        using (MD5 md5Hash = MD5.Create())
        using (FileStream stream = File.OpenRead(filePath))
        {
            byte[] hashBytes = md5Hash.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}