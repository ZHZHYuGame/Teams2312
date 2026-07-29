using System;
using System.Collections.Generic;

/// <summary>
/// 消息管理器
/// </summary>
public class MessageManager : Singleton<MessageManager>
{
    Dictionary<string, Action<object>> msgDict = new Dictionary<string, Action<object>>();

    // 添加消息监听
    public void AddMsg(string msgName, Action<object> action)
    {
        if (msgDict.ContainsKey(msgName))
        {
            msgDict[msgName] += action;
        }
        else
        {
            msgDict.Add(msgName, action);
        }
    }

    // 发送消息
    public void SendMsg(string msgName, object param = null)
    {
        if (msgDict.ContainsKey(msgName))
        {
            msgDict[msgName](param);
        }
    }

    // 移除消息
    public void RemoveMsg(string msgName, Action<object> action)
    {
        if (msgDict.ContainsKey(msgName))
        {
            msgDict[msgName] -= action;
        }
    }
}
