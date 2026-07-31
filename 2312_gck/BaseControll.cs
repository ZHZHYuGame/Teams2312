using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Âß¼­²ã»ùÀà
/// </summary>
public class BaseControll 
{
    public BaseControll()
    {
        OnRegister();
    }
    public virtual void OnRegister()
    {
        this.InitData();
        this.BindUIData();
        this.BindUIEvent();
    }
    protected virtual void InitData()
    {

    }

    protected virtual void BindUIData()
    {

    }

    protected virtual void BindUIEvent()
    {

    }
}
