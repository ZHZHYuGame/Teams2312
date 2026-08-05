using System;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using MyGame;

public class Game : MonoBehaviour
{
    public Button btn;
    //public Text text;
    //public InputField Inputext;
    // Start is called before the first frame update
    void Start()
    {
        NetManager.Instance.Start();
        //世界聊天
        //btn.onClick.AddListener(() =>
        //{
        //    //NetManager.Instance.SendMessage_To_Server();
        //    //序列化
        //    C_To_S_WorldChat_Msg C_Worid_Msg = new C_To_S_WorldChat_Msg();
        //    //双端通信内容
        //    C_Worid_Msg.TextDesc = Inputext.text;
        //    //PB协议序列化字节内容
        //    NetManager.Instance.SendMessage_To_Server(NetMsg_ID.C_TO_S_WoridChat_msg, C_Worid_Msg.ToByteArray());
        //});

        //MessageManager.Instance.Addlisternr<byte[]>(MesKey.str, Message_Handle);
    }

    private void Message_Handle(byte[] byteds)
    {
        //世界聊天数据反序列化
        S_To_C_WoridChat_Msg c_Worid_msg = S_To_C_WoridChat_Msg.Parser.ParseFrom(byteds);

        //text.text += c_Worid_msg.Speak + "对世界说" + c_Worid_msg.TextDesc + "\r\n";
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Instance.Update();
    }

    private void OnApplicationQuit()
    {
        
    }
}
/// <summary>
/// 用户 数据
/// </summary>
public class User
{

    /// <summary>
    /// 用户名
    /// </summary>
    public string account;
    /// <summary>
    /// 用户密码
    /// </summary>
    public string password;

    public User(string account, string password)
    {
        this.account = account;
        this.password = password;
    }
}
