using System.Collections;
using System.Collections.Generic;
using UI.Base;
using UI.MainControll;
using UnityEngine;

/// <summary>
/// 主控界面逻辑层
/// 1网络数据
/// 2模块之间
/// </summary>
public class MainControll:BaseControll
{
    private MainModel m_Model;
    private MainView m_View;
    public MainControll()
    {
        
    }
    
    /// <summary>
    /// 添加网络通讯的消息事件触发的界面数据刷新显示
    /// </summary>
    void AddNetListener()
    {
        MessageControll.Instance.AddListener(NetMsg_ID.S_To_C_UserAttridute_Msg,S_To_C_UserAttridute_Msg_Handle);
    }

    private void S_To_C_UserAttridute_Msg_Handle(object obj)
    {
        m_Model.UpdateUserInfo();
       // m_Model.userInfo
       //m_View.xxx
    }

    /// <summary>
    /// 添加游戏中模块面板之间的消息事件触发的界面数据刷新显示
    /// </summary>
    void AddPanelListener()
    {
        
    }
    // public override void OnRegister()
    // {
    //     base.OnRegister();
    // }
    /// <summary>
    /// 初始化模块所需数据
    /// </summary>
    protected override void InitData()
    {
        m_Model = GameFacadeMgr.Instance.GetModel(MainModel.ModelName ) as MainModel;

    }
    
    /// <summary>
    /// ？？？
    /// </summary>
    protected override void BindUIData()
    {
        base.BindUIData();
    }

    /// <summary>
    /// 模块UI组件事件的注册
    /// </summary>
    protected override void BindUIEvnet()
    {
        base.BindUIEvnet();
    }
}
