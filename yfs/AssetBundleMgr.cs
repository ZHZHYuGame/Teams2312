using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AssetBundleMgr : Editor
{
    [MenuItem("Tools/资源生成AB包")]
    public static void CreateAssetBundle()
    {
        Res_Delete();
        Res_Pack_Handle();
        //打包输出路径 Unity中的S目录
        string res_Out_Path = $"{Application.streamingAssetsPath}";

        BuildPipeline.BuildAssetBundles(res_Out_Path, BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.StandaloneWindows64);

        Process.Start(res_Out_Path);
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
    /// 资源的打包划分（AB包打包策略）
    /// </summary>
    public static void Res_Pack_Handle()
    {
        //打包资源路径
        string res_Pack_Path = $"{Application.dataPath}/Resources";
        //找到所有资源
        string[] res_All_Files = Directory.GetFiles(res_Pack_Path, "*.*", SearchOption.AllDirectories);
        //打包的资源类型或者不打包的资源类型
        string[] extendArr = new string[] { ".meta" };
        //筛选出所有符合条件的资源路径
        string[] res_File_Path_Arr =
            res_All_Files.Where((f) => !extendArr.Contains(Path.GetExtension(f).ToLower())).ToArray();
        //用筛选出的资源打包成AB包
        foreach (var filePath in res_File_Path_Arr)
        {
            //打AB包有规则，有格式
            string changePath = filePath.Replace(@"\", "/");
            string splitAssetPath = changePath.Replace(Application.dataPath, "Assets");

            //string name1 = Path.GetFileName(filePath);
            //string name2 = Path.GetDirectoryName(filePath);
            //string name3 = Path.GetFileNameWithoutExtension(filePath);

            string onlyResName = Path.GetFileNameWithoutExtension(filePath);
            string extensionName = Path.GetExtension(filePath);

            AssetImporter ai = AssetImporter.GetAtPath(splitAssetPath);

            if (extensionName == ".prefab")
                ai.assetBundleName = "Prefab/" + Path.GetFileNameWithoutExtension(filePath);
            else if (extensionName == ".mat")
                ai.assetBundleName = "Mat/" + Path.GetFileNameWithoutExtension(filePath);
            ai.assetBundleVariant = "u3d";
        }
    }

    static void Res_Delete()
    {
        //打包输出路径 Unity中的S目录
        string res_Out_Path = $"{Application.streamingAssetsPath}";

        string[] res_All_File = Directory.GetFiles(res_Out_Path, "*.*", SearchOption.AllDirectories);

        foreach (var file in res_All_File)
        {
            File.Delete(file);
        }
    }
}