using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Games;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using Kuanjia;
using MyGame;
using Object = System.Object;

public class Game : MonoBehaviour
{
    

    public Button btn,btn_Login,btn_Resigter;
    [SerializeField] private Transform CreateRole;
    public Text txt;
    public InputField input,Accountinput,Passwordinput;
   
    public Transform XuanRole;

   
    // Start is called before the first frame update
    void Start()
    {
      
        DontDestroyOnLoad(gameObject);
        NetManager.GetInstance().Start();
       ConfignManger.GetInstance().InitConfign();
        // btn.onClick.AddListener(() =>
        // {
        //     //虚拟化pb
        //    // net.SendMessage_To_Server();
        //    C_To_S_WorldChat_Msg msg = new C_To_S_WorldChat_Msg();
        //    msg.TextDesc=input.text;
        //    net.SendMessage_To_Server(NewID.C_To_S_WorldChat_Msg,msg.ToByteArray());
        // });
        btn_Resigter.onClick.AddListener((() =>
        {
            C_To_S_Register_Message msg = new C_To_S_Register_Message();
            msg.Account=Accountinput.text;
            msg.Password=Passwordinput.text;
            NetManager.GetInstance().SendMessage_To_Server(NewID.C_To_S_Resigter_Msg,msg.ToByteArray());
            
        }));
        MessageControll.GetInstance().AddListener(NewID.S_To_C_Resigter_Msg,Resigter_Handle);
        btn_Login.onClick.AddListener((() =>
        {
            C_To_S_Login_Message msg = new C_To_S_Login_Message();
            msg.Account=Accountinput.text;
            msg.Password=Passwordinput.text;
            NetManager.GetInstance().SendMessage_To_Server(NewID.C_To_S_Login_Msg,msg.ToByteArray());
        }));
        MessageControll.GetInstance().AddListener(NewID.S_To_C_Login_Msg,Login_Handle);//这里的S指发送者,C指接收者其他同理
       // MessageControll.GetInstance().AddListener(NewID.S_To_C_WorldChat_Msg, Message_Handle);
    }

    private void Login_Handle(object obj)
    {
        object[] objList=obj as object[];
        byte[] byteData = objList[0] as byte[];
        S_To_C_Login_Message msg=S_To_C_Login_Message.Parser.ParseFrom(byteData);
        switch (msg.R)
        {
            case Login_Result.LoginSucc:
                txt.text ="登入成功";
                XuanRole.gameObject.SetActive(true);
            
                break;
            case Login_Result.LoginNoaccount:
                txt.text ="请输入账号";
                break;
            case Login_Result.LoginNohaveaccount:
                txt.text ="请先注册账号";
                break;
            case Login_Result.LoginNopassword:
                txt.text ="请输入密码";
                break;
            case Login_Result.LoginNohavepassword:
                txt.text ="密码不对";
                break;
            case Login_Result.LoginOnlinetologin:
                txt.text ="该账号已在线";
                break;
           
        }
    }

    private void Resigter_Handle(object obj)
    {
        object[] objList = obj as object[];
        byte[] byteData=objList[0] as byte[];
        S_To_C_Register_Message msg=S_To_C_Register_Message.Parser.ParseFrom(byteData);
        switch (msg.R)
        {
            case Register_Result.RegisterSucc:
                txt.text ="注册成功";
                
                break;
            case Register_Result.RegisterChf:
                txt.text ="不能重复注册";
                break;
            case Register_Result.RegisterNopassword:
                txt.text ="请输入密码";
                break;
            case Register_Result.RegisterNoaccount:
                txt.text ="请输入账号";
                break;
          
        }
        
    }

    private void Message_Handle(object obj)
    {
        object[] objList = obj as object[];
        byte[] data = objList[0] as byte[];
        S_To_C_WorldChat_Msgl s_World_Msg = S_To_C_WorldChat_Msgl.Parser.ParseFrom(data);
        txt.text += $"{s_World_Msg.Speak}对大家说{s_World_Msg.TextDesc}\r\n";
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.GetInstance().Update();
    }
}
