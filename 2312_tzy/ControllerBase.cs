using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// MVC控制器基类 - 处理消息、协调Model和View
/// </summary>
public class ControllerBase 
{
    //关联的Model和View
    protected ModelBase _model;
    protected ViewBase _view;

    //消息处理器映射表
    //Key：消息ID，value：处理函数
    private Dictionary<int, Action<object>> _handlers = new Dictionary<int, Action<object>>();

    /// <summary>
    /// 绑定Model和View
    /// </summary>
    /// <param name="model"></param>
    /// <param name="view"></param>
    public virtual void Bind(ModelBase model,ViewBase view)
    {
        _model = model;
        _view = view;
    }

    /// <summary>
    /// 注册消息处理器
    /// </summary>
    /// <param name="messageId">消息ID</param>
    /// <param name="handler">处理函数</param>
    public void RegisterHandler(int messageId,Action<object> handler)
    {
        if (!_handlers.ContainsKey(messageId))
        {
            _handlers[messageId] = handler;
        }
    }

    /// <summary>
    /// 处理消息
    /// </summary>
    /// 是否成功处理
    /// <param name="messageId"></param>
    /// <param name="data"></param>
    public virtual bool HandleMessage(int messageId,object data)
    {
        if (_handlers.ContainsKey(messageId))
        {
            _handlers[messageId]?.Invoke(data);
            return true;
        }
        Console.WriteLine($"[waring]没有找到消息{messageId} 的处理器");
        return false;
    }

    //便捷方法：从Model获取数据
    public object GetModelData(string key) => _model?.Get(key);

    //便捷方法：设置Model数据
    public void SetModelData(string key, object value) => _model?.Set(key, value);

    //便捷方法：刷新View
    public void RefreshView() => _view?.Refresh();
}
