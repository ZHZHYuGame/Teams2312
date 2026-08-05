using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GameEnum;

/// <summary>
/// 主控界面数据层
/// 1.缓存--数据结构
/// 2.数据增加
/// 3.数据删除
/// 4.数据修改
/// 5.数据查找(外部通过M层)
/// 6.数据(获取外部数据)(游戏内部的XXX数据缓存 or 游戏内的配置数据缓存)
/// 7.接收数据事件
/// 8.通知数据事件
/// </summary>
public class MainModel:IModel
{
    public static string ModelName = "ModelName";

    public string modelName { get; }


    /// <summary>
    /// 玩家基本信息（主控界面左上角显示数据）
    /// </summary>
    public UserInfo userInfo { get; set; }
    /// <summary>
    /// 玩家任务信息
    /// </summary>
    UserTaskInfo userTaskInfo;
    /// <summary>
    /// 进入游戏的时候，初始化主控界面的数据显示
    /// </summary>
    public void Start()
    {
        userInfo = UserCache.GetInstance().userInfo;
        userTaskInfo= UserCache.GetInstance().userTaskInfo;
    }
    /// <summary>
    /// 不管几个属性变化，整体一次全部刷新
    /// </summary>
    public void UpdateUserInfo()
    {
        userInfo = UserCache.GetInstance().userInfo;
        //通知主控界面刷新修改等级信息
    }
    /// <summary>
    /// 通过某个属性类型，进行赋值刷新
    /// </summary>
    /// <param name="attType"></param>
    /// <param name="value"></param>
    public void UpdateUserInfo(UserAttType aType,int value)
    {
        switch (aType)
        {
            case UserAttType.name:
                break;
            case UserAttType.level:
                userInfo.userLevel = value;
                break;
            case UserAttType.exp:
                break;
            case UserAttType.vipLevel:
                break;
        }
        //通知主控界面刷新修改等级信息
    }
}