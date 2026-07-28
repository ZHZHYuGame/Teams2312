using System; 
using System.Collections; 
using System.Collections.Generic; 
using System.Net.Sockets; 
using System.Text;
using DefaultNamespace;
using UnityEngine; 

/// <summary>
/// 网络管理器 - 负责与服务器建立TCP连接并进行异步通信
/// </summary>
public class NetManager :Singleton<NetManager>
{
    public int RoleGuid=0;
    // Socket对象，用于TCP网络通信
    Socket socket = null; 
       /// <summary>
    /// 用来存储数据(粘包)
    /// </summary>
    private MyMemoryStream myStream = new MyMemoryStream();

    public List<RoleData> roles = new List<RoleData>();
    // 接收数据的缓冲区，最大1024字节
    byte[] byteDataArr = new byte[1024]; 

    // 接收数据的队列，用于存储已接收但未处理的数据
    Queue<byte[]> byteQueue = new Queue<byte[]>(); 

    /// <summary>
    /// 开始连接服务器（类似于Unity的Start方法）
    /// </summary>
    public void Start() 
    { 
        // 创建TCP Socket对象：IPv4协议、流式传输、TCP协议
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); 
        
        // 异步连接到服务器：地址127.0.0.1，端口10086，连接完成后回调Connect_Server_Handle
        socket.BeginConnect("127.0.0.1", 10086, Connect_Server_Handle, null); 
    } 

    /// <summary>
    /// 连接服务器完成的回调方法
    /// </summary>
    /// <param name="ar">异步操作结果对象</param>
    private void Connect_Server_Handle(IAsyncResult ar) 
    { 
        // 结束连接操作
        socket.EndConnect(ar); 
        
        // 连接成功后，开始异步接收服务器数据
        socket.BeginReceive(byteDataArr, 0, byteDataArr.Length, SocketFlags.None, Receive_Server_Data_Handle, null); 
    } 

    /// <summary>
    /// 接收服务器数据的回调方法
    /// </summary>
    /// <param name="ar">异步操作结果对象</param>
    private void Receive_Server_Data_Handle(IAsyncResult ar) 
    { 
        try 
        { 
            // 结束接收操作，获取实际接收到的数据长度
            int data_Count = socket.EndReceive(ar); 
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

                           // 将完整的数据（ID+内容）加入队列，等待Update中处理
                           byteQueue.Enqueue(tampData);

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
            
               }
               // 继续监听服务器数据（递归调用，保持持续接收）
               socket.BeginReceive(byteDataArr, 0, byteDataArr.Length, SocketFlags.None, Receive_Server_Data_Handle, null); 
        
        } 
        catch (Exception) 
        { 
            // 异常捕获（此处为空实现，实际项目中应记录日志）
        } 
    } 

    /// <summary>
    /// 向服务器发送消息
    /// </summary>
    public void SendMessage_To_Server() 
    { 
        // 将字符串"ddd"转换为UTF-8字节数组
        byte[] send_Data_Arr = Encoding.UTF8.GetBytes("ddd"); 
        
        // 异步发送数据到服务器
        socket.BeginSend(send_Data_Arr, 0, send_Data_Arr.Length, SocketFlags.None, Send_To_Server_Handle, null); 
    } 
    public void SendMessage_To_Server(int id, byte[] contextData) 
    { 
        //功能ID的byte[]
        byte[] idData = BitConverter.GetBytes(id);
        //new一个功能ID与内容的长度的byte[]
        byte[] data = new byte[idData.Length + contextData.Length];
        //将功能ID的byte[]写到data里
        Buffer.BlockCopy(idData, 0, data, 0, idData.Length);
        //将内容的byte[]写到data里(在idData的后面)
        Buffer.BlockCopy(contextData, 0, data, idData.Length, contextData.Length);
        
        //添加2字节长度前缀
        ushort len = (ushort)data.Length;
        byte[] lenData = BitConverter.GetBytes(len);
        byte[] finalData = new byte[lenData.Length + data.Length];
        Buffer.BlockCopy(lenData, 0, finalData, 0, lenData.Length);
        Buffer.BlockCopy(data, 0, finalData, lenData.Length, data.Length);
        
        // 异步发送数据到服务器
        socket.BeginSend(finalData, 0, finalData.Length, SocketFlags.None, Send_To_Server_Handle, socket); 
    } 
    

    /// <summary>
    /// 发送数据完成的回调方法
    /// </summary>
    /// <param name="ar">异步操作结果对象</param>
    private void Send_To_Server_Handle(IAsyncResult ar) 
    { 
        try 
        { 
            // 结束发送操作
            socket.EndSend(ar); 
        } 
        catch (Exception) 
        { 
            // 异常捕获（此处为空实现，实际项目中应记录日志）
        } 
    } 

    /// <summary>
    /// 更新方法（类似于Unity的Update方法，每帧调用）
    /// </summary>
    public void Update() 
    { 
        // 循环处理队列中所有待处理的数据
        while (byteQueue.Count > 0) 
        { 
            // 从队列中取出一条数据（包含ID+内容）
            byte[] r_Bytes = byteQueue.Dequeue();
            
            // 解析消息ID（前4字节）
            int netID = BitConverter.ToInt32(r_Bytes, 0);
          
            // 解析protobuf内容（ID后面的部分）
            byte[] netDesc = new byte[r_Bytes.Length - 4];
            Buffer.BlockCopy(r_Bytes, 4, netDesc, 0, netDesc.Length);
   
            // 通过消息控制器分发到对应的处理器
            MessageControll.GetInstance().Dispach(netID, netDesc); 
        } 
    } 
    
}