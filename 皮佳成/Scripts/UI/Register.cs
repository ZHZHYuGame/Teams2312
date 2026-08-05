using Games;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using Games;

/// <summary>
/// 注册
/// </summary>
public class Register : MonoBehaviour
{
    [SerializeField] InputField account; //用户账号
    [SerializeField] InputField password; //用户密码

    private void Awake()
    {
        MessageManager.Instance.Addlisternr(NetMsg_ID.OpenRefister, OnOpen);
        MessageManager.Instance.Addlisternr<byte[]>(NetMsg_ID.S_TO_C_Register_Msg, Register_Message_Handle);
    }

    private void Register_Message_Handle(byte[] bytes)
    {
        S_To_C_Register_Message S_To_Register_Msg = S_To_C_Register_Message.Parser.ParseFrom(bytes);

        switch (S_To_Register_Msg.Result)
        {
            case RegisterResult.RegisterSucc:
                Debug.Log("注册成功");
                JumpLogin();
                break;
            case RegisterResult.Chf:
                Debug.Log("注册失败,用户名重复");
                break;
            case RegisterResult.RegisterNopassword:
                Debug.Log("注册失败,密码是空的");
                break;
            case RegisterResult.RegisterNoaccunt:
                Debug.Log("注册失败,账号是空的");
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
    public void JumpLogin()
    {
        OnClose();
        MessageManager.Instance.BroadCast(NetMsg_ID.OpenLogin);
    }

    /// <summary>
    /// 点击注册
    /// </summary>
    public void OnRegister() //发给服务器 注册
    {
        C_To_S_Register_Message C_To_Register_msg = new C_To_S_Register_Message();
        C_To_Register_msg.Account = account.text;
        C_To_Register_msg.Password = password.text;
        NetManager.Instance.SendMessage_To_Server(NetMsg_ID.C_TO_S_Register_Msg, C_To_Register_msg.ToByteArray());
    }
}
