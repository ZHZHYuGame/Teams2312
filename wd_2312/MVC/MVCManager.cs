using System;
using System.Collections.Generic;
using MVC;
using PMVC;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// MVC 管理器 - 单例模式，负责注册和管理所有MVC组件
/// 提供便捷的初始化和获取组件的方法
/// </summary>
public class MVCManager : Singleton<MVCManager>
{

    /// <summary>
    /// 存储所有注册的Model实例
    /// </summary>
    private readonly Dictionary<string, BaseModel> _models = new Dictionary<string, BaseModel>();

    /// <summary>
    /// 存储所有注册的View实例
    /// </summary>
    private readonly Dictionary<string, BaseView> _views = new Dictionary<string, BaseView>();

    /// <summary>
    /// 存储所有注册的Controller实例
    /// </summary>
    private readonly Dictionary<string, BaseController> _controllers = new Dictionary<string, BaseController>();

    /// <summary>
    /// 初始化所有Model数据，为游戏开始做准备
    /// </summary>
    public void LoadAllModel()
    {
        foreach (var model in _models.Values)
        {
            model.InitData();
        }
    }
    #region ModelManager
    /// <summary>
    /// 注册Model
    /// </summary>
    /// <typeparam name="T">Model类型</typeparam>
    /// <param name="model">Model实例</param>
    /// <param name="key">可选的键名，默认为类型名</param>
    public T RegisterModel<T>(T model) where T : BaseModel
    {
        string modelKey = typeof(T).Name;
        _models.TryAdd(modelKey, model);
        return model;
    }
    
    /// <summary>
    /// 获取Model
    /// </summary>
    /// <typeparam name="T">Model类型</typeparam>
    /// <param name="key">可选的键名</param>
    /// <returns>Model实例</returns>
    public T GetModel<T>(string key = null) where T : BaseModel
    {
        string modelKey = key ?? typeof(T).Name;
        if (_models.TryGetValue(modelKey, out var model))
        {
            return (T)model;
        }
        return null;
    }
    
    /// <summary>
    /// 取消注册指定的Model
    /// </summary>
    public void UnregisterModel<T>(string key = null) where T : BaseModel
    {
        string modelKey = key ?? typeof(T).Name;
        _models.Remove(modelKey);
    }
    #endregion

    #region ControllerManager
    /// <summary>
    /// 注册Controller
    /// </summary>
    /// <typeparam name="T">Controller类型</typeparam>
    /// <param name="controller">Controller实例</param>
    /// <param name="key">可选的键名，默认为类型名</param>
    public T RegisterController<T>(T controller, string key = null) where T : BaseController
    {
        string controllerKey = key ?? typeof(T).Name;
        _controllers[controllerKey] = controller;
        return controller;
    }
    
    /// <summary>
    /// 获取Controller
    /// </summary>
    /// <typeparam name="T">Controller类型</typeparam>
    /// <param name="key">可选的键名</param>
    /// <returns>Controller实例</returns>
    public T GetController<T>(string key = null) where T : BaseController
    {
        string controllerKey = key ?? typeof(T).Name;
        if (_controllers.TryGetValue(controllerKey, out var controller))
        {
            return (T)controller;
        }
        return null;
    }
    
    /// <summary>
    /// 取消注册指定的Controller
    /// </summary>
    public void UnregisterController<T>(string key = null) where T : BaseController
    {
        string controllerKey = key ?? typeof(T).Name;
        if (_controllers.TryGetValue(controllerKey, out var controller))
        {
            controller.Dispose();
        }
        _controllers.Remove(controllerKey);
    }
    #endregion
    
    #region ViewManager
    /// <summary>
    /// 注册View
    /// </summary>
    /// <typeparam name="T">View类型</typeparam>
    /// <param name="view">View实例</param>
    /// <param name="key">可选的键名，默认为类型名</param>
    public T RegisterView<T>(T view, string key = null) where T : BaseView
    {
        string viewKey = key ?? typeof(T).Name;
        _views[viewKey] = view;
        return view;
    }
    
    /// <summary>
    /// 获取View
    /// </summary>
    /// <typeparam name="T">View类型</typeparam>
    /// <param name="key">可选的键名</param>
    /// <returns>View实例</returns>
    public T GetView<T>(string key = null) where T : BaseView
    {
        string viewKey = key ?? typeof(T).Name;
        if (_views.TryGetValue(viewKey, out var view))
        {
            return (T)view;
        }
        return null;
    }

    /// <summary>
    /// 取消注册指定的View
    /// </summary>
    public void UnregisterView<T>(string key = null) where T : BaseView
    {
        string viewKey = key ?? typeof(T).Name;
        if (_views.TryGetValue(viewKey, out var view))
        {
            view.Dispose();
        }
        _views.Remove(viewKey);
    }
    #endregion
    
    #region <T>Manager
    /// <summary>
    /// 初始化MVC三元组 - 一键注册并关联Model、View、Controller
    /// </summary>
    /// <typeparam name="TModel">Model类型</typeparam>
    /// <typeparam name="TView">View类型</typeparam>
    /// <typeparam name="TController">Controller类型</typeparam>
    /// <param name="model">Model实例</param>
    /// <param name="view">View实例</param>
    /// <param name="controller">Controller实例</param>
    public void Initialize_MVC<TModel, TView, TController>(
        TModel model, TView view, TController controller)
        where TModel : BaseModel
        where TView : BaseView
        where TController : BaseController
    {
        RegisterModel(model);
        RegisterView(view);
        RegisterController(controller);
        controller.Init(model, view);
    }
    /// <summary>
    /// 取消注册所有MVC组件
    /// </summary>
    public void UnregisterAll()
    {
        foreach (var controller in _controllers.Values)
        {
            controller.Dispose();
        }
        _controllers.Clear();
        _views.Clear();
        _models.Clear();
    }
    #endregion
    /// <summary>
    /// 销毁时清理所有资源
    /// </summary>
    private void OnDestroy()
    {
        UnregisterAll();
        if (instance == this)
        {
            instance = null;
        }
    }
}
