using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制器管理器
/// </summary>
public class ControllerManager : Singleton<ControllerManager>
{
    Dictionary<string, ControllerBase> controllerDic = new Dictionary<string, ControllerBase>();

    // 注册Controller
    public void AddController(string controllerName, ControllerBase controller)
    {
        if (!controllerDic.ContainsKey(controllerName))
        {
            controllerDic.Add(controllerName, controller);
            controller.Init();
        }
    }

    // 获取Controller
    public T GetController<T>(string controllerName) where T : ControllerBase
    {
        if (controllerDic.ContainsKey(controllerName))
        {
            return controllerDic[controllerName] as T;
        }
        return null;
    }
}
