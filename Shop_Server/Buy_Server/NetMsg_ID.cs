using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 消息ID
/// </summary>
public class NetMsg_Id
{
    /// <summary>
    /// 在线玩家消息
    /// </summary>
    public static int Account_Msg = 1000;

    public const int S_2_C_OnLineList = 1001;

    public const int C_2_S_Login_Msg = 1002;

    public const int S_2_C_Login_Msg = 1003;

    public const int C_2_S_Register_Msg = 1004;

    public const int S_2_C_Register_Msg = 1005;

    public const int S_2_C_GoodsList_Msg = 1006;

    public const int S_2_C_JsonGoods_Msg = 1007;

    public const int C_2_S_Buy_Msg = 1008;
    public const int S_2_C_Buy_Msg = 1009;
    public const int C_2_S_Delet_Msg = 1010;
    public const int S_2_C_Delet_Msg = 1010;
}