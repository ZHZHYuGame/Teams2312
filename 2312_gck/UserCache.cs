using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家角色进入游戏后一些必须缓存显示的数据
/// </summary>
public class UserCache:Singleton<UserCache>
{
    /// <summary>
    /// 角色基本信息
    /// </summary>
    public UserInfo userInfo;
    /// <summary>
    /// 角色任务信息
    /// </summary>
    public UserTaskInfo userTaskInfo;
    /// <summary>
    /// 角色背包信息
    /// </summary>
    public UserBagInfo userBagInfo;
}