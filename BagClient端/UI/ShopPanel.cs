using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Kuanjia;
using MyGame;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{//如果做账号限购的话 , 或者VIP优惠,那数据只能从服务器来
    //但现在不做 所以直接本地通过配置表实例数据
    [SerializeField] Transform content;
    List<ItemData> itemDatas = new List<ItemData>();
    [SerializeField] private Text Moneytext;
    [SerializeField] private Button btn_Exit;

    [SerializeField] private Text tips;
    // Start is called before the first frame update
    void Start()
    {
        itemDatas = JsonConvert.DeserializeObject<List<ItemData>>(Resources.Load<TextAsset>("Jsons/Inventory").text);
        for (int i = 0; i < itemDatas.Count; i++)
        {
            Item item=GameObject.Instantiate(Resources.Load<Item>("UI/Item"), content);
            item.Init(itemDatas[i],1);
        }

        Moneytext.text = "金币:" + ConfignManger.GetInstance().Money;
        btn_Exit.onClick.AddListener((() =>
        {
            gameObject.SetActive(false);
        }));
        MessageControll.GetInstance().AddListener(NewID.S_To_C_Buy_ShopData_Msg,BuyShopDataHandle);
    }

    private void BuyShopDataHandle(object obj)
    {
        object[] objList=obj as object[];
        byte[] byteData=objList[0] as byte[];
        S_To_C_Buy_ShopData msg = S_To_C_Buy_ShopData.Parser.ParseFrom(byteData);
        switch (msg.Result)
        {
            case BuyShopDataResult.Nocoin:
                tips.text = "钱不够";
                break;
            case BuyShopDataResult.Nogoods:
                tips.text = "没有找到该商品";
                break;
            case BuyShopDataResult.BuySucc:
                tips.text = $"购买{ConfignManger.GetInstance().CreateData(msg.ItemID).name}";
                ConfignManger.GetInstance().Money = msg.Money;
                Moneytext.text = "金币:" + ConfignManger.GetInstance().Money;
                break;
            case BuyShopDataResult.Nocapacity:
                tips.text = "背包没有容量";
                break;
           
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
