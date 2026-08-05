using Games;
using Google.Protobuf;
using Newtonsoft.Json;
using System.Collections.Generic;
using Google.Protobuf.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 创建角色 
/// </summary>
public class CreateRole : MonoBehaviour
{
    [SerializeField] CreateRoleItem createRoleItem;// 创建 角色 item
    [SerializeField] RoleItem roleItem;// 选角item
    [SerializeField] Transform crRoot; // 创建角色root
    [SerializeField] Transform rRoot;  // 选角root
    [SerializeField] Transform objroot;// 角色 root
    [SerializeField] GameObject createRoleBtn, gameBtn; // 激活失活 按钮

    public static CreateRole ins;
    int jobType;
    public InputField rName;

    [SerializeField] Text job_text, desc_text;
    [SerializeField] GameObject playerObj;

    List<Job> jobList;
    public void Awake()
    {
        if (ins == null)
        {
            ins = this;
        }
        Debug.Log(456);
        MessageManager.Instance.Addlisternr(NetMsg_ID.OpenCreateRole, OnOpen);
        MessageManager.Instance.Addlisternr<byte[]>(NetMsg_ID.S_2_C_Get_Roel_List_Msg, Init);
        MessageManager.Instance.Addlisternr<byte[]>(NetMsg_ID.S_2_C_CreateRole_Msg, S_CreateRoleMsg_Handle);
        jobList = JsonConvert.DeserializeObject<List<Job>>(Resources.Load<TextAsset>("job").text);

        for (int i = 0; i < jobList.Count; i++)
        {
            RoleItem item = Instantiate(roleItem, rRoot);
            item.Init(jobList[i]);
        }

        roleItem.gameObject.SetActive(false);

        rRoot.gameObject.SetActive(false); //选择root

        createRoleBtn.SetActive(false);

        gameBtn.SetActive(false);

        Close();
    }

    /// <summary>
    /// 处理创建人物的结果 成功刷新
    /// </summary>
    /// <param name="bytes"></param>
    private void S_CreateRoleMsg_Handle(byte[] bytes)
    {
        S_2_C_CreateRole_Msg S_CreateRole_Msg = S_2_C_CreateRole_Msg.Parser.ParseFrom(bytes);
        switch (S_CreateRole_Msg.R)
        {
            case CreateRole_Result.Succ:
                Debug.Log("人物创建成功");
                rName.gameObject.SetActive(false);
                createRoleBtn.gameObject.SetActive(false);
                RefreshCreateRole(S_CreateRole_Msg.Rlist);
                break;
            case CreateRole_Result.Noname:
                break;
            case CreateRole_Result.Counts:
                break;
        }
    }

    /// <summary>
    /// 消息过来 初始化 
    /// </summary>
    /// <param name="bytes"></param>
    private void Init(byte[] bytes)
    {
        Debug.Log("收服务器用户 人物数据列表");
        S_2_C_Get_Role_List_Msg S_GetRoloeList = S_2_C_Get_Role_List_Msg.Parser.ParseFrom(bytes);
        CreateUser cUser = S_GetRoloeList.CreateUser;
        RefreshCreateRole(cUser.List);
    }

    /// <summary>
    ///  创建 role 刷新
    /// </summary>
    /// <param name="list"></param>
    public void RefreshCreateRole(RepeatedField<CreateRoleData> list)
    {
        createRoleItem.gameObject.SetActive(true);

        for (int i = 0; i < crRoot.childCount; i++)
        {
            Destroy(crRoot.GetChild(i).gameObject);
        }

        for (int i = 0; i < list.Count; i++)
        {
            CreateRoleItem cRoleItem = GameObject.Instantiate(createRoleItem, crRoot);
            cRoleItem.Init(list[i]);
        }

        createRoleItem.gameObject.SetActive(false);
        CloseSelectClass();
    }

    /// <summary>
    ///  创建角色显示  开始游戏
    /// </summary>
    /// <param name="job"></param>
    public void Refresh(Job job)
    {
        Debug.Log(job.type);
        jobType = (int)job.type;
        Debug.Log(jobType);
        if (playerObj != null)
        {
            Destroy(playerObj);
        }
        switch (job.type) // 赋值职业
        {
            case RoleJobType.none:
                break;
            case RoleJobType.Warrior:
                job_text.text = "战士";
                break;
            case RoleJobType.Mage:
                job_text.text = "法师";
                break;
            case RoleJobType.Assassin:
                job_text.text = "刺客";
                break;
        }

        // 显示描述
        desc_text.text = job.desc;
        // 创建角色
        playerObj = GameObject.Instantiate(Resources.Load<GameObject>(job.name), objroot);
        //显示创建角色
        createRoleBtn.gameObject.SetActive(true);
        rName.gameObject.SetActive(true);
        gameBtn.gameObject.SetActive(false);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="mydata"></param>
    public void Refresh(CreateRoleData mydata)
    {
        jobType = mydata.JobType;

        if (playerObj != null)
        {
            Destroy(playerObj);
        }

        Job job = jobList.Find(x => x.name == mydata.RolePrefabID);

        if (job != null)
        {
            switch (job.type)
            {
                case RoleJobType.none:
                    break;
                case RoleJobType.Warrior:
                    job_text.text = "战士";
                    break;
                case RoleJobType.Mage:
                    job_text.text = "法师";
                    break;
                case RoleJobType.Assassin:
                    job_text.text = "刺客";
                    break;
            }

            desc_text.text = job.desc;

            playerObj = GameObject.Instantiate(Resources.Load<GameObject>(job.name), objroot);

            createRoleBtn.gameObject.SetActive(false);

            rName.gameObject.SetActive(false);

            gameBtn.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 打开
    /// </summary>
    public void OnOpen()
    {
        Debug.Log("打开了人物数据");
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 创建角色
    /// </summary>
    public void OnCreateRole()
    {
        if (rName.text != "")
        {
            Debug.Log("客户端申请创建角色");
            C_2_S_CreateRole_Msg C_CreateRole = new C_2_S_CreateRole_Msg();
            Debug.Log(jobType);
            C_CreateRole.JobType = jobType;
            C_CreateRole.JobName = rName.text;

            NetManager.Instance.SendMessage_To_Server(NetMsg_ID.C_2_S_CreateRole_Msg, C_CreateRole.ToByteArray());
        }
        else
        {
            Debug.Log("还没有起名字");
        }
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void KaiShi()
    {
        Close();
    }

    /// <summary>
    ///  打他开选择职业 
    /// </summary>
    public void OpenSelectClass()
    {
        crRoot.gameObject.SetActive(false);
        rRoot.gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭选择职业
    /// </summary>
    public void CloseSelectClass()
    {
        crRoot.gameObject.SetActive(true);
        rRoot.gameObject.SetActive(false);
    }
}
/// <summary>
/// 职业
/// </summary>
public class Job
{
    public string name;
    public string desc;
    public string icon;
    public RoleJobType type;
}

/// <summary>
/// 人物类型
/// </summary>
public enum RoleJobType
{
    none = 0,//无
    Warrior = 1,//战士
    Mage = 2,//法师
    Assassin = 3,//刺客
}
