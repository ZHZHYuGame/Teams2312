using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetMsg_ID
{
    /// <summary>
    /// 客户端向服务器请求世界聊天功能
    /// </summary>
    public const int C_TO_S_WoridChat_msg = 1001;
    /// <summary>
    /// 服务器回馈客户端世界聊天
    /// </summary>
    public const int S_TO_WoridChat_Meg = 1002;

    /// <summary>
    /// 客户端向服务器请求 登录验证
    /// </summary>
    public const int C_TO_S_Login_Msg = 1003;
    /// <summary>
    /// 服务器向回馈客户端 登录结果
    /// </summary>
    public const int S_TO_C_Login_Msg = 1004;


    /// <summary>
    /// 客户端向服务器请求 注册验证
    /// </summary>
    public const int C_TO_S_Register_Msg = 1005;
    /// <summary>
    /// 服务器向回馈客户端 注册结果
    /// </summary>
    public const int S_TO_C_Register_Msg = 1006;


    public const int C_2_S_CreateRole_Msg = 1007;

    public const int S_2_C_CreateRole_Msg = 1008;

    public const int C_2_S_Get_Roel_List_Msg = 1009;

    public const int S_2_C_Get_Roel_List_Msg = 1010;

    public const int C_2_S_Role_EnterGame_Msg = 1011;

    public const int S_2_C_Role_EnterGame_Msg = 1012;

    // 客户端 服务器
    public const int C_2_S_Caht_Msg = 1013;

    //服务器 客户端
    public const int S_2_C_Caht_Msg = 1014;

    /// <summary>
    /// 
    /// </summary>
    public const int S_2_C_OnLine_List_Msg = 1015;


    /// <summary>
    ///  服务器通知客户端 上线
    /// </summary>
    public const int S_2_C_OnLine_Add_Msg = 1016;

    /// <summary>
    /// 服务器 通知客户端 玩家离开
    /// </summary>
    public const int S_2_C_OnLine_Exit_Msg = 1017;


   
    /// <summary>
    /// 打开登录面板
    /// </summary>
    public const int OpenLogin = 1;

    /// <summary>
    /// 打开注册面板
    /// </summary>
    public const int OpenRefister = 2;

    /// <summary>
    /// 打开 人物选择列表
    /// </summary>
    public const int OpenCreateRole = 3;

}
