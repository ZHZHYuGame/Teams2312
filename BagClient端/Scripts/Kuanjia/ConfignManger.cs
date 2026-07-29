using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;

namespace Kuanjia
{
    public class ConfignManger:Singleton<ConfignManger>
    {
        public List<ItemData> Confign = new List<ItemData>();
        public List<ItemData> bagDatas = new List<ItemData>();
        public float Money;
        public void InitConfign()
        {
            Confign = JsonConvert.DeserializeObject<List<ItemData>>(Resources.Load<TextAsset>("Jsons/Inventory").text);
        }

        public ItemData CreateData(int id)
        {
            ItemData itemData = null;
            for (int i = 0; i < Confign.Count; i++)
            {
                if (Confign[i].id==id)
                {

                    itemData = new ItemData(id, Confign[i].name, Confign[i].icon, Confign[i].inventoryType,
                        Confign[i].equipType,
                        Confign[i].sale, Confign[i].starLeve, Confign[i].quality, Confign[i].damage, Confign[i].hp,
                        Confign[i].power,
                        Confign[i].Des, 1);
                        bagDatas.Add(itemData);
                        break;
                }
               
            }

            return itemData;
        }
        
    }
}