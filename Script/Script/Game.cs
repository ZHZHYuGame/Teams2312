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

    public Button btn;

    public Text txt;

    public InputField inputF;

    // Start is called before the first frame update
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

        MessageControll.GetInstance().AddListener(NetMsg_ID.S_To_C_WorldChat_Msg, S_To_C_WorldChat_Msg_Handle);
    }

    private void S_To_C_WorldChat_Msg_Handle(object obj)
    {
        object[] objList = obj as object[];

        byte[] data = objList[0] as byte[];
        S_To_C_WorldChat_Msg s_World_Msg = S_To_C_WorldChat_Msg.Parser.ParseFrom(data);

        txt.text += $"{s_World_Msg.Speak}对大家说：{s_World_Msg.YexyDesc}\r\n";
    }

    // Update is called once per frame
    void Update()
    {
        net.Update();
    }
}
