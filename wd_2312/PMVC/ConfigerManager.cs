using System.Collections.Generic;
using Data;
using Newtonsoft.Json;
using UnityEngine;

namespace MVC
{
    public class ConfigManager : Singleton<ConfigManager>
    {
        public List<GoodsData> goodsList = new List<GoodsData>();

        public void LoadAllJson()
        {
            goodsList = LoadOneJson<List<GoodsData>>("good");
        }
        public T LoadOneJson<T>(string jsonName)
        {
            //获取json数据
            //反序列化为C#对象
            return JsonConvert.DeserializeObject<T>(Resources.Load<TextAsset>(jsonName).text);
        }
    }
}