using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 热更新管理
/// 1.版本号
/// 2.资源清单
/// 3.资源
/// 资源服务器 Http
/// 拷贝
/// 上传
/// 下载
/// </summary>
public class HotfixManager : MonoBehaviour
{
    /// <summary>
    /// 资源服务器的地址（Https://）
    /// </summary>
    private string res_Server_Url = "http://127.0.0.1/2312A";

    /// <summary>
    /// 资源服务器的游戏版本号
    /// </summary>
    private Version res_Server_Version;
    
    /// <summary>
    /// 资源下载队列
    /// 1.第一次安装游戏必触发
    /// </summary>
    Queue<AssetItem> downLoad_Asset_Que = new Queue<AssetItem>();
    // Start is called before the first frame update
    void Start()
    {
        DownLoad_ServerVersion();
    }
    
    /// <summary>
    /// 下载服务器版本号
    /// </summary>
    void DownLoad_ServerVersion()
    {
        //资源服务器版本号文件地址
        string version = $"{res_Server_Url}/Version.txt";
        StartCoroutine(DownLoad_Url_To_Local(version, (data) =>
        {
            //服务器版本号（最新）
            //记录资源服务器版本号最新
            string s_Version = Encoding.UTF8.GetString(data);
            res_Server_Version = new Version(s_Version);
            //服务器版本号（最新）
            
            //本地版本号（当前玩家设备端最新）
            //本地不含有版本号文件时， 为首次安装， 需要全部下载
            if (!File.Exists($"{Application.dataPath}/Version.txt"))
            {
                Game_AllRes_DownLoad();
                return;
            }
            string local_Version_Str = File.ReadAllText($"{Application.dataPath}/Version.txt");
            Version local_Version = new Version(local_Version_Str);

            //大版本号
            if (res_Server_Version.big > local_Version.big)
            {
                //平台商城下载安装
                Game_AllRes_DownLoad();
            }
            else
            {
                //中版本
                if (res_Server_Version.middle > local_Version.middle)
                {
                    //进入游戏中判断更新、更完重新登录，重新编译
                }
                else
                {
                    
                    if (res_Server_Version.small > local_Version.small)
                    {
                        //进入游戏中判断更新、下载完直接进游戏
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
        //当前资源服务器最新版本资源清单文件
        string resList_Config_Path = $"{res_Server_Url}/{ res_Server_Version}ResList_Config.txt";
        StartCoroutine(DownLoad_Url_To_Local(resList_Config_Path, (data) =>
        {
            string resList_Config_Str = Encoding.UTF8.GetString(data);
            string[] resList = resList_Config_Str.Trim().Split(new string[]{"\r\n"}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var res in resList)
            {
                if (string.IsNullOrEmpty(res) || !res.Contains("|")) continue;
                string[] resStrArr = res.Split('|');
                AssetItem asset = new AssetItem()
                {
                    path = resStrArr[0],
                    md5 = resStrArr[1],
                };
                downLoad_Asset_Que.Enqueue(asset);
            }
            if(downLoad_Asset_Que.Count > 0)
                DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
        }));
    }

    private void DownLoad_Asset_Handle(AssetItem dequeue)
    {
        string asset_Path = res_Server_Url + dequeue.path;
        StartCoroutine(DownLoad_Url_To_Local(asset_Path, (data) =>
        {
            string local_Asset_Path = Application.persistentDataPath+"/"+dequeue.path;
            //本地的资源下载与更新
            if(File.Exists(local_Asset_Path))File.Delete(local_Asset_Path);
            
            if(!Directory.Exists(Path.GetDirectoryName(local_Asset_Path)))Directory.CreateDirectory(Path.GetDirectoryName(local_Asset_Path));
            File.WriteAllBytes(local_Asset_Path,data);
            if(downLoad_Asset_Que.Count > 0)
                DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
        }));
    }

    /// <summary>
    /// 通过访问URL地址下载所需资源
    /// </summary>
    /// <param name="res_URL"></param>
    /// <param name="complete"></param>
    /// <returns></returns>
    private IEnumerator DownLoad_Url_To_Local(string res_URL, Action<byte[]> complete)
    {
        //访问对应资源服务器资源
        UnityWebRequest request = UnityWebRequest.Get(res_URL);
        //访问结果
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        yield return operation;
        //访问是否完成
        if (request.isDone)
        {
            complete?.Invoke(request.downloadHandler.data);
        }
        else
        {
            Debug.LogError($"下载失败: {res_URL} - {request.error}");
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public Version(string version)
    {
        string[] vList = version.Split('.');
        
        big = int.Parse(vList[0]);
        middle = int.Parse(vList[1]);
        small = int.Parse(vList[2]);
    }
    public override string ToString()
    {
        return big+"."+middle+"."+small;
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