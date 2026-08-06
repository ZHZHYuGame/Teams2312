using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 热更新管理
/// 1版本号
/// 2资源清单
/// 3资源
/// 资源服务器 Http
/// 拷贝
/// 上传
/// 下载
/// </summary>
public class HotfixMgr : MonoBehaviour
{
    /// <summary>
    /// 资源服务器的地址(Https://)
    /// </summary>
    string res_Sever_Url = "http://127.0.0.1/Main/";
    //资源服务器的游戏版本号
    Version res_Sever_Version;
    /// <summary>
    /// 资源下载队列
    /// 1.第一次安装游戏必触发
    /// 2.双端版本不一致的情况必触发
    /// </summary>
    Queue<AssetItem> downLoad_Asset_Que = new Queue<AssetItem>();
    Version local_Version;
    /// <summary>
    /// 资源服务器清单文件
    /// </summary>
    string resList_ConfigStr;
    void Start()
    {
        // 测试网络连接
        StartCoroutine(TestNetworkConnection());
        DownLoad_SeverVersion();
        
    }
    /// <summary>
    /// 网络测试
    /// </summary>
    /// <returns></returns>
    IEnumerator TestNetworkConnection()
    {
        string testUrl = "http://127.0.0.1/Main/";
        using (UnityWebRequest request = UnityWebRequest.Head(testUrl))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ 服务器连接成功");
            }
            else
            {
                Debug.LogError($"❌ 服务器连接失败: {request.error}");
                Debug.LogError("请确保:");
                Debug.LogError("1. 本地服务器正在运行");
                Debug.LogError("2. 服务器端口和路径正确");
                Debug.LogError("3. 防火墙没有阻止连接");
            }
        }
    }
    /// <summary>
    /// 下载服务器版本号
    /// </summary>
    void DownLoad_SeverVersion()
    {
        //资源服务器版本号文件地址
        string version = res_Sever_Url + "Version.txt";
        //文件下载回调
        StartCoroutine(DownLoad_Url_To_Local(version, (data) =>
        {
            if(data == null)
            {
                return;
            }
            //服务器版本号(最新)
            //记录资源服务器版本号数据
            string s_Version = Encoding.UTF8.GetString(data);
            res_Sever_Version = new Version(s_Version);
            //服务器版本号(最新)

            //本地版本号(最前玩家自己设备端最新)
            //本地不含有版本号文件夹时,为首次安装,需要全部下载
            if (!File.Exists(Application.persistentDataPath + "/Version.txt"))
            {
                Game_AllRes_DownLoad();
                return;
            }
            string local_Version_Str = File.ReadAllText(Application.persistentDataPath + "/Version.txt");
            local_Version = new Version(local_Version_Str);
            //本地版本号(最前玩家自己设备端最新)

            //大版本号(第一次游戏安装,需下载所有资源服务器的游戏资源(根据资源清单))
            if (res_Sever_Version.big > local_Version.big)
            {
                //平台 应用宝
                Game_AllRes_DownLoad();
            }
            else
            {
                //中版本更新
                if (res_Sever_Version.middle > local_Version.middle)
                {
                    //进入游戏中判断更新,但是更新完需要重新登录
                }
                else
                {
                    //小版本更新
                    if (res_Sever_Version.small > local_Version.small)
                    {
                        //进入游戏中判断更新,下载完直接进入游戏
                    }
                }
            }
        }));
    }
    /// <summary>
    /// 游戏所有资源下载
    /// </summary>
    void Game_AllRes_DownLoad()
    {
        //当前资源服务器最新的版本资源清单文件
        string resList_Config_Path = res_Sever_Url + res_Sever_Version.ToString() + "/ResList_Config.txt";
        StartCoroutine(DownLoad_Url_To_Local(resList_Config_Path, (data) =>
        {
            //当前版本的资源清单文件数据
            resList_ConfigStr = Encoding.UTF8.GetString(data);

            string[] resList = resList_ConfigStr.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);

            //将所有解析出来的资源用Queue记录,记录后下载
            foreach (var res in resList)
            {
                string[] resStrArr = res.Split('|');
                AssetItem ai = new AssetItem()
                {
                    path = resStrArr[0],
                    md5 = resStrArr[1],
                };
                downLoad_Asset_Que.Enqueue(ai);
            }
            //开启下载
            DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
        }));
    }
    /// <summary>
    /// 游戏根据版本进行资源下载
    /// </summary>
    void Game_Hotfix_Res_DownLoad()
    {
        //当前资源服务器最新的版本资源清单文件
        string resList_Config_Path = res_Sever_Url + res_Sever_Version.ToString() + "/ResList_Config.txt";
        StartCoroutine(DownLoad_Url_To_Local(resList_Config_Path, (data) =>
        {
            //当前版本的资源清单文件数据
            resList_ConfigStr = Encoding.UTF8.GetString(data);
            Dictionary<string, AssetItem> server_ResDict = Get_Res_AssetItems(resList_ConfigStr);
            //本地的资源清单数据
            string local_ResList_ConfigStr = File.ReadAllText(Application.persistentDataPath + local_Version.ToString() + "/ResList_Config.txt");
            Dictionary<string, AssetItem> local_ResDict = Get_Res_AssetItems(local_ResList_ConfigStr);
            //双端资源对比（1找出不同资源数据存储,2差异化的资源直接存储）
            ComPare_Server_And_Local_Res_To_Queue(server_ResDict, local_ResDict);
            //开启下载
            DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
        }));
    }

    /// <summary>
    /// 返回一个资源字典
    /// </summary>
    /// <param name="resStr"></param>
    /// <returns></returns>
    Dictionary<string, AssetItem> Get_Res_AssetItems(string resStr)
    {
        Dictionary<string, AssetItem> res_Dict = new Dictionary<string, AssetItem>();
        string[] resList = resStr.Trim().Split(new string[] { "\r\n" }, StringSplitOptions.None);
        foreach (var res in resList)
        {
            string[] resStrArr = res.Split('|');
            AssetItem ai = new AssetItem()
            {
                path = resStrArr[0],
                md5 = resStrArr[1],
            };
            res_Dict.Add(ai.path, ai);
        }
        return res_Dict;
    }

    /// <summary>
    /// 对比服务器与本地的资源找到需要下载的资源加入到下载队列
    /// </summary>
    /// <param name="s_ResDict"></param>
    /// <param name="l_ResDict"></param>
    void ComPare_Server_And_Local_Res_To_Queue(Dictionary<string, AssetItem> s_ResDict, Dictionary<string, AssetItem> l_ResDict)
    {
        //服务器的资源信息--筛出1:新增 2:MD5不同需要替换 两种状态
        foreach (var s_Res in s_ResDict)
        {
            //与本地资源信息对比
            if (l_ResDict.ContainsKey(s_Res.Key))
            {
                //判断双端都有的资源MD5是否相同
                if (s_Res.Value.md5 != l_ResDict[s_Res.Key].md5)
                {
                    //将双端都存在的Md5不同的资源放入下载队列
                    downLoad_Asset_Que.Enqueue(s_Res.Value);
                }

            }
            else
            {
                //服务器存在 本地不存在的资源加入到下载队列
                downLoad_Asset_Que.Enqueue(s_Res.Value);
            }
        }
        //反向判断 能找出服务器不存在 本地存在资源 为删除状态
        foreach (var l_Res in l_ResDict)
        {
            if (!s_ResDict.ContainsKey(l_Res.Key))
            {
                //删除本地资源
                File.Delete(Application.persistentDataPath + "/" + l_Res.Value.path);
            }
        }
    }
    /// <summary>
    /// 具体下载某个Asset(Ab)包资源的处理
    /// </summary>
    /// <param name="aItem"></param>
    //void DownLoad_Asset_Handle(AssetItem aItem)
    //{
    //     string asset_Path= res_Sever_Url  + aItem.path;
    //    //string asset_Path = res_Sever_Url  + aItem.path;
    //    StartCoroutine(DownLoad_Url_To_Local(asset_Path, (data) =>
    //    { 

    //        string local_Asset_Path = Application.persistentDataPath+"/"+aItem.path;
    //        //本地的资源下载与更新
    //        if(File.Exists(local_Asset_Path))
    //        {
    //            File.Delete(local_Asset_Path);
    //        }

    //        //判断是否缺少资源路径中的文件夹,不存在哪个就创建哪个
    //        if(!Directory.Exists(Path.GetDirectoryName(local_Asset_Path)))
    //        {
    //            Directory.CreateDirectory(Path.GetDirectoryName(local_Asset_Path));
    //        }
    //        //用字节流写到对应设置的位置
    //        File.WriteAllBytes(local_Asset_Path, data);
    //        if(downLoad_Asset_Que.Count>0)
    //        {
    //            DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
    //        }
    //        else
    //        {

    //            Save_GameVersion();
    //            Save_GameResList_Config();
    //            //进入游戏 or 退出游戏重启
    //        }
    //    }));
    //}
    void DownLoad_Asset_Handle(AssetItem aItem)
    {
        // 确保URL末尾没有多余的斜杠
        string baseUrl = res_Sever_Url.TrimEnd('/');
        string asset_Path = baseUrl + "/" + aItem.path;

        Debug.Log($"开始下载: {asset_Path}");
        StartCoroutine(DownLoad_Url_To_Local(asset_Path, (data) =>
        {
            Debug.Log($"下载完成，数据大小: {data?.Length ?? 0} 字节");

            if (data == null || data.Length == 0)
            {
                Debug.LogError($"下载的文件为空！路径: {asset_Path}");
                return;
            }

            string local_Asset_Path = Application.persistentDataPath + "/" + aItem.path;
            Debug.Log($"保存到本地: {local_Asset_Path}");

            // 确保目录存在
            string directory = Path.GetDirectoryName(local_Asset_Path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 写入文件
            File.WriteAllBytes(local_Asset_Path, data);
            Debug.Log($"文件保存成功，大小: {data.Length} 字节");

            // 验证保存的文件
            if (File.Exists(local_Asset_Path))
            {
                FileInfo fileInfo = new FileInfo(local_Asset_Path);
                Debug.Log($"保存后的文件大小: {fileInfo.Length} 字节");
            }

            if (downLoad_Asset_Que.Count > 0)
            {
                DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
            }
            else
            {
                Save_GameVersion();
                Save_GameResList_Config();
            }
        }));
    }
    /// <summary>
    /// 通过访问URL地址进行下载所需资源
    /// </summary>
    /// <param name="res_URL"></param>
    /// <param name="complete"></param>
    /// <returns></returns>
    //IEnumerator DownLoad_Url_To_Local(string res_URL, Action<byte[]> complete)
    //{
    //    ////访问对应的资源服务器
    //    //UnityWebRequest u_Web =UnityWebRequest.Get(res_URL);
    //    ////访问结果
    //    //UnityWebRequestAsyncOperation op = u_Web.SendWebRequest();
    //    ////询问是否访问成功
    //    //if(op.isDone)
    //    //{
    //    //    complete?.Invoke(u_Web.downloadHandler.data);
    //    //}
    //    //yield return null;
    //    Debug.Log($"开始下载: {res_URL}");

    //    using (UnityWebRequest u_Web = UnityWebRequest.Get(res_URL))
    //    {
    //        // ✅ 正确写法：等待下载完成
    //        yield return u_Web.SendWebRequest();

    //        // 检查是否成功
    //        if (u_Web.result == UnityWebRequest.Result.Success)
    //        {
    //            Debug.Log($"✅ 下载成功: {res_URL}, 大小: {u_Web.downloadHandler.data.Length} 字节");
    //            complete?.Invoke(u_Web.downloadHandler.data);
    //        }
    //        else
    //        {
    //            Debug.LogError($"❌ 下载失败: {res_URL}");
    //            Debug.LogError($"错误: {u_Web.error}");
    //            Debug.LogError($"状态码: {u_Web.responseCode}");

    //            // 如果下载失败，返回空数据，让后续逻辑能继续
    //            complete?.Invoke(new byte[0]);
    //        }
    //    }
    //}
    IEnumerator DownLoad_Url_To_Local(string res_URL, Action<byte[]> complete)
    {
        Debug.Log($"开始下载: {res_URL}");

        using (UnityWebRequest u_Web = UnityWebRequest.Get(res_URL))
        {
            // 设置超时
            u_Web.timeout = 30;

            // 发送请求并等待
            yield return u_Web.SendWebRequest();

            // 检查结果
            if (u_Web.result == UnityWebRequest.Result.Success)
            {
                byte[] data = u_Web.downloadHandler.data;
                Debug.Log($"✅ 下载成功: {res_URL}, 大小: {data?.Length ?? 0} 字节");

                // 验证数据
                if (data != null && data.Length > 0)
                {
                    // 可选：验证MD5
                    string downloadedMd5 = GetMD5FromBytes(data);
                    // 可以在这里比对MD5
                }

                complete?.Invoke(data);
            }
            else
            {
                Debug.LogError($"❌ 下载失败: {res_URL}");
                Debug.LogError($"错误: {u_Web.error}");
                Debug.LogError($"状态码: {u_Web.responseCode}");

                // 不要返回空数组，返回null以便上层判断
                complete?.Invoke(null);
            }
        }
    }

    // 辅助方法：从字节数组计算MD5
    string GetMD5FromBytes(byte[] data)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
    /// <summary>
    /// 保存当前游戏版本号数据
    /// </summary>
    void Save_GameVersion()
    {
        File.WriteAllText(Application.persistentDataPath + "/Version.txt", res_Sever_Version.ToString());
    }
    /// <summary>
    /// 保存当前游戏资源清单数据
    /// </summary>
    void Save_GameResList_Config()
    {
        // File.WriteAllText(Application.persistentDataPath +local_Version+ "/ResList_Config.txt",resList_ConfigStr);
        File.WriteAllText(Application.persistentDataPath + "/" + res_Sever_Version.ToString() + "/ResList_Config.txt", resList_ConfigStr);
    }
}
/// <summary>
/// 游戏版本号
/// </summary>
public class Version
{
    public int big;
    public int middle;
    public int small;
    public Version(string verStr)
    {
        string[] vList = verStr.Split('.');
        big = int.Parse(vList[0]);
        middle = int.Parse(vList[1]);
        small = int.Parse(vList[2]);

    }
    public override string ToString()
    {
        return big + "." + middle + "." + small;
    }

}
/// <summary>
/// AB资源信息
/// </summary>
public class AssetItem
{
    public string path;
    public string md5;
}
