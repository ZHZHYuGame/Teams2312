namespace MVC.Controller
{
    public class ControllerBase
    {
        protected ModelBase model;
        protected UIBase view;

        public virtual void Init(ModelBase model, UIBase view)
        {
            this.model = model;
            this.view = view;
            if (view!=null)
            {
                view.Init();
                RegisterUiEvents();
            }

            if (model!=null)
            {
                model.Init();
                RegisterModelEvents(); 
            }
            
        }

        protected virtual void RegisterModelEvents()
        {
          
        }

         protected virtual void RegisterUiEvents()
        {
           
        }
        public virtual void ShowView()
        {
            view?.Show();
        }

        public virtual void HideView()
        {
            view?.Hide();
        }

        public virtual void Dispose()
        {
            // 清理资源，解绑事件
        }
    }
}