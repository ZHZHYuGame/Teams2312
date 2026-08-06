using System;
using System.Collections.Generic;
using Data;
using MVC.Bag;
using PMVC;
using UnityEngine;
using UnityEngine.UI;

public class BagView:MonoBehaviour
{
    public Transform root;
    public Button buyBtn;
    public Dictionary<int, BagItem> Bag = new Dictionary<int, BagItem>();

    /// <summary>
    /// 刷新面板
    /// </summary>
    /// <param name="goodsData"></param>
    public void RefreshPanel(BagData goodsData)
    {
        if (!Bag.TryGetValue(goodsData.Id, out BagItem item))
        {
            item = Instantiate(Resources.Load<BagItem>("BagItem"), root);
            Bag.Add(goodsData.Id, item);
            item?.RefreshItem(goodsData);
        }
        else
        {
            item?.RefreshItem(goodsData);
        }
    }
}