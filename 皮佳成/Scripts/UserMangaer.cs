using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用户管理
/// </summary>
public class UserMangaer : Singleton<UserMangaer>
{
    User user;
    /// <summary>
    /// 登录成功 记录 自己登录的数据
    /// </summary>
    /// <param name="account"></param>
    /// <param name="password"></param>
    public void Login(string account, string password)
    {
        user = new User(account, password);
    }
}
