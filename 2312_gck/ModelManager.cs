using System;
using System.Collections.Generic;

namespace MVC.Model
{
    public class ModelManager:Singleton<ModelManager>
    {
        Dictionary<Type,ModelBase> dic=new Dictionary<Type,ModelBase>();

        void LoadOneModel(ModelBase model)
        {
            if (!dic.ContainsKey(model.GetType()))
            {
                dic.Add(model.GetType(), model);
                model.Init();
            }
        }

        public T GetModel<T>() where T : ModelBase
        {
            return dic[typeof(T)] as T;
        }

        public void LoadALlModel()
        {
            
        }
    }
    
}