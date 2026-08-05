using System;

namespace UI.Base
{
    /// <summary>
    /// 逻辑层基类
    /// </summary>
    public class BaseControll
    {
        public BaseControll()
        {
            OnRegister();
        }
        public virtual void OnRegister()
        {
           this.InitData();
           this.BindUIData();
           this.BindUIEvnet();
        }

        protected virtual void InitData()
        {
            throw new NotImplementedException();
        }

        protected virtual void BindUIData()
        {
            throw new NotImplementedException();
        }

        protected virtual void BindUIEvnet()
        {
            throw new NotImplementedException();
        }
    }
}