namespace PMVC
{
    /// <summary>
    /// MVC 控制器基类 - 负责处理用户输入和业务逻辑
    /// 纯C#类，协调Model和View：绑定UI事件，处理用户操作，更新Model数据
    /// </summary>
    public class BaseController
    {
        /// <summary>
        /// 关联的Model引用
        /// </summary>
        protected BaseModel Model;

        /// <summary>
        /// 关联的View引用
        /// </summary>
        protected BaseView View;

        /// <summary>
        /// 初始化Controller，绑定Model和View
        /// </summary>
        /// <param name="model">要绑定的Model</param>
        /// <param name="view">要绑定的View</param>
        public virtual void Init(BaseModel model, BaseView view)
        {
            Model = model;
            View = view;

            if (View != null)
            {
                View.Init(Model);
                BindUIEvents();
            }
        }

        /// <summary>
        /// 绑定UI事件 - 子类应重写此方法来绑定具体的UI事件
        /// </summary>
        protected virtual void BindUIEvents()
        {
            // 子类重写此方法以绑定View中的UI元素事件
        }

        /// <summary>
        /// 清理Controller
        /// </summary>
        public virtual void Dispose()
        {
            UnbindUIEvents();
            if (View != null)
            {
                View.Dispose();
            }
            Model = null;
            View = null;
        }

        /// <summary>
        /// 解绑UI事件 - 子类应重写此方法来解绑具体的UI事件
        /// </summary>
        protected virtual void UnbindUIEvents()
        {
            // 子类重写此方法以解绑View中的UI元素事件
        }

        /// <summary>
        /// 辅助方法：更新Model数据并自动触发通知
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名</param>
        /// <param name="value">数据值</param>
        protected void UpdateModelData<T>(string key, T value)
        {
            if (Model != null)
            {
                Model.SetData(key, value);
            }
        }

        /// <summary>
        /// 辅助方法：从Model获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名</param>
        /// <returns>数据值</returns>
        protected T GetModelData<T>(string key)
        {
            if (Model != null)
            {
                return Model.GetData<T>(key);
            }
            return default;
        }
    }
}
