using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFacadeMgr:Singleton<GameFacadeMgr>
{
  /// <summary>
  ///   Model数据层字典
  /// </summary>
  Dictionary<string,IModel> modelDict=new Dictionary<string, IModel>();
  /// <summary>
  /// controllerl逻辑层字典
  /// </summary>
  Dictionary<string,Icontroller> controllerDict=new Dictionary<string, Icontroller>();
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
          modelDict.Add(model.modelName,model);
      }
  }

  /// <summary>
  /// 获取Model
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
  /// 注册功能逻辑层
  /// </summary>
  /// <param name="controller"></param>
  public void RegisterController(Icontroller controller)
  {
      if (!controllerDict.ContainsKey(controller.name))
      {
          controllerDict.Add(controller.name,controller);
      }
  }

  /// <summary>
  /// 获取功能逻辑层
  /// </summary>
  /// <param name="controllerName"></param>
  /// <returns></returns>
  public Icontroller GetController(string controllerName)
  {
      if (controllerDict.ContainsKey(controllerName))
      {
          return controllerDict[controllerName];
      }
      return null;
  }
}
