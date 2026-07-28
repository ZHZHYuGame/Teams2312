using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Collections;
using MyGame;
using Newtonsoft.Json;

public class NetManager : Singleton<NetManager>
{
    Socket st;

    byte[] receiveDataByte = new byte[1024];

    /// <summary>
    /// 用来存储数据(粘包)
    /// </summary>
    private MyMemoryStream myStream = new MyMemoryStream();

    public List<Client> clientsList = new List<Client>();

    /// <summary>
    /// 启动服务器
    /// </summary>
    public void Start()
    {
        //建立一个基于IPV4地址的TCP流式服务
        st = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //设定一个接收任何一个可以接受客户端访问，并设置端口号
        st.Bind(new IPEndPoint(IPAddress.Any, 10086));
        //设定服务器接受客户端访问数量
        st.Listen(5);

        Console.WriteLine("服务器启动成功");

        //接受客户端访问处理
        st.BeginAccept(AcceptHandle, null);
    }

    /// <summary>
    /// 接受客户端访问处理
    /// </summary>
    /// <param name="ar"></param>
    private void AcceptHandle(IAsyncResult ar)
    {
        //处理连接进来的客户端
        Socket clientSocket = st.EndAccept(ar);
        IPEndPoint ip = clientSocket.RemoteEndPoint as IPEndPoint;
        //记录客户端的信息
        Client c = new Client()
        {
            st = clientSocket,
            ip = ip.Address.ToString(),
            port = ip.Port
        };
        Console.WriteLine("计入");
        clientsList.Add(c);
        GetOnLineList(c);
        GetGoodsList(c);
        Console.WriteLine($"连接进来的客户端IP = {ip.Address}  端口号 = {ip.Port}");
        //接收客户端发送过来的行为数据
        c.st.BeginReceive(receiveDataByte, 0, receiveDataByte.Length, SocketFlags.None, clientReceiveHandle, c);

        //接受客户端访问处理--多线程的再次唤醒
        st.BeginAccept(AcceptHandle, null);
    }

    /// <summary>
    /// 获取聊天室在线用户列表
    /// </summary>
    /// <param name="client"></param>
    public void GetOnLineList(Client client)
    {
        
    }

