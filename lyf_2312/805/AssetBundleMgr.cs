using System;
using System.Collections;
using System.Collections.Generic;
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
        string res_Out_Path = $"{Application.streamingAssetsPath}";
        BuildPipeline .BuildAssetBundles(res_Out_Path, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneLinux64);
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
    public static void OpenPersitenDatePath()
    {
        Process.Start(Application.persistentDataPath);
    }
    [MenuItem("Tools/清空所有 AssetBundle 名称")]
    public static void ClearAllBundleNames()
    {
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        int clearedCount = 0;

        foreach (string path in assetPaths)
        {
            if (Directory.Exists(path)) continue;

            AssetImporter importer = AssetImporter.GetAtPath(path);
            if (importer == null) continue;

            if (!string.IsNullOrEmpty(importer.assetBundleName))
            {
                importer.assetBundleName = "";
                importer.assetBundleVariant = "";
                EditorUtility.SetDirty(importer);
                clearedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
       
    }

    private static void Res_Pack_Handle()
    {
        string res_Pack_Path = $"{Application.dataPath}/Resources";
        string[] res_All_Files = Directory.GetFiles(res_Pack_Path, "*.*", SearchOption.AllDirectories);
        string[] extendArr = new string[] { ".meta" };
        string[] res_File_Path_Arr = res_All_Files.Where((f) => !extendArr.Contains(Path.GetExtension(f).ToLower())).ToArray();
        foreach (var filePath in res_File_Path_Arr)
        {
            string changePath = filePath.Replace(@"\", "/");
            string splitAssetPath = changePath.Replace(Application.dataPath, "Assets");
            string onlyResName = Path.GetFileNameWithoutExtension(filePath);
            string extensionName = Path.GetExtension(filePath);
            AssetImporter ai = AssetImporter.GetAtPath(splitAssetPath);
            if(extensionName == ".prefab")
                ai.assetBundleName = "Prefab/" + Path.GetFileNameWithoutExtension(filePath);
            else if(extensionName == ".mat")
                ai.assetBundleName = "Mat/" + Path.GetFileNameWithoutExtension(filePath);
            ai.assetBundleVariant = "u3d";
        }
    }

    private static void Res_Delete()
    {
        string res_Out_Path = $"{Application.streamingAssetsPath}";
        //string[] res_All_File = Directory.GetFiles(res_Out_Path, "*.*", SearchOption.AllDirectories);
        //foreach (var file in res_All_File)
        //{
        //    File.Delete(file);
        //}
        if (Directory.Exists(res_Out_Path))
        {
            string[] res_All_File = Directory.GetFiles(res_Out_Path, "*.*", SearchOption.AllDirectories);
            foreach (var file in res_All_File)
            {
                File.Delete(file);
            }
            
        }
        else
        {
            // 目录不存在，直接创建
            Directory.CreateDirectory(res_Out_Path);
            
        }
    }
}
