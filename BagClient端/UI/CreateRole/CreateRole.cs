using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using MyGame;
using Games;
public class CreateRole : MonoBehaviour
{
    public Dropdown dropdown;

    public Button btn_Create,btn_Exit;

    public InputField nameinput;
    public Text tipstxt;
    void Start()
    {
        btn_Exit.onClick.AddListener((() =>
        {
            gameObject.SetActive(false);
        }));
        btn_Create.onClick.AddListener((() =>
        {
            C_2_S_CreateRole_Msg msg = new C_2_S_CreateRole_Msg();
            msg.JobName = nameinput.text;
            switch (dropdown.captionText.text)
            {
                case "Zhanshi" :
                    msg.JobType = jobType.Zhanshi;
                    break;
                case "Fashi" :
                    msg.JobType = jobType.Fashi;
                    break;
                case "Daoshi" :
                    msg.JobType = jobType.Daoshi;
                    break;
            }
            NetManager.GetInstance().SendMessage_To_Server(NewID.C_To_S_CreateRole_Msg,msg.ToByteArray());
           
        }));
        MessageControll.GetInstance().AddListener(NewID.S_To_C_CreateRole_Msg,CreateRoleHandle);
    }

    private void CreateRoleHandle(object obj)
    {
        object[] objList=obj as object[];
        byte[] byteData = objList[0] as byte[];
        
        S_2_C_CreateRole_Msg msg =S_2_C_CreateRole_Msg.Parser.ParseFrom(byteData);
        switch (msg.CreateRoleResult)
        {
            case CreateRoleResult.CreateRoleSuss:
                tipstxt.text = "创建成功";
                break;
            case CreateRoleResult.NoName:
                tipstxt.text = "创建失败,请输入角色名字";
                break;
            case CreateRoleResult.Weihu:
                tipstxt.text = "该角色维护中,无法创建";
                break;
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
