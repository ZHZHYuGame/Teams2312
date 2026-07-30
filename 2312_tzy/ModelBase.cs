using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// MVC模型基类 -负责管理数据和业务逻辑
/// 支持观察者模式，数据变化时自动通知监听者
/// </summary>
public class ModelBase 
{
    //储存数据
    private Dictionary<string, object> _data = new Dictionary<string, object>();

    //数据变化事件（C#用事件代替Lua的监听器）
    //定义委托：数据变化时的回到方法签名
    public delegate void DataChangeHandler(string key, object newValue);

    //事件：当任何数据变化时触发
    public event DataChangeHandler OnDataChanged;

    /// <summary>
    /// 设置数据
    /// </summary>
    /// <param name="key">数据名称</param>
    /// <param name="value">数据值</param>
    public void Set(string key,object value)
    {
        _data[key] = value;
        OnDataChanged?.Invoke(key, value);
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public object Get(string key)
    {
        return _data.ContainsKey(key) ? _data[key] : null;
    }

    /// <summary>
    /// 获取所有数据的副本
    /// </summary>
    /// <returns></returns>
    public Dictionary<string,object> GetAll()
    {
        return new Dictionary<string, object>(_data);
    }

    /// <summary>
    /// 删除数据
    /// </summary>
    /// <param name="key"></param>
    public void Remove(string key)
    {
        if (_data.ContainsKey(key))
        {
            _data.Remove(key);
            OnDataChanged?.Invoke(key, null);
        }
    }
}
