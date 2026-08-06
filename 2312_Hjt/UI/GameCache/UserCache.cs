using System.Collections;
using System.Collections.Generic;
using UI.Data;
using UnityEngine;

/// <summary>
/// 玩家角色进入游戏后一些必须缓存显示的数据
/// </summary>
public class UserCache:Singleton<UserCache>
{
   /// <summary>
   /// 角色的基本信息
   /// </summary>
   public UserInfo userInfo;
   /// <summary>
   /// 角色的人物信息
   /// </summary>
   public UserTaskInfo  userTaskInfo;
   /// <summary>
   /// 角色的背包信息  
   /// </summary>
   public UserBagInfo userBagInfo;
}
