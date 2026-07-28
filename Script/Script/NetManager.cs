using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class NetManager
{

    Socket socket = null;
    //网络流式数据的数据结构，进行接收数据（发送数据？）的处理
    //数据长度1024
    //接收服务器的数据固定？长度是否可能很短或者超出1024?
    byte[] byteDataArr = new byte[1024];

    Queue<byte[]> byteQueue = new Queue<byte[]>();

    // Start is called before the first frame update
    public void Start()
    {
        //设定Socket参数（IP地址、字节流式双向传递、Tcp状态同步传输方式）
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //客户端连接服务器（IP地址、商品号、连接结果处理回调函数）
        socket.BeginConnect("127.0.0.1", 10086, Connect_Server_Handle, null);
    }
    /// <summary>
    /// 连接服务器结果处理回调函数
    /// </summary>
    /// <param name="ar"></param>
    private void Connect_Server_Handle(IAsyncResult ar)
    {
        socket.EndConnect(ar);
        //连接成功
        Debug.Log("连接服务器成功");
        //客户端 接收服务器数据 （接收数据的byte数组、起始位置、总长度） 
        socket.BeginReceive(byteDataArr, 0, byteDataArr.Length, SocketFlags.None, Receive_Server_Data_Handle, null);
    }
    /// <summary>
    /// 客户端 接收服务器数据
    /// </summary>
    /// <param name="ar"></param>
    private void Receive_Server_Data_Handle(IAsyncResult ar)
    {
        try
        {
            //接收服务器数据的长度
            int data_Count = socket.EndReceive(ar);
            //创建一个接收数据长度的字节流数组
            byte[] s_Byte_Arr = new byte[data_Count];
            //把byteDataArr里的服务器数据Copy到s_Byte_Arr里
            Buffer.BlockCopy(byteDataArr, 0, s_Byte_Arr, 0, data_Count);
            //将由其他线程处理的服务器数据拿出存放到主线程的队列byteQueue中，然后在主线程Update里处理派发事件就能正常刷新UI数据，进行赋值
            byteQueue.Enqueue(s_Byte_Arr);
            //MessageControll.GetInstance().Dispach(101, Encoding.UTF8.GetString(s_Byte_Arr));
            socket.BeginReceive(byteDataArr, 0, byteDataArr.Length, SocketFlags.None, Receive_Server_Data_Handle, null);
        }
        catch (Exception)
        {

        }
    }

    public void SendMessage_To_Server()
    {
        byte[] send_Data_Arr = Encoding.UTF8.GetBytes("ddd");
        socket.BeginSend(send_Data_Arr, 0, send_Data_Arr.Length, SocketFlags.None, Send_To_Server_Handle, null);
    }
    /// <summary>
    /// 客户端发送信息到服务器
    /// </summary>
    /// <param name="netID">网络消息唯一识别功能号</param>
    /// <param name="byteData">功能消息数据</param>
    public void SendMessage_To_Server(int netID, byte[] byteData)
    {
        //Encoding.UTF8.GetBytes();
        //将网络ID号转换成字节流数组
        byte[] id_Data = BitConverter.GetBytes(netID);
        //发送到服务器的字节流数组（消息号与数据的总长度）
        byte[] send_Data = new byte[id_Data.Length + byteData.Length];
        //将ID号的字节信息复制到发送的字节流数组
        Buffer.BlockCopy(id_Data, 0, send_Data, 0, id_Data.Length);
        //将功能消息的字节信息复制到发送的字节流数组（在上面ID号的后面）
        Buffer.BlockCopy(byteData, 0, send_Data, id_Data.Length, byteData.Length);

        socket.BeginSend(send_Data, 0, send_Data.Length, SocketFlags.None, Send_To_Server_Handle, socket);
    }

    private void Send_To_Server_Handle(IAsyncResult ar)
    {
        try
        {
            socket.EndSend(ar);
        }
        catch (Exception)
        {

        }

    }

    // Update is called once per frame
    public void Update()
    {
        while (byteQueue.Count > 0)
        {
            byte[] byteArr = byteQueue.Dequeue();

            //网络ID号
            int netID = BitConverter.ToInt32(byteArr, 0);
            //功能的数据部分
            byte[] netDesc = new byte[byteArr.Length - 4];
            Buffer.BlockCopy(byteArr, 4, netDesc, 0, byteArr.Length - 4);

            MessageControll.GetInstance().Dispach(netID, netDesc);
        }
    }
}
