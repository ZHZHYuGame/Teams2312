using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetSceneCommand
{
    /// <summary>
    /// 记录所有MVC的Model
    /// </summary>
    public void Start()
    {
        GameFacadelMgr.GetInstance().RegisterModel(new MainModel());
    }
}