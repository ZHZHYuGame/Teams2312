using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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
    string res_Sever_Url = "127.0.0.1/Demo8.5/";
    //资源服务器的游戏版本号
    Version res_Sever_Version;
    /// <summary>
    /// 资源下载队列
    /// 1.第一次安装游戏必触发
    /// 2.双端版本不一致的情况必触发
    /// </summary>
    Queue<AssetItem> downLoad_Asset_Que=new Queue<AssetItem>();
    void Start()
    {
        DownLoad_SeverVersion();
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
            //服务器版本号(最新)
            //记录资源服务器版本号数据
            string s_Version=Encoding.UTF8.GetString(data);
             res_Sever_Version = new Version(s_Version);
            //服务器版本号(最新)

            //本地版本号(最前玩家自己设备端最新)
            //本地不含有版本号文件夹时,为首次安装,需要全部下载
            if (!File.Exists(Application.persistentDataPath + "/Version.txt"))
            {
                Game_AllRes_DownLoad();
                return;
            }
            string local_Version_Str = File.ReadAllText(Application.persistentDataPath + "/Version.text"); 
            Version local_Version = new Version(local_Version_Str);
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
        string resList_Config_Path =res_Sever_Url+res_Sever_Version.ToString() + "/ResList_Config.txt";
        StartCoroutine(DownLoad_Url_To_Local(resList_Config_Path, (data) =>
        {
            //当前版本的资源清单文件数据
            string resList_ConfigStr=Encoding.UTF8.GetString(data);
     
            string[] resList = resList_ConfigStr.Trim().Split(new string[] {"\r\n"},StringSplitOptions.None);

            //将所有解析出来的资源用Queue记录,记录后下载
            foreach (var res in resList)
            {
                string[] resStrArr = res.Split('|');
                AssetItem ai = new AssetItem()
                {
                    path = resStrArr[0],
                    md5=resStrArr[1],
                };
                downLoad_Asset_Que.Enqueue(ai);
            }
            //开启下载
            DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
        }));
    }

    /// <summary>
    /// 具体下载某个Asset(Ab)包资源的处理
    /// </summary>
    /// <param name="aItem"></param>
    void DownLoad_Asset_Handle(AssetItem aItem)
    {
        //  string asset_Path= res_Sever_Url + res_Sever_Version.ToString() + aItem.path;
        string asset_Path = res_Sever_Url  + aItem.path;
        StartCoroutine(DownLoad_Url_To_Local(asset_Path, (data) =>
        { 
          
            string local_Asset_Path = Application.persistentDataPath+"/"+aItem.path;
            //本地的资源下载与更新
            if(File.Exists(local_Asset_Path))
            {
                File.Delete(local_Asset_Path);
            }
          
            //判断是否缺少资源路径中的文件夹,不存在哪个就创建哪个
            if(!Directory.Exists(Path.GetDirectoryName(local_Asset_Path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(local_Asset_Path));
            }
            //用字节流写到对应设置的位置
            File.WriteAllBytes(local_Asset_Path, data);
            if(downLoad_Asset_Que.Count>0)
            {
                DownLoad_Asset_Handle(downLoad_Asset_Que.Dequeue());
            }
            else
            {
                //进入游戏 or 退出游戏重启
            }
        }));
    }

    /// <summary>
    /// 通过访问URL地址进行下载所需资源
    /// </summary>
    /// <param name="res_URL"></param>
    /// <param name="complete"></param>
    /// <returns></returns>
    IEnumerator DownLoad_Url_To_Local(string res_URL, Action<byte[]> complete)
    {
        //访问对应的资源服务器
        UnityWebRequest u_Web =UnityWebRequest.Get(res_URL);
        //访问结果
        UnityWebRequestAsyncOperation op = u_Web.SendWebRequest();
        //询问是否访问成功
        if(op.isDone)
        {
            complete?.Invoke(u_Web.downloadHandler.data);
        }
        yield return null;
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
    public Version(string verStr)
    {
        string[] vList = verStr.Split('.');
        big= int.Parse(vList[0]);
        middle= int.Parse(vList[1]);
        small= int.Parse(vList[2]);
       
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