    public void GetGoodsList(Client client)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "goods.json");
    
        // 1. 先检查文件在不在
        if (!File.Exists(path))
        {
            Console.WriteLine("错误：goods.json 未找到！");
            // 这里可以给 client 返回一个“商品列表为空”的错误消息
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            // 2. 反序列化成功后将信息发送给客户端
            var list = JsonConvert.DeserializeObject<RepeatedField<GoodsInfos>>(json);
            S_2_C_GoodsList_Msg s_msg = new S_2_C_GoodsList_Msg();
            s_msg.GoodsList = list;
            SendNetMessage(client.st, NetMsg_Id.S_2_C_JsonGoods_Msg, s_msg.ToByteArray());
            
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON解析失败：{ex.Message}");
            // 给客户端返回错误
        }
    }
    /// <summary>
    /// 接收客户端发送过来的行为数据
    /// 1.接收单位为字节流Byte
    /// 2.是客户端的行为触发以数据的形式发送
    /// </summary>
    /// <param name="ar"></param>
    private void clientReceiveHandle(IAsyncResult ar)
    {
        try
        {
            Client client = ar.AsyncState as Client;


            //安全处理解析客户端的数据
            int dataLen = client.st.EndReceive(ar);
            
            //接收客户端数据成功
            if (dataLen > 0)
            {
                //与客户端同步数据组成，数据拆分的结构、数据对应位置数据类型
                byte[] r_Bytes = new byte[dataLen];

                Buffer.BlockCopy(receiveDataByte, 0, r_Bytes, 0, dataLen);
                //如有剩余未处理的包，则在包的后面进入写入
                myStream.Position = myStream.Length;
                //数据已经存进来了
                myStream.Write(r_Bytes, 0, r_Bytes.Length);
                //判断是不是到少有一个不完整的包(为什么？因为还没到判断完整包的地方)
                while (myStream.Length >= 2)
                {
                    //现在位置在写入数据的长度的位置
                    myStream.Position = 0;
                    //包头的值 = 包体的长度
                    ushort titleLen = myStream.ReadUshort();
                    //包的整体长度
                    int allLen = titleLen + 2;
                    //这里才是判断是不是有一个可以处理的完整的包
                    if (myStream.Length >= allLen)
                    {
                        //这里已经开始读消息的内容(id + 内容)
                        byte[] tampData = new byte[titleLen];
                        myStream.Read(tampData, 0, tampData.Length);

                        int netID = BitConverter.ToInt32(tampData, 0);
                        //内容
                        byte[] descByte = new byte[tampData.Length - 4];
                        Buffer.BlockCopy(tampData, 4, descByte, 0, descByte.Length);
                        //MessageControll.GetInstance().Dispach(netID, descByte, client, 1, "", true);
                        if (netID == NetMsg_Id.C_2_S_Login_Msg)
                            MessageControll.GetInstance().Dispach(NetMsg_Id.C_2_S_Login_Msg, descByte, client);
                        if (netID == NetMsg_Id.C_2_S_Register_Msg)
                            MessageControll.GetInstance().Dispach(NetMsg_Id.C_2_S_Register_Msg, descByte, client);
                        if(netID == NetMsg_Id.C_2_S_Buy_Msg)
                            MessageControll.GetInstance().Dispach(NetMsg_Id.C_2_S_Buy_Msg, descByte, client);
                        int shLen = (int)myStream.Length - allLen;
                        //还有未处理完的数据包
                        if (shLen > 0)
                        {
                            //存剩余数据
                            byte[] shData = new byte[shLen];
                            myStream.Read(shData, 0, shData.Length);
                            //请空流
                            myStream.Position = 0;
                            myStream.SetLength(0);
                            //将剩余的数据写到缓冲区
                            myStream.Write(shData, 0, shData.Length);
                        }
                        else
                        {
                            //请空流
                            myStream.Position = 0;
                            myStream.SetLength(0);
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                client.st.BeginReceive(receiveDataByte, 0, receiveDataByte.Length, SocketFlags.None,
                    clientReceiveHandle, client);
            }
            else
            {
                //下线
                client.st.Shutdown(SocketShutdown.Both);
                client.st.Close();
                Console.WriteLine($"客户端ip = {client.ip}, 端口号 = {client.port}下线");
                for (int i = clientsList.Count - 1; i > 0; i--)
                {
                    if (clientsList[i] == client)
                    {
                        clientsList.Remove(clientsList[i]);
                        break;
                    }
                }
                S_2_C_OnLineList_Msg s_msg = new S_2_C_OnLineList_Msg();
                for (int i = 0; i < clientsList.Count; i++)
                {
                    MyGame.Client c = new MyGame.Client();
                    c.Ip = clientsList[i].ip;
                    c.Port = clientsList[i].port;
                    s_msg.CList.Add(c);
                }
                for (int i = 0; i < s_msg.CList.Count; i++)
                {
                    SendNetMessage(clientsList[i].st, NetMsg_Id.S_2_C_OnLineList, s_msg.ToByteArray());
                }
                
            }
        }
        catch (Exception)
        {
            //异常捕捉：解析数据异常报错监听
            Console.WriteLine("客户端数据解析异常");
        }
    }

    public void SendNetMessage(Socket c_Socket, byte[] contextData)
    {
        c_Socket.BeginSend(contextData, 0, contextData.Length, SocketFlags.None, Client_Send_To_Net_Handle, c_Socket);
    }

    /// <summary>
    /// 服务器向客户端发送数据(字节流)，PB
    /// </summary>
    /// <param name="id">游戏功能ID</param>
    /// <param name="contextData">游戏要发送的内容</param>
    public void SendNetMessage(Socket c_Socket, int id, byte[] contextData)
    {
        byte[] idData = BitConverter.GetBytes(id);
        byte[] data = new byte[idData.Length + contextData.Length];
        Buffer.BlockCopy(idData, 0, data, 0, idData.Length);
        Buffer.BlockCopy(contextData, 0, data, idData.Length, contextData.Length);

        byte[] makeDatas = MakeData(data);
        c_Socket.BeginSend(makeDatas, 0, makeDatas.Length, SocketFlags.None, Client_Send_To_Net_Handle, c_Socket);
    }

    private void Client_Send_To_Net_Handle(IAsyncResult ar)
    {
        try
        {
            Socket st = ar.AsyncState as Socket;
            st.EndSend(ar);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public Client GetClient(int port)
    {
        for (int i = 0; i < clientsList.Count; i++)
        {
            if (clientsList[i].port == port)
            {
                return clientsList[i];
            }
        }
        return null;
    }
    /// <summary>
    /// 流结构（1-消息体长度，2-消息主体）
    /// </summary>
    /// <param name="data"></param>
    public byte[] MakeData(byte[] data)
    {
        //一个对象的生命周期
        using (MyMemoryStream d = new MyMemoryStream())
        {
            d.WriteUShort((ushort)data.Length);
            d.Write(data, 0, data.Length);
            return d.ToArray();
        }
    }
}