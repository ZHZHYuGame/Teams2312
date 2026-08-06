using System;
using System.Collections;
using System.Collections.Generic;
using MVC;
using UnityEngine;

public enum MsgName
{
    
}
public class MessageManager<T> : Singleton<MessageManager<T>>
{
    private Dictionary<MsgName, Action<T>> dic = new();

    public void AddListener(MsgName msgName, Action<T> callback)
    {
        if (dic.ContainsKey(msgName))
        {
            dic[msgName] += callback;
        }
        else
        {
            dic.Add(msgName, callback);
        }
    }

    public void BroadCast(MsgName msgName, T callback)
    {
        if (dic.ContainsKey(msgName))
        {
            dic[msgName](callback);
        }
    }

    public void RemoveListener(MsgName msgName, Action<T> callback)
    {
        if (dic.ContainsKey(msgName))
        {
            dic[msgName] -= callback;
        }
    }
}
