using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleItem : MonoBehaviour
{
    Job mydata;
    [SerializeField] Image icon;
    [SerializeField] Text mz;
    public void Init(Job data)
    {
        mydata = data;
        gameObject.SetActive(data != null);
        if (data != null)
        {
            mz.text = data.name;
            icon.sprite = Resources.Load<Sprite>(data.icon);
        }
    }

    /// <summary>
    /// 点击角色
    /// </summary>
    public void Role()
    {
        CreateRole.ins.Refresh(mydata);
    }
}
