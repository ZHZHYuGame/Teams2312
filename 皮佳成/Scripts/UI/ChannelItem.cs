using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Games;
using UnityEngine.UI;

public class ChannelItem : MonoBehaviour
{
    ChatType myType;
    [SerializeField] Text text;

    public void Init(ChatType tyope)
    {
        myType = tyope;
        switch (myType)
        {
            case ChatType.Worid:
                text.text = "世界";
                break;
            case ChatType.Private:
                text.text = "私聊";
                break;
            case ChatType.Team:
                text.text = "组队";
                break;
            case ChatType.Guild:
                text.text = "公会";
                break;
            case ChatType.Near:
                text.text = "附近";
                break;
            case ChatType.Announcement:
                text.text = "公告";
                break;
        }
    }
    

    /// <summary>
    /// 切换聊天 频道
    /// </summary>
    public void OnChatType()
    {
        UIChat.ins.OnCahtType(myType);
    }

}
