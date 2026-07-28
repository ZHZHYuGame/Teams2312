using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 游戏中双端功能网络消息号定义
/// </summary>
public class NetMsg_ID
{
    /// <summary>
    /// 客户端向服务器请求世界聊天功能
    /// </summary>
    public const int C_To_S_WorldChat_Msg = 1001;
    /// <summary>
    /// 服务器回馈客户端世界聊天功能结果
    /// </summary>
    public const int S_To_C_WorldChat_Msg = 1002;

}
