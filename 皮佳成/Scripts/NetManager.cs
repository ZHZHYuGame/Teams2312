using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Text;
using UnityEditor.PackageManager;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
public class NetManager : Singleton<NetManager>
{
    Socket socket = null;

    private MyMemoryStream myStream = new MyMemoryStream();

    //网络流式数据的数据结构 ,进行接受数据(发送数据?) 的处理
    //数据长度1024
    //接受服务器的数据固定? 长度是否可能很短或者超出1024?
    byte[] byteDataArr = new byte[1024];   //bute 数组 进行数据的传输 和接受数据
    Queue<byte[]> byteQuque = new Queue<byte[]>();
    // Start is called before the first frame update
    public void Start()
    {
        //设定socket 参数 (ip地址.字节流式 双向传递 Tcp 状态同步传输方式)
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //客户端 连接服务器(IP地址 目标服务器监听端口号 连接结果处理回调函数 )
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
        //客户端 连接服务器数据 (接受数据的Byte数组, 起始位置 总长度)
        socket.BeginReceive(byteDataArr, 0, byteDataArr.Length, SocketFlags.None, Receive_Server_Data_handle, null);
    }
    /// <summary>
    /// 客户端 接接受服务器数据
    /// </summary>
    /// <param name="ar"></param>
    private void Receive_Server_Data_handle(IAsyncResult ar)
    {
        try
        {
            //接受服务器数据长度
            int data_Count = socket.EndReceive(ar);
            ////创建一个接收数据长度的字节流数组
            //byte[] s_Byte_Arr = new byte[data_Count]; //有隐患 的   还是个局部变量
            ////把byteDataArr里的服务器数据Copy到s_Byte_arr里
            //Buffer.BlockCopy(byteDataArr, 0, s_Byte_Arr, 0, data_Count);
            ////将由其他线程处理的服务器数据拿出存放到主线程的队列byteQueue中,然后在主线程update里处理派发事件就能正常刷新ui数据,进行赋值
            //byteQuque.Enqueue(s_Byte_Arr);


            if (data_Count > 0)
            {
                //与客户端同步数据组成，数据拆分的结构、数据对应位置数据类型
                byte[] r_Bytes = new byte[data_Count];

                Buffer.BlockCopy(byteDataArr, 0, r_Bytes, 0, data_Count);
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

                        //消息中心    其他线程 不能直接操作  unity 里面的 东西  因为  unity 是 单线程  
                        //MessageManager.Instance.BroadCast(netID, descByte);

                        byteQuque.Enqueue(tampData); // 要存在主线程队列里面   通过 主线程掉用  

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
                socket.BeginReceive(byteDataArr, 0, byteDataArr.Length, SocketFlags.None, Receive_Server_Data_handle, null);
            }
        }
        catch (Exception)
        {


        }
    }

    /// <summary>
    /// 客户端发送信息到服务器
    /// </summary>
    public void SendMessage_To_Server(int netID, byte[] buteData)
    {
        //byte[] send_Data_Arr = Encoding.UTF8.GetBytes("ddd");
        //socket.BeginSend(send_Data_Arr, 0, send_Data_Arr.Length, SocketFlags.None, SendMessage_To_Handle, null);

        //Encoding.UTF8.GetBytes();
        //将网络ID号转换成字节流数组
        byte[] id_Data = BitConverter.GetBytes(netID);
        // 包头 包体总长度  id 和 数据的总长度
        byte[] headLength = BitConverter.GetBytes((ushort)(id_Data.Length + buteData.Length));

        //发送到服务器的字节流数组(消息号与数据的总长度)
        byte[] send_Data = new byte[id_Data.Length + buteData.Length + headLength.Length];

        //将头文件
        Buffer.BlockCopy(headLength, 0, send_Data, 0, headLength.Length);

        //将ID号的字节信息赋值到发送的字节流数组
        Buffer.BlockCopy(id_Data, 0, send_Data, headLength.Length, id_Data.Length);

        //将功能消息的字节信息复制到发送的字节流数组(在上面ID的后面)
        Buffer.BlockCopy(buteData, 0, send_Data, headLength.Length + id_Data.Length, buteData.Length);

        socket.BeginSend(send_Data, 0, send_Data.Length, SocketFlags.None, SendMessage_To_Handle, null);
    }

    private void SendMessage_To_Handle(IAsyncResult ar)
    {
        try
        {
            socket.EndSend(ar);
        }
        catch (Exception)
        {

        }
    }

    public void Update()
    {
        while (byteQuque.Count > 0)
        {
            byte[] byteArr = byteQuque.Dequeue();

            int netID = BitConverter.ToInt32(byteArr, 0);

            //MessageManager.Instance.BroadCast(MesKey.str,Encoding.UTF8.GetString(byteArr));
            byte[] netDesc = new byte[byteArr.Length - 4];
            Buffer.BlockCopy(byteArr, 4, netDesc, 0, byteArr.Length - 4);

            MessageManager.Instance.BroadCast(netID, netDesc);
        }
    }
}
