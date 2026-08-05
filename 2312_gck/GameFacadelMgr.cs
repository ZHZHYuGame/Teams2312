using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFacadelMgr : Singleton<GameFacadelMgr>
{

    /// <summary>
    /// Model数据层管理器
    /// </summary>
    Dictionary<string,IModel> modelDict=new Dictionary<string,IModel>();
    /// <summary>
    /// Controll
    /// </summary>
    Dictionary<string, IControll> controllDict = new();

   public void Start()
    {

    }
    /// <summary>
    /// 注册Model
    /// </summary>
    /// <param name="model"></param>
    public void RegisterModel(IModel model)
    {
        if (!modelDict.ContainsKey(model.modelName))
        {
            modelDict.Add(model.modelName, model);
        }
    }
    /// <summary>
    /// 获取model
    /// </summary>
    /// <param name="modelName"></param>
    /// <returns></returns>
    public IModel GetModel(string modelName)
    {
        if (modelDict.ContainsKey(modelName))
        {
            return modelDict[modelName];
        }
        return null;
    }
    /// <summary>
    /// 注册公共逻辑层
    /// </summary>
    /// <param name="controll"></param>
    public void RegisterControll(IControll controll)
    {

    }
    public IControll GetControll(string controllName)
    {
        return null;
    }
}
