using System.Collections.Generic;
using Google.Protobuf;
using MyGame;

public class BagManager: Singleton<AccountManager>
{
    List<GoodsInfos> bagList = new List<GoodsInfos>();
    public void Start()
    {
        MessageControll.GetInstance().AddListener(NetMsg_Id.C_2_S_Buy_Msg, Buy_Handle);
        MessageControll.GetInstance().AddListener(NetMsg_Id.C_2_S_Delet_Msg, Delete_Handle);
    }

    private void Delete_Handle(object obj)
    {
        object[] list = obj as object[];
        byte[] data = (byte[])list?[0];
        Client client = (Client)list?[1];
        
        C_2_S_Buy_Msg c_msg = C_2_S_Buy_Msg.Parser.ParseFrom(data);
        var item = bagList.Find(x => x.Id == c_msg.Infos.Id);
        if (item != null)
        {
            
        }
    }

    private void Buy_Handle(object obj)
    {
        object[] list = obj as object[];
        byte[] data = (byte[])list?[0];
        Client client = (Client)list?[1];

        C_2_S_Buy_Msg c_msg = C_2_S_Buy_Msg.Parser.ParseFrom(data);
        GoodsInfos item = new GoodsInfos()
        {
            Id = c_msg.Infos.Id,
            Name = c_msg.Infos.Name,
            Icon = c_msg.Infos.Icon,
            Sale = c_msg.Infos.Sale,
            Type = c_msg.Infos.Type
        };
        for (int i = 0; i < bagList.Count; i++)
        {
            if (bagList[i].Id == c_msg.Infos.Id)
            {
                
            }
        }
        
        // S_2_C_Buy_Msg s_msg = new S_2_C_Buy_Msg();
        // s_msg.Infos = c_msg.Infos;
        // NetManager.GetInstance().SendNetMessage(client?.st, NetMsg_Id.S_2_C_Buy_Msg, s_msg.ToByteArray());
        
    }

    private void RefreshBagXml(object obj)
    {
        
    }
}