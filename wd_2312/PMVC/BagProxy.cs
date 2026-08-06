using System.Collections;
using System.Collections.Generic;
using Data;
using MVC.Bag;
using PureMVC.Patterns.Proxy;
using UnityEngine;
///背包物品数据
public class BagData
{
    public int Id;
    public string Icon;
    public string ItemName;
    public int Num;
}

/// <summary>
/// 背包数据代理
/// </summary>
public class BagProxy : Proxy
{
    public const string bagModelProxy = "msg_Add";
    public Dictionary<int, BagData> Items = new();

    public BagProxy(string proxyName, object data = null) : base(proxyName, data)
    {
        
    }

    /// <summary>
    /// 中介者——定义背包添加的逻辑
    /// </summary>
    /// <param name="goods"></param>
    public void AddToBag(GoodsData goods)
    {
        if (goods == null) return;
        if (!Items.TryGetValue(int.Parse(goods.id), out BagData bagData))
        {
            bagData = new BagData()
            {
                Id = int.Parse(goods.id),
                Icon = goods.icon,
                ItemName = goods.name,
                Num = 1
            };
            Items.Add(int.Parse(goods.id), bagData);
        }
        else
        {
            bagData.Num++;
        }
        //向 BagMediator 发通知 更新背包界面
        SendNotification(NotificationName.BAG_UPDATE, bagData);
    }
}