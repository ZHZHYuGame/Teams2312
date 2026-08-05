using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetSceneCommand
{
   /// <summary>
   /// 记录所有的MVC的Model
   /// </summary>
    void Start()
    {
        GameFacadeMgr.Instance.RegisterModel(new MainModel());
        
    }

    
}
