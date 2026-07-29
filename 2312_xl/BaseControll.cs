using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseControll
{
    public BaseControll() { OnRegister(); }
    public virtual void OnRegister()
    {
        this.InitData();
        this.BindUIData();
        this.BindUIEvent();
    }

    protected virtual void InitData()
    {
        throw new NotImplementedException();
    }
    protected virtual void BindUIData()
    {
        throw new NotImplementedException();
    }
    protected virtual void BindUIEvent()
    {
        throw new NotImplementedException();
    }
}
