using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using MyGame;

//using MyGame;

public class Game : MonoBehaviour
{
    NetManager net;
    public static Game ins;
    public Button btn;
    public Text txt;
    public InputField inputF;

    public InputField accountInput, passwordInput;
    public Button RegisterBtn, LoginBtn;
    public Image LoginPanel;

    //私聊
    public InputField input;
    public Button sendBtn;
    public Text sendBtnText;
    public Transform friendCell, friendTran;
    public string SpendChat;

    // Start is called before the first frame update
    private void Awake()
    {
        ins = this;
    }

    void Start()
    {
        net = new NetManager();
        net.Start();

        btn.onClick.AddListener(() =>
        {
            //net.SendMessage_To_Server();
            //序列化发送的PB消息数据
            C_To_S_WorldChat_Msg c_World_Msg = new C_To_S_WorldChat_Msg();
            //双端通信内容
            c_World_Msg.TextDesc = inputF.text;
            //PB协议序列化字节流
            net.SendMessage_To_Server(NetMsg_ID.C_To_S_WorldChat_Msg, c_World_Msg.ToByteArray());
        });

        //登录注册
        RegisterBtn.onClick.AddListener(() =>
        {
            C_To_S_Register_Msg c_Register_Msg = new C_To_S_Register_Msg();
            c_Register_Msg.Username = accountInput.text;
            c_Register_Msg.Password = passwordInput.text;
            net.SendMessage_To_Server(NetMsg_ID.C_To_S_Register_Message, c_Register_Msg.ToByteArray());
        });
        LoginBtn.onClick.AddListener(() =>
        {
            C_To_S_Login_Msg c_Login_Msg = new C_To_S_Login_Msg();
            c_Login_Msg.Username = accountInput.text;
            c_Login_Msg.Password = passwordInput.text;
            net.SendMessage_To_Server(NetMsg_ID.C_To_S_Login_Message, c_Login_Msg.ToByteArray());
        });
        sendBtn.onClick.AddListener(() =>
        {
            C_2_S_Chat_msg c_Prive_Msg = new C_2_S_Chat_msg();
            c_Prive_Msg.Infos = new ChatInfos();
            c_Prive_Msg.Infos.SpeakDesc = input.text;
            c_Prive_Msg.Infos.ToSpeak = SpendChat;
            if (SpendChat != null)
            {
                net.SendMessage_To_Server(NetMsg_ID.C_To_S_Private_Msg, c_Prive_Msg.ToByteArray());
            }
            else
            {
                net.SendMessage_To_Server(NetMsg_ID.C_To_S_WorldChat_Msg, c_Prive_Msg.ToByteArray());
            }
        });

        MessageControll.GetInstance().AddListener(NetMsg_ID.S_To_C_Private_Msg, S_To_C_Private_Msg_Handle);
        MessageControll.GetInstance().AddListener(NetMsg_ID.S_To_C_WorldChat_Msg, S_To_C_WorldChat_Msg_Handle);
        MessageControll.GetInstance().AddListener(NetMsg_ID.S_To_C_Register_Message, S_To_C_Register_MSg_Handle);
        MessageControll.GetInstance().AddListener(NetMsg_ID.S_To_C_Login_Message, S_To_C_Login_MSg_Handle);
    }

    private void S_To_C_Private_Msg_Handle(object obj)
    {
        // Debug.Log((123123));
        object[] objList = obj as object[];
        byte[] data = objList[0] as byte[];
        S_2_C_Chat_msg s_Register_Msg = S_2_C_Chat_msg.Parser.ParseFrom(data);
        sendBtnText.text +=
            $"{s_Register_Msg.Infos.Speak}对{s_Register_Msg.Infos.ToSpeak}说{s_Register_Msg.Infos.SpeakDesc}\r\n";
    }

    private void S_To_C_Register_MSg_Handle(object obj)
    {
        object[] objList = obj as object[];
        byte[] data = objList[0] as byte[];
        S_To_C_Register_Msg s_Register_Msg = S_To_C_Register_Msg.Parser.ParseFrom(data);
        Debug.Log(s_Register_Msg.Message);
    }

    private void S_To_C_Login_MSg_Handle(object obj)
    {
        try
        {
            object[] objList = obj as object[];
            byte[] data = objList[0] as byte[];
            S_To_C_Login_Msg s_Login_Msg = S_To_C_Login_Msg.Parser.ParseFrom(data);
            Debug.Log(s_Login_Msg.Message);
            if (s_Login_Msg.Result == Login_Result.LoginSucc)
            {
                LoginPanel.gameObject.SetActive(false);
                for (int i = 0; i < s_Login_Msg.FriendList.Count; i++)
                {
                    var ce = Instantiate(friendCell, friendTran);
                    ce.GetComponent<FriendItem>().Init(s_Login_Msg.FriendList[i]);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void S_To_C_WorldChat_Msg_Handle(object obj)
    {
        object[] objList = obj as object[];

        byte[] data = objList[0] as byte[];
        S_2_C_Chat_msg s_World_Msg = S_2_C_Chat_msg.Parser.ParseFrom(data);

        sendBtnText.text += $"{s_World_Msg.Infos.Speak}对大家说：{s_World_Msg.Infos.SpeakDesc}\r\n";
    }

    // Update is called once per frame
    void Update()
    {
        net.Update();
    }
}