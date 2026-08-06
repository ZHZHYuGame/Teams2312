using System;
using System.Collections.Generic;

namespace PMVC
{
    /// <summary>
    /// MVC 模型基类 - 负责数据存储和业务逻辑
    /// 纯C#类，不依赖Unity生命周期
    /// 通过事件机制通知View数据变更
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// 数据变更事件 - 当Model中任意数据发生变化时触发
        /// </summary>
        public event Action OnDataChanged;

        /// <summary>
        /// 数据字典 - 存储所有业务数据
        /// </summary>
        protected readonly Dictionary<string, object> DataDict = new Dictionary<string, object>();

        /// <summary>
        /// 初始化数据
        /// </summary>
        public virtual void InitData()
        {
            
        }
        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名</param>
        /// <returns>数据值</returns>
        public virtual T GetData<T>(string key)
        {
            if (DataDict.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            return default;
        }

        /// <summary>
        /// 设置数据并触发变更事件
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名</param>
        /// <param name="value">数据值</param>
        public virtual void SetData<T>(string key, T value)
        {
            DataDict[key] = value;
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// 移除指定数据
        /// </summary>
        /// <param name="key">数据键名</param>
        public virtual void RemoveData(string key)
        {
            if (DataDict.ContainsKey(key))
            {
                DataDict.Remove(key);
                OnDataChanged?.Invoke();
            }
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public virtual void ClearData()
        {
            DataDict.Clear();
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// 检查是否包含指定键
        /// </summary>
        /// <param name="key">数据键名</param>
        /// <returns>是否存在</returns>
        public virtual bool HasData(string key)
        {
            return DataDict.ContainsKey(key);
        }

        /// <summary>
        /// 触发数据变更事件（用于批量更新后手动通知）
        /// </summary>
        public void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }
    }
}
