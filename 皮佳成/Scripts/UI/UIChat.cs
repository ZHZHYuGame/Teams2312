using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using Games;
using UnityEngine.UI;

public class UIChat : MonoBehaviour
{
    [SerializeField] ChannelItem channelitem;
    [SerializeField] Transform channelRoot;
    [SerializeField] Transform chatRoot;
    [SerializeField] ChatItem chatItem;
    [SerializeField] Transform lineRoot;
    [SerializeField] LineItem line;

    ChatType type;

    List<LineItem> lineItems = new List<LineItem>();

    [SerializeField] InputField inputField;
    public static UIChat ins;

    Dictionary<ChatType, List<Chatinfo>> chatDic = new Dictionary<ChatType, List<Chatinfo>>();

    List<ChatItem> chatItems = new List<ChatItem>();
    private void Awake()
    {
        ins = this;
        MessageManager.Instance.Addlisternr<byte[]>(NetMsg_ID.S_2_C_OnLine_List_Msg, OnLienHandle);
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            ChannelItem citem = Instantiate(channelitem, channelRoot);
            citem.Init((ChatType)i);
        }
    }

    private void OnLienHandle(byte[] bytes)
    {
        S_2_C_OnLine_List_Msg S_OnLineMsg = S_2_C_OnLine_List_Msg.Parser.ParseFrom(bytes);
        for (int i = 0; i < S_OnLineMsg.List.Count; i++)
        {
            LineItem lineItem = Instantiate(line);
            lineItem.Init(S_OnLineMsg.List[i]);
            lineItems.Add(lineItem);
        }
    }

    /// <summary>
    /// 切换类型
    /// </summary>
    public void OnCahtType(ChatType chatType)
    {
        //当前类型
        type = chatType;
        Debug.Log(type);


        for (int i = 0; i < chatItems.Count; i++)
        {
            if (i < chatDic[type].Count)
            {
                chatItems[i].Init(chatDic[type][i]);
            }
            else
            {
                chatItems[i] = Instantiate(chatItem);
                chatItems[i].Init(chatDic[type][i]);
            }
        }

    }

    /// <summary>
    /// 发送给服务器
    /// </summary>
    public void Send()
    {
        if (inputField.text != "")
        {
            C_2_S_Chat_Msg ChatMsg = new C_2_S_Chat_Msg();
            ChatMsg.Type = type;
            ChatMsg.Info.Speak = "";
            ChatMsg.Info.Speakdesc = inputField.text;
            NetManager.Instance.SendMessage_To_Server(NetMsg_ID.C_2_S_Caht_Msg, ChatMsg.ToByteArray());
        }
    }
}


