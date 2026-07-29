using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Kuanjia;
using MyGame;
using UnityEngine;
using UnityEngine.UI;

public class BagPanel : MonoBehaviour
{
    [SerializeField] Transform content;
   

    [SerializeField] private Button btn_Exit;
    // Start is called before the first frame update
    void Start()
    {
        btn_Exit.onClick.AddListener((() =>
        {
            gameObject.SetActive(false);
        }));
        MessageControll.GetInstance().AddListener(NewID.S_To_C_Get_BagData_Msg,GetBagDatasHandle);
    }

    private void GetBagDatasHandle(object obj)
    {
        object[] objList=obj as object[];
        byte[] byteData=objList[0] as byte[];
        S_To_C_Get_BagData_Msg msg=S_To_C_Get_BagData_Msg.Parser.ParseFrom(byteData);
        for (int i = 0; i < content.childCount; i++)
        {
            Destroy(content.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < msg.BagCount; i++)
        {
            if (i<msg.Itemid.Count)
            {
                Item item = Instantiate(Resources.Load<Item>("UI/Item"), content);
                var Data = ConfignManger.GetInstance().CreateData(msg.Itemid[i]);
                item.Init(Data,0);
                
            }
            else
            {
                Item item = Instantiate(Resources.Load<Item>("UI/Item"), content);
                item.Init(null,0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
