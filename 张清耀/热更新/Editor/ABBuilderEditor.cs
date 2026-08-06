using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// AB包打包与发布工具 - 游戏公司版本
/// 菜单：Tool/打包并发布到服务器
/// 
/// 流程：
/// 1. 读取 Application.version 作为版本号
/// 2. 扫描 Resources/ 下的资源，设置 AssetBundle 名称
/// 3. 打包到 ServerTest/版本号/AssetBundles/
/// 4. 计算每个包的 MD5，生成 manifest.json
/// 5. 更新根 version.json（指向新版本）
/// </summary>
public class ABBuilderEditor
{
    /// <summary>服务器目录（项目根目录下的 ServerTest）</summary>
    private static string ServerDir =>
        Path.GetFullPath(Path.Combine(Application.dataPath, "../ServerTest"));

    /// <summary>服务器地址</summary>
    private const string ServerUrl = "http://127.0.0.1:9999";

    // =========================================================================
    // 菜单入口
    // =========================================================================

    [MenuItem("Tool/打包并发布到服务器")]
    public static void BuildAndPublish()
    {
        // 检查是否在 Play 模式
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("错误", "不能在 Play 模式下打包！\n请先停止运行，再打包发布。", "确定");
            return;
        }

        string version = Application.version;
        if (string.IsNullOrEmpty(version))
        {
            EditorUtility.DisplayDialog("错误", "版本号为空！\n请先在 PlayerSettings 中设置版本号", "确定");
            return;
        }

        UnityEngine.Debug.Log($"[ABBuilder] ===== 开始打包 =====");
        UnityEngine.Debug.Log($"[ABBuilder] 版本号: {version}");
        UnityEngine.Debug.Log($"[ABBuilder] 服务器目录: {ServerDir}");

        // 1. 设置 AssetBundle 名称
        SetAssetBundleNames();

        // 2. 准备版本目录
        string versionDir = Path.Combine(ServerDir, version);
        string abDir = Path.Combine(versionDir, "AssetBundles");
        if (Directory.Exists(abDir))
        {
            Directory.Delete(abDir, true);
        }
        Directory.CreateDirectory(abDir);

        // 3. 打包
        BuildPipeline.BuildAssetBundles(abDir,
            BuildAssetBundleOptions.ChunkBasedCompression,
            BuildTarget.StandaloneWindows64);
        UnityEngine.Debug.Log($"[ABBuilder] AB包打包完成: {abDir}");

        // 4. 加载主清单获取依赖信息
        AssetBundleManifest bundleManifest = LoadBundleManifest(abDir);

        // 5. 生成 manifest.json
        GenerateManifest(versionDir, abDir, version, bundleManifest);

        // 6. 更新根 version.json
        UpdateRootVersion(version);

        // 7. 清理 Unity 生成的临时文件
        CleanupUnityFiles(abDir);

