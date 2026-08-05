using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using Games;
using System;
/// <summary>
/// 登录
/// </summary>
public class Login : MonoBehaviour
{
    [SerializeField] InputField account; //用户账号
    [SerializeField] InputField password; //用户密码

    private void Awake()
    {
        MessageManager.Instance.Addlisternr(NetMsg_ID.OpenLogin, OnOpen);
        MessageManager.Instance.Addlisternr<byte[]>(NetMsg_ID.S_TO_C_Login_Msg, Login_Message_Handle);
        OnClose();
    }

    private void Login_Message_Handle(byte[] bytes)
    {
        Debug.Log("登录收到服务器的消息");
        S_TO_C_Login_Message S_Login = S_TO_C_Login_Message.Parser.ParseFrom(bytes);

        switch (S_Login.Result)
        {
            case LoginResult.LoginSucc: //登录成功
                Debug.Log("成功1");
                UserMangaer.Instance.Login(account.text, password.text);
                Debug.Log("成功2");
                MessageManager.Instance.BroadCast(NetMsg_ID.OpenCreateRole);
                Debug.Log("登录成功");
                OnClose();
                break;
            case LoginResult.LoginNoaccount://input  用户名是空的
                Debug.Log("登录失败,请输入用户名");
                break;
            case LoginResult.LoginNohaveaccount://用户名不存在
                Debug.Log("登录失败,不存在用户名");
                break;
            case LoginResult.LoginNopassword://input  密码是空的
                Debug.Log("登录失败,不存在用户名");
                break;
            case LoginResult.LoginNohavepassword://密码错误
                Debug.Log("登录失败,不存在用户名");
                break;
            case LoginResult.Onlinetologin:// 用户在线
                Debug.Log("不存在用户名");
                break;
        }
    }

    /// <summary>
    /// 打开
    /// </summary>
    public void OnOpen()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void OnClose()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 跳转到注册界面
    /// </summary>
    public void JumpRegister()
    {
        OnClose();
        MessageManager.Instance.BroadCast(NetMsg_ID.OpenRefister);
    }

    /// <summary>
    /// 点击登录
    /// </summary>
    public void OnLogin() // 发给服务器 登录
    {
        C_TO_S_Login_Message C_To_Login_msg = new C_TO_S_Login_Message();
        C_To_Login_msg.Account = account.text;
        C_To_Login_msg.Password = password.text;
        NetManager.Instance.SendMessage_To_Server(NetMsg_ID.C_TO_S_Login_Msg, C_To_Login_msg.ToByteArray());
    }
}
