using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using MyGame;

/// <summary>
/// 聊天管理器
/// </summary>
public class AccountManager : Singleton<AccountManager>
{
    public AccountManager()
    {
    }

    /// <summary>
    /// 初始经聊天功能所有的功能相关
    /// 1.监听
    /// </summary>
    public void Start()
    {
        MessageControll.GetInstance().AddListener(NetMsg_Id.C_2_S_Login_Msg, C_To_S_Login_Msg_Handle);
        MessageControll.GetInstance().AddListener(NetMsg_Id.C_2_S_Register_Msg, C_To_S_Register_Msg_Handle);
    }

    /// <summary>
    /// 登录处理
    /// </summary>
    /// <param name="obj"></param>
    private void C_To_S_Login_Msg_Handle(object obj)
    {
        object[] objList = obj as object[];
        byte[] byteData = objList[0] as byte[];
        Client c = objList[1] as Client;
        //安全：有请求，有回馈，没有擅自操作

        //客户端数据反序列化
        C_2_S_Login_Msg c_Login_Msg = C_2_S_Login_Msg.Parser.ParseFrom(byteData);
        //序列化服务器回馈结果消息
        S_2_C_Login_Msg s_Login_Msg = new S_2_C_Login_Msg();
        //验证是否存有该账号
        var data = UserSQLMgr.GetInstance().Find(c_Login_Msg.Account);
        if (data != null && data.password == c_Login_Msg.Password)
        {
            s_Login_Msg.R = LoginResult.Logsucc;
            s_Login_Msg.Account = c_Login_Msg.Account;
            s_Login_Msg.Password = c_Login_Msg.Password;
            MessageControll.GetInstance().Dispach(NetMsg_Id.Account_Msg, s_Login_Msg.Account);
        }

        if (c_Login_Msg.Account == ""||c_Login_Msg.Password == "")
            s_Login_Msg.R = LoginResult.Null;
        if (UserSQLMgr.GetInstance().Find(c_Login_Msg.Account) == null||(UserSQLMgr.GetInstance().Find(c_Login_Msg.Account) != null && UserSQLMgr.GetInstance().Find(c_Login_Msg.Account).password != c_Login_Msg.Password))
            s_Login_Msg.R = LoginResult.Wrong;
        NetManager.GetInstance().SendNetMessage(c.st, NetMsg_Id.S_2_C_Login_Msg, s_Login_Msg.ToByteArray());
    }

    /// <summary>
    /// 注册处理
    /// </summary>
    /// <param name="obj"></param>
    private void C_To_S_Register_Msg_Handle(object obj)
    {
        object[] objList = obj as object[];
        byte[] byteData = objList[0] as byte[];
        Client c = objList[1] as Client;
        //客户端数据反序列化
        C_2_S_Register_Msg c_Register_Msg = C_2_S_Register_Msg.Parser.ParseFrom(byteData);

        //服务器世界聊天数据序列化
        S_2_C_Register_Msg s_Register_Msg = new S_2_C_Register_Msg();

        if (c_Register_Msg.Account != "" && c_Register_Msg.Password != "")
        {
            if (UserSQLMgr.GetInstance().Find(c_Register_Msg.Account)==null)
            {
                s_Register_Msg.R = RegisterResult.Regsucc;
                s_Register_Msg.Account = c_Register_Msg.Account;
                s_Register_Msg.Password = c_Register_Msg.Password;
                User user = new User(c_Register_Msg.Account, c_Register_Msg.Password);
                UserSQLMgr.GetInstance().SaveFile(user);
            }
            else
            {
                s_Register_Msg.R = RegisterResult.Repeat;
            }
        }

        if (c_Register_Msg.Account == ""||c_Register_Msg.Password == "")
            s_Register_Msg.R = RegisterResult.Nullt;
        NetManager.GetInstance().SendNetMessage(c.st, NetMsg_Id.S_2_C_Register_Msg, s_Register_Msg.ToByteArray());
    }
}