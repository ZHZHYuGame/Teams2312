using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

/// <summary>
/// 物品数据结构
/// </summary>
public class InventoryData
{
    public string id;
    public string name;
    public string icon;
    public string inventoryType;
    public string equipType;
    public string sale;
    public string quality;
    public string damage;
    public string hp;
    public string power;
    public string Des;
}

/// <summary>
/// 配置管理器 - 负责加载和管理游戏配置数据
/// </summary>
public class ConfigManager : Singleton<ConfigManager>
{
    public List<InventoryData> Inventories { get; private set; }

    public ConfigManager()
    {
        Inventories = new List<InventoryData>();
    }

    /// <summary>
    /// 加载JSON配置表
    /// </summary>
    private List<T> LoadJsonTable<T>(string fileName)
    {
        TextAsset jsonAsset = ResourceManager.Ins.LoadRes<TextAsset>("Jsons", fileName);
        if (jsonAsset == null)
        {
            Debug.LogError(string.Format("找不到配置文件: {0}", fileName));
            return new List<T>();
        }

        return JsonConvert.DeserializeObject<List<T>>(jsonAsset.text);
    }

    /// <summary>
    /// 加载所有配置表
    /// </summary>
    public void LoadAllConfig()
    {
        Inventories = LoadJsonTable<InventoryData>("Inventory");
        Debug.Log(string.Format("加载物品配置数量: {0}", Inventories.Count));
    }

    /// <summary>
    /// 根据ID查找物品
    /// </summary>
    public InventoryData GetInventoryById(string id)
    {
        return Inventories.Find(x => x.id == id);
    }
}
