using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using MyGame;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManger : MonoBehaviour
{
    public static GameUIManger Instance;
    [SerializeField] public GameObject Bag, Shop;
    [SerializeField] private Button btn_Shop, btn_Bag;
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        btn_Bag.onClick.AddListener((() =>
        {
            Bag.gameObject.SetActive(true);
            C_To_S_Get_BagData_Msg msg = new C_To_S_Get_BagData_Msg();
            NetManager.GetInstance().SendMessage_To_Server(NewID.C_To_S_Get_BagData_Msg,msg.ToByteArray());

        }));
        btn_Shop.onClick.AddListener((() =>
        {
            Shop.gameObject.SetActive(true);
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
