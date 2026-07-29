using UnityEngine;

namespace PMVC
{
    /// <summary>
    /// MVC 视图基类 - 负责UI显示
    /// 继承 MonoBehaviour，可直接挂载到GameObject上
    /// 监听Model数据变更事件并更新UI
    /// </summary>
    public class BaseView : MonoBehaviour
    {
        /// <summary>
        /// 关联的Model引用
        /// </summary>
        protected BaseModel Model;

        /// <summary>
        /// 初始化View并绑定Model
        /// </summary>
        /// <param name="model">要绑定的Model</param>
        public virtual void Init(BaseModel model)
        {
            Model = model;
            if (Model != null)
            {
                Model.OnDataChanged += OnDataChanged;
            }
            RefreshView();
        }

        /// <summary>
        /// Model数据变更时的回调 - 触发View刷新
        /// </summary>
        protected virtual void OnDataChanged()
        {
            RefreshView();
        }

        /// <summary>
        /// 刷新视图 - 子类应重写此方法来更新UI显示
        /// </summary>
        public virtual void RefreshView()
        {
            // 子类重写此方法以实现具体的UI刷新逻辑
        }

        /// <summary>
        /// 清理事件订阅
        /// </summary>
        public virtual void Dispose()
        {
            if (Model != null)
            {
                Model.OnDataChanged -= OnDataChanged;
            }
        }
    }
}
