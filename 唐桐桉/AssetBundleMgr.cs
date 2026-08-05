using Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
/// <summary>
/// AssetBundle打包
/// </summary>
public class AssetBundleMgr : Editor
{
    [MenuItem("Tools/资源生成AB包")]
    public static void CreateAssetBundle()
    {
        Res_Delete();
        Res_Pack_Handle();
        //打包输出路径 Unity中的S目录
        //  string res_Out_Path = $"{Application.streamingAssetsPath}";

        string res_Out_Path = "E:/GameRes";
        BuildPipeline.BuildAssetBundles(res_Out_Path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows64);

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
        //string res_Pack_Path = "c";
        //找到所有资源
        string[] res_All_Files = Directory.GetFiles(res_Pack_Path, "*.*", SearchOption.AllDirectories);
        //打包的资源类型或者不打包的资源类型
        string[] extendArr = new string[] { ".meta" };
        //筛选出所有符合条件的资源路径
        string[] res_File_Path_Arr = res_All_Files.Where((f) => !extendArr.Contains(Path.GetExtension(f).ToLower())).ToArray();
        //用筛选出的资源打包成AB包
        StringBuilder sb=new StringBuilder();
        foreach (var filePath in res_File_Path_Arr)
        {
            //打AB包有规则，有格式
            string changePath = filePath.Replace(@"\", "/");
            string splitAssetPath = changePath.Replace(Application.dataPath, "Assets");


            string onlyResName = Path.GetFileNameWithoutExtension(filePath);
            string extensionName = Path.GetExtension(filePath);

            AssetImporter ai = AssetImporter.GetAtPath(splitAssetPath);

            if (extensionName == ".prefab")
                ai.assetBundleName = Application.version + "/Prefab/" + Path.GetFileNameWithoutExtension(filePath);
            else if(extensionName == ".mat")
                ai.assetBundleName = Application.version + "/Mat/" + Path.GetFileNameWithoutExtension(filePath);
            ai.assetBundleVariant = "u3d";
            string md5 = GetMD5(changePath);
            string res_Format = ai.assetBundleName+"."+ai.assetBundleVariant+"|"+md5;
            sb.AppendLine(res_Format);
        }
        SaveGameVersion();
        SaveGameResList_Config(sb.ToString());
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
    public static string GetMD5(string filePath)
    {
        // 判断文件是否存在
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("文件不存在", filePath);
        }

        using (MD5 md5Hash = MD5.Create())
        using (FileStream stream = File.OpenRead(filePath))
        {
            byte[] hashBytes = md5Hash.ComputeHash(stream);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
    /// <summary>
    /// 保存当前更新迭代的游戏版本号
    /// </summary>
    static void SaveGameVersion()
    {
        //保存路径
        string save_Path = "E:/GameRes/Version.txt";
        //保存内容
        string save_DescStr = Application.version;
        File.WriteAllText(save_Path,save_DescStr);
    }
    /// <summary>
    /// 保存当前更新迭代的游戏资源清单文件
    /// </summary>
    static void SaveGameResList_Config(string contect)
    {
        //保存路径
        string save_ResList_Config_Path = "E:/GameRes/"+Application.version+"ResList_Config.txt";
        File.WriteAllText(save_ResList_Config_Path,contect);
    }
}
