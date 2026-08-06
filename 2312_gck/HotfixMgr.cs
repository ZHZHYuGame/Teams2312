using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 热更新主逻辑
/// 流程：拉取服务端版本号 → 对比本地 → 全量或差异下载
/// 所有资源直接存放在persistentDataPath下，不额外套版本号文件夹，方便覆盖
/// </summary>
public class HotfixMgr : MonoBehaviour
{
    [SerializeField] private string res_Server_Url = "http://127.0.0.1/TST/";
    private Version res_Server_Version;
    private Queue<AssetItem> downLoad_Asset_Que = new Queue<AssetItem>();
    private string resList_ConfigStr;
    private const int MAX_RETRY = 3;   // 单文件下载失败重试次数

    private void Start()
    {
        // 启动就检查更新，实际项目可能加个UI过渡
        DownLoad_ServerVersion();
    }

    /// <summary>
    /// 第一步：获取服务器版本号
    /// 如果本地没有版本文件，则视为首次安装，全量下载
    /// </summary>
    private void DownLoad_ServerVersion()
    {
        string versionUrl = res_Server_Url + "Version.txt";
        StartCoroutine(DownLoad_Url_To_Local(versionUrl, (data) =>
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogError("服务器版本文件下载失败或为空");
                return;
            }

            string s_Version = Encoding.UTF8.GetString(data).Trim();
            try
            {
                res_Server_Version = new Version(s_Version);
            }
            catch (Exception e)
            {
                Debug.LogError($"版本解析失败: {s_Version}, 错误: {e.Message}");
                return;
            }

            string localVersionPath = Application.persistentDataPath + "/Version.txt";
            if (!File.Exists(localVersionPath))
            {
                Game_AllRes_DownLoad();
                return;
            }

            string local_Version_Str = File.ReadAllText(localVersionPath).Trim();
            Version local_Version;
            try
            {
                local_Version = new Version(local_Version_Str);
            }
            catch
            {
                // 本地版本损坏，直接当作首次安装
                Game_AllRes_DownLoad();
                return;
            }

            // 版本相同或更小，无需更新
            if (res_Server_Version.CompareTo(local_Version) <= 0)
            {
                Debug.Log("已是最新版本，进入游戏");
                EnterGame();
                return;
            }

            // 大版本（首位变化）直接全量，否则差异更新
            // 中版本更新后建议重启，小版本可以直接热更
            if (res_Server_Version.big > local_Version.big)
                Game_AllRes_DownLoad();
            else
                Game_Hotfix_Res_DownLoad(needRestart: res_Server_Version.middle > local_Version.middle);
        }));
    }

    /// <summary>
    /// 全量下载：下载完整清单，然后把所有资源加入队列依次下载
    /// </summary>
    private void Game_AllRes_DownLoad()
    {
        string resList_Config_Path = res_Server_Url + "ResList_Config.txt";
        StartCoroutine(DownLoad_Url_To_Local(resList_Config_Path, (data) =>
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogError("清单文件下载失败，检查网络或服务器");
                return;
            }

            resList_ConfigStr = Encoding.UTF8.GetString(data);
            ParseAndEnqueueAllAssets(resList_ConfigStr);
            StartCoroutine(DownloadQueueCoroutine());
        }));
    }

    /// <summary>
    /// 差异更新：对比本地清单和服务端清单，只下载新增或MD5变化的资源
    /// 同时删除本地多余的文件（服务器已废弃）
    /// </summary>
    /// <param name="needRestart">中版本以上需要重启游戏</param>
    private void Game_Hotfix_Res_DownLoad(bool needRestart)
    {
        string resList_Config_Path = res_Server_Url + "ResList_Config.txt";
        StartCoroutine(DownLoad_Url_To_Local(resList_Config_Path, (data) =>
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogError("清单文件下载失败");
                return;
            }

            resList_ConfigStr = Encoding.UTF8.GetString(data);
            Dictionary<string, AssetItem> serverDict = Get_Res_AssetItems(resList_ConfigStr);

            string localPath = Application.persistentDataPath + "/ResList_Config.txt";
            Dictionary<string, AssetItem> localDict = new Dictionary<string, AssetItem>();
            if (File.Exists(localPath))
            {
                string localContent = File.ReadAllText(localPath);
                localDict = Get_Res_AssetItems(localContent);
            }

            Compare_Server_And_Local_Res_To_Queue(serverDict, localDict);

            if (downLoad_Asset_Que.Count == 0)
            {
                Debug.Log("无差异资源，直接进入游戏");
                Save_GameVersion();
                Save_GameResList_Config();
                if (needRestart)
                    Debug.Log("中版本更新完成，建议重启游戏");
                else
                    EnterGame();
                return;
            }

            StartCoroutine(DownloadQueueCoroutine(needRestart));
        }));
    }

    /// <summary>
    /// 解析清单字符串，全量加入队列（用于首次安装）
    /// </summary>
    private void ParseAndEnqueueAllAssets(string resStr)
    {
        string[] resList = resStr.Trim().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var res in resList)
        {
            string[] parts = res.Split('|');
            if (parts.Length != 2) continue;
            downLoad_Asset_Que.Enqueue(new AssetItem { path = parts[0], md5 = parts[1] });
        }
    }

    /// <summary>
    /// 将清单字符串转为字典，key为相对路径，方便对比
    /// </summary>
    private Dictionary<string, AssetItem> Get_Res_AssetItems(string resStr)
    {
        Dictionary<string, AssetItem> dict = new Dictionary<string, AssetItem>();
        string[] resList = resStr.Trim().Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var res in resList)
        {
            string[] parts = res.Split('|');
            if (parts.Length != 2) continue;
            dict[parts[0]] = new AssetItem { path = parts[0], md5 = parts[1] };
        }
        return dict;
    }

    /// <summary>
    /// 双端对比：
    /// 1. 服务端有但本地没有 → 下载
    /// 2. 服务端MD5与本地不同 → 下载（覆盖）
    /// 3. 本地有但服务端没有 → 删除
    /// </summary>
    private void Compare_Server_And_Local_Res_To_Queue(Dictionary<string, AssetItem> serverDict, Dictionary<string, AssetItem> localDict)
    {
        foreach (var kv in serverDict)
        {
            if (localDict.TryGetValue(kv.Key, out AssetItem localItem))
            {
                if (kv.Value.md5 != localItem.md5)
                    downLoad_Asset_Que.Enqueue(kv.Value);
            }
            else
                downLoad_Asset_Que.Enqueue(kv.Value);
        }

        foreach (var kv in localDict)
        {
            if (!serverDict.ContainsKey(kv.Key))
            {
                string fullPath = Application.persistentDataPath + "/" + kv.Key;
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Debug.Log($"删除多余本地资源: {kv.Key}");
                }
            }
        }
    }

    /// <summary>
    /// 队列下载协程，逐个下载，全部完成后保存版本和清单
    /// </summary>
    private IEnumerator DownloadQueueCoroutine(bool needRestart = false)
    {
        while (downLoad_Asset_Que.Count > 0)
        {
            AssetItem item = downLoad_Asset_Que.Dequeue();
            yield return StartCoroutine(DownloadSingleAsset(item));
        }

        Save_GameVersion();
        Save_GameResList_Config();
        Debug.Log("所有资源下载完成");

        if (needRestart)
            Debug.Log("中版本更新完成，请重启游戏");
        else
            EnterGame();
    }

    /// <summary>
    /// 单个资源下载，包含重试和MD5校验
    /// 重试次数用尽仍失败则报错，但不会阻塞后面的资源（实际项目可做补偿）
    /// </summary>
    private IEnumerator DownloadSingleAsset(AssetItem item)
    {
        string assetUrl = res_Server_Url + item.path;   // 直接拼接，不带版本号
        string localPath = Application.persistentDataPath + "/" + item.path;
        int retry = 0;
        bool success = false;

        while (retry < MAX_RETRY && !success)
        {
            retry++;
            Debug.Log($"下载 ({retry}/{MAX_RETRY}): {assetUrl}");

            byte[] data = null;
            bool downloadDone = false;
            StartCoroutine(DownLoad_Url_To_Local(assetUrl, (bytes) =>
            {
                data = bytes;
                downloadDone = true;
            }));

            yield return new WaitUntil(() => downloadDone);

            if (data == null || data.Length == 0)
            {
                Debug.LogWarning($"下载失败，重试 {retry}/{MAX_RETRY}");
                continue;
            }

            string localMd5 = CalculateMD5(data);
            if (localMd5 != item.md5)
            {
                Debug.LogWarning($"MD5校验失败，期望: {item.md5}, 实际: {localMd5}，重试 {retry}/{MAX_RETRY}");
                continue;
            }

            try
            {
                string dir = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(localPath, data);
                Debug.Log($"下载成功: {item.path}");
                success = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"写入文件失败: {e.Message}");
                // 写入失败不重试，直接退出循环，由外部逻辑处理
                break;
            }
        }

        if (!success)
            Debug.LogError($"下载失败超过最大重试: {item.path}");
    }

    /// <summary>
    /// 核心下载函数，使用UnityWebRequest，兼容HTTP/HTTPS
    /// 成功返回byte[]，失败返回null，由上层决定重试
    /// </summary>
    private IEnumerator DownLoad_Url_To_Local(string res_URL, Action<byte[]> complete)
    {
        using (UnityWebRequest u_Web = UnityWebRequest.Get(res_URL))
        {
            yield return u_Web.SendWebRequest();
            if (u_Web.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"下载成功: {res_URL}, 大小: {u_Web.downloadHandler.data.Length}");
                complete?.Invoke(u_Web.downloadHandler.data);
            }
            else
            {
                Debug.LogError($"下载失败: {res_URL}\n错误: {u_Web.error}\n状态码: {u_Web.responseCode}");
                complete?.Invoke(null);
            }
        }
    }

    /// <summary>
    /// 计算byte数组的MD5，用于校验下载的文件是否完整
    /// </summary>
    private string CalculateMD5(byte[] data)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hash)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }

    private void Save_GameVersion()
    {
        File.WriteAllText(Application.persistentDataPath + "/Version.txt", res_Server_Version.ToString());
    }

    private void Save_GameResList_Config()
    {
        File.WriteAllText(Application.persistentDataPath + "/ResList_Config.txt", resList_ConfigStr);
    }

    private void EnterGame()
    {
        Debug.Log("进入游戏...");
        // 这里加载主场景或者通知其他模块
    }
}

/// <summary>
/// 简单版本号类，支持 x.y.z 三段比较
/// 写这个是因为不想依赖System.Version的额外判断，自己控制更放心
/// </summary>
public class Version : IComparable<Version>
{
    public int big, middle, small;

    public Version(string verStr)
    {
        if (string.IsNullOrEmpty(verStr))
            throw new ArgumentException("版本字符串不能为空");
        var parts = verStr.Trim().Split('.');
        if (parts.Length != 3)
            throw new FormatException($"版本格式应为 'x.y.z'，实际为 '{verStr}'");
        if (!int.TryParse(parts[0], out big) ||
            !int.TryParse(parts[1], out middle) ||
            !int.TryParse(parts[2], out small))
            throw new FormatException($"版本包含非数字字符: {verStr}");
    }

    public override string ToString() => $"{big}.{middle}.{small}";

    public int CompareTo(Version other)
    {
        if (other == null) return 1;
        if (big != other.big) return big.CompareTo(other.big);
        if (middle != other.middle) return middle.CompareTo(other.middle);
        return small.CompareTo(other.small);
    }
}

/// <summary>
/// 资源条目，包含路径和MD5，用于队列和字典存储
/// </summary>
public class AssetItem
{
    public string path;
    public string md5;
}