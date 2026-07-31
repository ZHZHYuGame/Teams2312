using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 模型管理器
/// </summary>
public class ModelManager : Singleton<ModelManager>
{
    Dictionary<string, ModelBase> modelDic = new Dictionary<string, ModelBase>();

    // 注册Model
    public void AddModel(string modelName, ModelBase model)
    {
        if (!modelDic.ContainsKey(modelName))
        {
            modelDic.Add(modelName, model);
            model.Init();
        }
    }

    // 获取Model
    public T GetModel<T>(string modelName) where T : ModelBase
    {
        if (modelDic.ContainsKey(modelName))
        {
            return modelDic[modelName] as T;
        }
        return null;
    }
}
