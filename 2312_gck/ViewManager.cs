using System;
using System.Collections.Generic;

namespace MVC.View
{
    public class ViewManager:Singleton<ViewManager>
    {
        private Dictionary<Type,UIBase> views = new Dictionary<Type, UIBase>();

        public T GetView<T>() where T : UIBase
        {
            Type type = typeof(T);
            if (views.ContainsKey(type))
            {
                return views[type] as T;
            }
            return null;
        }

        public void RegisterView<T>(T view) where T : UIBase
        {
            Type type = typeof(T);
            if (!views.ContainsKey(type))
            {
                views.Add(type, view);
            }
        }

        public void ClearAllViews()
        {
            views.Clear();
        }
    }
}