        UnityEngine.Debug.Log($"[ABBuilder] ===== 发布完成！版本: {version} =====");
        EditorUtility.DisplayDialog("发布完成",
            $"版本 {version} 已发布到:\n{ServerDir}\n\n" +
            "目录结构:\n" +
            $"  ServerTest/{version}/manifest.json\n" +
            $"  ServerTest/{version}/AssetBundles/*.u3d\n" +
            $"  ServerTest/version.json",
            "确定");
    }

    // =========================================================================
    // 步骤1：设置 AssetBundle 名称
    // =========================================================================

    /// <summary>
    /// 扫描 Resources 目录，为每个资源设置 AssetBundle 名称
    /// 命名规则：文件名小写，variant = u3d
    /// </summary>
    private static void SetAssetBundleNames()
    {
        string resDir = Path.Combine(Application.dataPath, "Resources");
        if (!Directory.Exists(resDir))
        {
            UnityEngine.Debug.LogWarning("[ABBuilder] Resources 目录不存在");
            return;
        }

        // 支持的资源类型
        string[] validExtensions = { ".prefab", ".mat", ".png", ".jpg", ".fbx", ".wav", ".mp3" };

        string[] files = Directory.GetFiles(resDir, "*.*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta"))
            .ToArray();

        int count = 0;
        foreach (string file in files)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (!validExtensions.Contains(ext))
                continue;

            // 转换为 Unity 的 Asset 路径
            string assetPath = file.Replace("\\", "/").Replace(Application.dataPath, "Assets");
            string bundleName = Path.GetFileNameWithoutExtension(file).ToLower();

            AssetImporter ai = AssetImporter.GetAtPath(assetPath);
            if (ai != null)
            {
                ai.assetBundleName = bundleName;
                ai.assetBundleVariant = "u3d";
                UnityEngine.Debug.Log($"[ABBuilder] 设置AB名: {assetPath} → {bundleName}.u3d");
                count++;
            }
        }
        UnityEngine.Debug.Log($"[ABBuilder] 共设置 {count} 个资源的AB名");
    }

    // =========================================================================
    // 步骤4：加载主清单
    // =========================================================================

    /// <summary>
    /// 加载 Unity 生成的 AssetBundleManifest（用于获取依赖关系）
    /// </summary>
    private static AssetBundleManifest LoadBundleManifest(string abDir)
    {
        string dirName = Path.GetFileName(abDir);
        string mainManifestPath = Path.Combine(abDir, dirName);

        if (!File.Exists(mainManifestPath))
        {
            UnityEngine.Debug.LogWarning("[ABBuilder] 主清单文件不存在，依赖关系将为空");
            return null;
        }

        var mainBundle = AssetBundle.LoadFromFile(mainManifestPath);
        var manifest = mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        mainBundle.Unload(false);
        return manifest;
    }

    // =========================================================================
    // 步骤5：生成 manifest.json
    // =========================================================================

    /// <summary>
    /// 生成版本清单 manifest.json
    /// </summary>
    private static void GenerateManifest(string versionDir, string abDir, string version, AssetBundleManifest bundleManifest)
    {
        // 收集所有 .u3d 文件
        string[] abFiles = Directory.GetFiles(abDir, "*.u3d")
            .Where(f => !f.EndsWith(".manifest"))
            .ToArray();

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"Version\": \"{version}\",");
        sb.AppendLine($"  \"ResVersion\": \"{version}\",");
        sb.AppendLine($"  \"Timestamp\": {GetTimestamp()},");
        sb.AppendLine("  \"AssetBundles\": [");

        for (int i = 0; i < abFiles.Length; i++)
        {
            string file = abFiles[i];
            string name = Path.GetFileName(file);
            string hash = GetFileMD5(file);
            long size = new FileInfo(file).Length;
            string url = $"{ServerUrl}/{version}/AssetBundles/{name}";

            // 获取依赖包列表
            string[] deps = bundleManifest != null
                ? bundleManifest.GetAllDependencies(name)
                : new string[0];

            string depsJson = deps.Length > 0
                ? "[" + string.Join(", ", deps.Select(d => $"\"{d}\"")) + "]"
                : "[]";

            sb.AppendLine("    {");
            sb.AppendLine($"      \"Name\": \"{name}\",");
            sb.AppendLine($"      \"Hash\": \"{hash}\",");
            sb.AppendLine($"      \"Size\": {size},");
            sb.AppendLine($"      \"Url\": \"{url}\",");
            sb.AppendLine($"      \"Dependencies\": {depsJson},");
            sb.AppendLine($"      \"IsEncrypted\": false,");
            sb.AppendLine($"      \"ShardSize\": 0,");
            sb.AppendLine($"      \"Type\": 0,");
            sb.AppendLine($"      \"Tags\": [\"test\"]");
            sb.AppendLine(i < abFiles.Length - 1 ? "    }," : "    }");

            UnityEngine.Debug.Log($"[ABBuilder] {name}: size={size}, hash={hash}, deps={deps.Length}");
        }

        sb.AppendLine("  ]");
        sb.AppendLine("}");

        string manifestPath = Path.Combine(versionDir, "manifest.json");
        File.WriteAllText(manifestPath, sb.ToString());
        UnityEngine.Debug.Log($"[ABBuilder] manifest.json 已生成: {manifestPath}");
    }

    // =========================================================================
    // 步骤6：更新根 version.json
    // =========================================================================

    /// <summary>
    /// 更新根 version.json，指向当前最新版本
    /// </summary>
    private static void UpdateRootVersion(string version)
    {
        // 确保 ServerTest 目录存在
        Directory.CreateDirectory(ServerDir);

        string rootPath = Path.Combine(ServerDir, "version.json");
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"CurrentVersion\": \"{version}\",");
        sb.AppendLine($"  \"ServerUrl\": \"{ServerUrl}\",");
        sb.AppendLine("  \"ManifestPath\": \"/{version}/manifest.json\",");
        sb.AppendLine($"  \"UpdateTime\": \"{DateTime.Now:yyyy-MM-ddTHH:mm:ssZ}\"");
        sb.AppendLine("}");
        File.WriteAllText(rootPath, sb.ToString());
        UnityEngine.Debug.Log($"[ABBuilder] 根 version.json 已更新: {rootPath}");
    }

    // =========================================================================
    // 步骤7：清理 Unity 临时文件
    // =========================================================================

    /// <summary>
    /// 清理 Unity 生成的 .manifest 文件和主清单文件
    /// 服务器只需要 .u3d 文件
    /// </summary>
    private static void CleanupUnityFiles(string abDir)
    {
        // 删除所有 .manifest 文件（Unity自动生成的依赖描述文件）
        string[] manifestFiles = Directory.GetFiles(abDir, "*.manifest");
        foreach (string f in manifestFiles)
        {
            File.Delete(f);
        }

        // 删除主清单文件（与目录同名的无扩展名文件）
        string dirName = Path.GetFileName(abDir);
        string mainManifest = Path.Combine(abDir, dirName);
        if (File.Exists(mainManifest))
        {
            File.Delete(mainManifest);
        }

        UnityEngine.Debug.Log("[ABBuilder] 已清理 Unity 临时文件");
    }

    // =========================================================================
    // 工具方法
    // =========================================================================

    /// <summary>
    /// 计算文件的 MD5 哈希值
    /// </summary>
    private static string GetFileMD5(string filePath)
    {
        using (MD5 md5 = MD5.Create())
        using (FileStream fs = File.OpenRead(filePath))
        {
            byte[] hashBytes = md5.ComputeHash(fs);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    /// <summary>
    /// 获取当前 Unix 时间戳
    /// </summary>
    private static long GetTimestamp()
    {
        return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
    }
}
