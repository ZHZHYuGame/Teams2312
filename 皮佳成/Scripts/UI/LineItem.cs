using Games;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LineItem : MonoBehaviour
{
    public Text text;
    public void Init(PlayerInfo playerInfo)
    {
        gameObject.SetActive(playerInfo != null);
        if (playerInfo != null)
        {
            text.text = playerInfo.Name;
        }
    }

    public void ChatPrivate()
    { 
    
    }
}
