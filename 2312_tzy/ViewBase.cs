using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// MVC视图基类 - 负责显示数据
/// 监听Model的数据变化，自动更新显示
/// </summary>
public class ViewBase 
{
    //关联的Model
    protected ModelBase _model;

    /// <summary>
    /// 关联Model，自动监听数据变化
    /// </summary>
    /// <param name="model"></param>
    public virtual void SetModel(ModelBase model)
    {
        _model = model;
        //订阅Model的数据变化事件
        if(model!=null)
        {
            model.OnDataChanged += OnDataChanged;
        }
    }

    /// <summary>
    /// 数据变化时的回调（子类重写）
    /// </summary>
    /// <param name="key"></param>
    /// <param name="newValue"></param>
    private void OnDataChanged(string key, object newValue)
    {
        //默认空实现，子类重写
    }

    /// <summary>
    /// 渲染/显示数据（子类重写）
    /// </summary>
    public virtual void Render()
    {
        if (_model!=null)
        {
            var data = _model.GetAll();
            foreach (var item in data)
            {
                Console.WriteLine($"{item.Key}={item.Value}");
            }
        }
    }

    /// <summary>
    /// 刷新显示
    /// </summary>
    public virtual void Refresh()
    {
        Render();
    }
}
