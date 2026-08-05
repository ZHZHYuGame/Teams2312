using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class MessageManager : Singleton<MessageManager>
{
    Dictionary<int, Delegate> dic = new Dictionary<int, Delegate>();

    public void Addlisternr(int key, Action action)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = action as Action + action;
        }
        else
        {
            dic.Add(key, action);
        }
    }

    public void Remove(int key, Action action)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = action as Action - action;
        }
        else
        {
            dic.Remove(key);
        }
    }

    public void BroadCast(int key)
    {
        if (dic.ContainsKey(key))
        {
            Action action = dic[key] as Action;
            if (action != null)
            {
                action();
            }
        }
    }

    public void Addlisternr<T>(int key, Action<T> action)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = action as Action<T> + action;
        }
        else
        {
            dic.Add(key, action);
        }
    }

    public void Remove<T>(int key, Action<T> action)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = action as Action<T> - action;
        }
        else
        {
            dic.Remove(key);
        }
    }

    public void BroadCast<T>(int key, T t)
    {
        if (dic.ContainsKey(key))
        {
            Action<T> action = dic[key] as Action<T>;
            if (action != null)
            {
                action(t);
            }
        }
    }
    public void Addlisternr<T, M>(int key, Action<T, M> action)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = action as Action<T, M> + action;
        }
        else
        {
            dic.Add(key, action);
        }
    }

    public void Remove<T, M>(int key, Action<T, M> action)
    {
        if (dic.ContainsKey(key))
        {
            dic[key] = action as Action<T, M> - action;
        }
        else
        {
            dic.Remove(key);
        }
    }

    public void BroadCast<T, M>(int key, T t, M m)
    {
        if (dic.ContainsKey(key))
        {
            Action<T, M> action = dic[key] as Action<T, M>;
            if (action != null)
            {
                action(t, m);
            }
        }
    }

}





public enum MesKey
{
    str,
    //客户端向服务器请求世界聊天功能
    C_TO_S_WoridChat_msg,
    //服务器回馈客户端世界聊天
    S_TO_WoridChat_Meg,
    //打开注册面板
    OpenRefister,
    //打开登录面板
    OpenLogin
}
