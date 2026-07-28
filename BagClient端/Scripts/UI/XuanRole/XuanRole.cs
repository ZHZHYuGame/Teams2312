using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf;
using Kuanjia;
using MyGame;
using UnityEngine.SceneManagement;

public class XuanRole : MonoBehaviour
{
    [SerializeField] private Button btn_CreateRole,btn_StartGame;
    [SerializeField] private GameObject CreateRole;
    [SerializeField] private Transform content;

    [SerializeField] private Text Tipstext;
    // Start is called before the first frame update
    void Start()
    {
        btn_CreateRole.onClick.AddListener((() =>
        {
            CreateRole.gameObject.SetActive(true);
        }));
        btn_StartGame.onClick.AddListener((() =>
        {
            C_2_S_Role_EnterGame_Msg msg = new C_2_S_Role_EnterGame_Msg();
            msg.RoleGUID = NetManager.GetInstance().RoleGuid;
            NetManager.GetInstance().SendMessage_To_Server(NewID.C_To_S_Role_EnterGame_Msg,msg.ToByteArray());
            //客户端发送给服务器所以C_To_S
        }));
        MessageControll.GetInstance().AddListener(NewID.S_To_C_Get_Role_List_Msg,GetRoleListHandle);
        MessageControll.GetInstance().AddListener(NewID.S_To_C_Role_EnterGame_Msg,EnterGame);
    }

    private async void EnterGame(object obj)
    {
        object[] objList = obj as object[];
        byte[] byteData=objList[0] as byte[];
        S_2_C_Role_EnterGame_Msg msg=S_2_C_Role_EnterGame_Msg.Parser.ParseFrom(byteData);;
        
        switch (msg.RoleEnterGameResult)
        {
            case Role_EnterGame_Result.EnterGame:
                SceneManager.LoadScene("Game");
                await Task.Delay(10);
                for (int i = 0; i < NetManager.GetInstance().roles.Count; i++)
                {
                    if (NetManager.GetInstance().roles[i].Roleid == msg.RoleGUID)
                    {
                        GameObject go=GameObject.Instantiate(Resources.Load<GameObject>(NetManager.GetInstance().roles[i].Path));
                        go.transform.position=Vector3.zero;
                        go.AddComponent<PlayerController>();
                        go.name = NetManager.GetInstance().roles[i].RoleName;
                        go.tag = "Player";
                    }
                }
               
                break;
            case Role_EnterGame_Result.NoRole:
                Tipstext.text = "请选择角色";
                break;
        
        }
        
    }

    private void GetRoleListHandle(object obj)
    {
        object[] objList=obj as object[];
        byte[] byteData = objList[0] as byte[];
        S_To_C_RoleList msg=S_To_C_RoleList.Parser.ParseFrom(byteData);
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.transform.GetChild(i).gameObject);
            NetManager.GetInstance().roles.Clear();
        }
        for (int i = 0; i < msg.Data.Count; i++)
        {
            RoleData Data = new RoleData(msg.Data[i].RoleName, msg.Data[i].Path, (int.Parse(msg.Data[i].Roleid)));
            XuanRoleitem go=Instantiate(Resources.Load<XuanRoleitem>("XuanRoleitem"),content);
            go.Init(Data);
            NetManager.GetInstance().roles.Add(Data);
        }

        ConfignManger.GetInstance().Money = msg.Money;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
