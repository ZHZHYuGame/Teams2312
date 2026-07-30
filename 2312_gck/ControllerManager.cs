using System;
using System.Collections.Generic;
using UnityEngine;
namespace MVC.Controller
{
    public class ControllerManager:Singleton<ControllerManager>
    {
        private Dictionary<Type, ControllerBase> controllers = new Dictionary<Type, ControllerBase>() ;
        // 注册Controller
        public void RegisterController<T>(ControllerBase controller) where T : ControllerBase
        {
            Type type = typeof(T);
            if (!controllers.ContainsKey(type))
            {
                controllers.Add(type, controller);
                Debug.Log($"Controller {type.Name} registered successfully.");
            }
            else
            {
                Debug.LogWarning($"Controller {type.Name} already exists.");
            }
        }

        // 获取Controller
        public T GetController<T>() where T : ControllerBase
        {
            Type type = typeof(T);
            if (controllers.ContainsKey(type))
            {
                return controllers[type] as T;
            }
            else
            {
                Debug.LogError($"Controller {type.Name} not found.");
                return null;
            }
        }

        // 创建并注册Controller（自动绑定对应的Model和View）
        public T CreateController<T>(ModelBase model, UIBase view) where T : ControllerBase, new()
        {
            T controller = new T();
            controller.Init(model, view);
            RegisterController<T>(controller);
            return controller;
        }

        // 移除Controller
        public void RemoveController<T>() where T : ControllerBase
        {
            Type type = typeof(T);
            if (controllers.ContainsKey(type))
            {
                controllers[type].Dispose();
                controllers.Remove(type);
                Debug.Log($"Controller {type.Name} removed.");
            }
        }

        // 清空所有Controller
        public void ClearAllControllers()
        {
            foreach (var kvp in controllers)
            {
                kvp.Value.Dispose();
            }
            controllers.Clear();
            Debug.Log("All controllers cleared.");
        }
    }
    
}