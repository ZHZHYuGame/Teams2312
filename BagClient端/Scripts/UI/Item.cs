using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Google.Protobuf;
using MyGame;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    [SerializeField] private Image icon;

    [SerializeField] private Text Saletext;
    private ItemData myItemData;

    public void Init(ItemData itemData,int type)//0背包//1商城
    {
        Saletext.gameObject.SetActive(true);
        if (itemData==null||itemData.name==null)
        {
            icon.sprite=Resources.Load<Sprite>("Inventorys/bg_道具");
            Saletext.gameObject.SetActive(false);
        }
        else
        {
            if (type==0)
            {
                Saletext.gameObject.SetActive(false);
                
            }
            else if(type==1)
            {
                GetComponent<Button>().onClick.AddListener((() =>
                {
                    C_To_S_Buy_ShopData msg = new C_To_S_Buy_ShopData();
                    msg.ItemID = itemData.id;
                    NetManager.GetInstance().SendMessage_To_Server(NewID.C_To_S_Buy_ShopData_Msg,msg.ToByteArray());
                }));
            }
            myItemData = itemData;
            icon.sprite=Resources.Load<Sprite>(myItemData.icon);
            Saletext.text = myItemData.sale;

        }
     
    }
}
