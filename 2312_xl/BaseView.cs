using UnityEngine;

namespace MVC.Core
{
    /// <summary>
    /// MVC框架 View 基类，继承 MonoBehaviour 用于绑定 Unity UI 组件。
    /// 提供生命周期回调和事件分发器集成。
    /// </summary>
    public class BaseView : MonoBehaviour
    {
        protected EventDispatcher dispatcher = new EventDispatcher();

        /// <summary>暴露事件分发器，供 Controller 监听 View 事件。</summary>
        public EventDispatcher Dispatcher => dispatcher;

        /// <summary>View 初始化时调用（仅一次），用于获取组件引用和初始设置。</summary>
        public virtual void OnInit() { }

        /// <summary>每次显示 View 时调用。</summary>
        public virtual void OnShow() { }

        /// <summary>每次隐藏 View 时调用。</summary>
        public virtual void OnHide() { }

        /// <summary>View 销毁时调用，清理事件监听和资源。</summary>
        public virtual void OnClose()
        {
            dispatcher.ClearEvent();
        }

        protected virtual void Awake() { }
        protected virtual void Start() { }

        protected virtual void OnDestroy()
        {
            OnClose();
        }
    }
}