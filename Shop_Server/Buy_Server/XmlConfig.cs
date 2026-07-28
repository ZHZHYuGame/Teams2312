using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using MyGame;

public class XmlConfig : Singleton<XmlConfig>
{
    //private static XmlConfig _xmlConfig;
    //public static XmlConfig GetInstance()
    //{
    //    if (_xmlConfig == null)
    //        _xmlConfig = new XmlConfig();
    //    return _xmlConfig;
    //}
    /// <summary>
    /// 服务器数据列表
    /// </summary>
    private List<ServerConfigData> _serverList = new List<ServerConfigData>();
    public List<ServerConfigData> ServerList => _serverList;

    /// <summary>
    /// 场景数据列表
    /// </summary>
    private List<SceneConfigData> _sceneList = new List<SceneConfigData>();
    public List<SceneConfigData> SceneList => _sceneList;
    
    
    // Start is called before the first frame update

    void Start()
    {
    }

    /// <summary>
    /// //创建服务器xml 
    /// </summary>
    /// <param name="id">服务器id</param>
    /// <param name="serverIP">服务器ip</param>
    /// <param name="serverPort">服务器端口号</param>
    /// <param name="serverName">服务器名称</param>
    public void CreateXML(string id, string serverIP, string serverPort, string serverName)
    {
        string path = "D:/2312A/Net/Server_2312" + "/Server.xml";
        //Debug.Log(path);
        if (!File.Exists(path))
        {
            //创建
            XmlDocument xml = new XmlDocument();
            //创建最上一层的节点。
            XmlElement root = xml.CreateElement("Server");
            //创建子节点
            XmlElement element = xml.CreateElement("Server" + id);
            //设置节点的属性
            element.SetAttribute("id", id);
            element.SetAttribute("serverIP", serverIP);
            element.SetAttribute("serverPort", serverPort);
            element.SetAttribute("serverName", serverName);
            //把节点一层一层的添加至xml中，注意他们之间的先后顺序，这是生成XML文件的顺序
            root.AppendChild(element);
            xml.AppendChild(root);
            //最后保存文件
            xml.Save(path);
        }
    }

    /// <summary>
    /// 创建场景布置配置文件
    /// </summary>
    /// <param name="id">布置物体的ID，这里设置为地图块的索引</param>
    /// <param name="path">物体的加载路径</param>
    /// <param name="position">物体的显示位置</param>
    /// <param name="type">物体的类型，例如可碰撞、不可碰撞等</param>
    /// <param name="width">物体的大小，暂定长、宽相同的比例</param>
    public void CreateXML(string id, string loadPath, string position, string type, int width, string trigger = "")
    {
        string path = "D:/2312A/Net/Server_2312" + "/OutPath/SceneLayer1.xml";
        //Debug.Log(path);
        if (!File.Exists(path))
        {
            //创建
            XmlDocument xml = new XmlDocument();
            //创建最上一层的节点。
            XmlElement root = xml.CreateElement("Scene");
            //创建子节点
            XmlElement element = xml.CreateElement("Layer" + id);
            //设置节点的属性
            element.SetAttribute("id", id);
            element.SetAttribute("loadPath", loadPath);
            element.SetAttribute("position", position);
            element.SetAttribute("type", type);
            element.SetAttribute("width", width.ToString());
            //把节点一层一层的添加至xml中，注意他们之间的先后顺序，这是生成XML文件的顺序
            root.AppendChild(element);
            xml.AppendChild(root);
            //最后保存文件
            xml.Save(path);
        }
    }
    /// <summary>
    /// 创建角色数据的XML
    /// </summary>
    /// <param name="id"></param>
    /// <param name="serverIP"></param>
    /// <param name="serverPort"></param>
    /// <param name="serverName"></param>
    public void CreateXML(string acc,string GUID, string roleName, string type, string loadPath)
    {
        string path = "D:/2312A/Net/Server_2312" + "/Roles.xml";
        if (!File.Exists(path))
        {
            //创建
            XmlDocument xml = new XmlDocument();
            //创建最上一层的节点。
            XmlElement root = xml.CreateElement("Role");
            //创建子节点
            XmlElement element = xml.CreateElement("Role" + GUID);
            //设置节点的属性
            element.SetAttribute("id", GUID);
            element.SetAttribute("acc", acc);
            element.SetAttribute("roleName", roleName);
            element.SetAttribute("jobType", type);
            element.SetAttribute("loadPath", loadPath);
            //把节点一层一层的添加至xml中，注意他们之间的先后顺序，这是生成XML文件的顺序
            root.AppendChild(element);
            xml.AppendChild(root);
            //最后保存文件
            xml.Save(path);
        }
    }
    
    
    //加载
    public void LoadXml()
    {
        //创建xml文档
        XmlDocument xml = new XmlDocument();
        xml.Load("D:/2312A/Net/Server_2312" + "/Server.xml");
        //得到objects节点下的所有子节点
        XmlNodeList xmlNodeList = xml.SelectSingleNode("Server").ChildNodes;
        //遍历所有子节点
        foreach (XmlElement xl1 in xmlNodeList)
        {
            ServerConfigData sConfigData = new ServerConfigData()
            {
                id = xl1.GetAttribute("id"),
                serverIP = xl1.GetAttribute("serverIP"),
                serverPort = xl1.GetAttribute("serverPort"),
                serverName = xl1.GetAttribute("serverName")
            };
            _serverList.Add(sConfigData);
        }
        //print(xml.OuterXml);
    }

    //加载
    public void LoadSceneXml()
    {
        //创建xml文档
        XmlDocument xml = new XmlDocument();
        xml.Load("D:/2312A/Net/Server_2312" + "/OutPath/SceneLayer1.xml");
        //得到objects节点下的所有子节点
        XmlNodeList xmlNodeList = xml.SelectSingleNode("Scene").ChildNodes;
        //遍历所有子节点
        foreach (XmlElement xl1 in xmlNodeList)
        {
            SceneConfigData sConfigData = new SceneConfigData()
            {
                id = xl1.GetAttribute("id"),
                loadPath = xl1.GetAttribute("loadPath"),
                position = xl1.GetAttribute("position"),
                type = xl1.GetAttribute("type"),
                width = xl1.GetAttribute("width")
            };
            _sceneList.Add(sConfigData);
        }
        //print(xml.OuterXml);
    }

    /// <summary>
    /// 添加场景布置配置文件
    /// </summary>
    /// <param name="id">布置物体的ID，这里设置为地图块的索引</param>
    /// <param name="path">物体的加载路径</param>
    /// <param name="position">物体的显示位置</param>
    /// <param name="type">物体的类型，例如可碰撞、不可碰撞等</param>
    /// <param name="width">物体的大小，暂定长、宽相同的比例</param>
    public void addXMLData(string id, string loadPath, string position, string type, int width, string trigger = "")
    {
        string path = "D:/2312A/Net/Server_2312" + "/OutPath/SceneLayer1.xml";
        if (File.Exists(path))
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNode root = xml.SelectSingleNode("Scene");
            //下面的东西就跟上面创建xml元素是一样的。我们把他复制过来就行了
            XmlElement element = xml.CreateElement("Layer" + id);
            //设置节点的属性
            element.SetAttribute("id", id);
            element.SetAttribute("loadPath", loadPath);
            element.SetAttribute("position", position);
            element.SetAttribute("type", type);
            element.SetAttribute("width", width.ToString());
            root.AppendChild(element);
            xml.AppendChild(root);
            //最后保存文件
            xml.Save(path);
        }
        else
        {
            //没有文件 就创建添加
            CreateXML(id, loadPath, position, type, width);
        }
    }

    /// <summary>
    /// 添加Xml数据
    /// </summary>
    /// <param name="id">服务器id</param>
    /// <param name="serverIP">服务器ip</param>
    /// <param name="serverPort">服务器端口号</param>
    /// <param name="serverName">服务器名称</param>
    public void addXMLData(string id, string serverIP, string serverPort, string serverName)
    {
        string path = "D:/2312A/Net/Server_2312" + "/Server.xml";
        if (File.Exists(path))
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNode root = xml.SelectSingleNode("Server");
            //下面的东西就跟上面创建xml元素是一样的。我们把他复制过来就行了
            XmlElement element = xml.CreateElement("Server" + id);
            //设置节点的属性
            element.SetAttribute("id", id);
            element.SetAttribute("serverIP", serverIP);
            element.SetAttribute("serverPort", serverPort);
            element.SetAttribute("serverName", serverName);
            root.AppendChild(element);
            xml.AppendChild(root);
            //最后保存文件
            xml.Save(path);
        }
        else
        {
            //没有文件 就创建添加
            CreateXML(id, serverIP, serverPort, serverName);
        }
    }
    /// <summary>
    /// 添加角色
    /// </summary>
    /// <param name="id"></param>
    /// <param name="loadPath"></param>
    /// <param name="position"></param>
    /// <param name="type"></param>
    /// <param name="width"></param>
    /// <param name="trigger"></param>
    public void addXMLData(string acc,string GUID, string roleName, string type, string loadPath)
    {
        string path = "D:/2312A/Net/Server_2312" + "/Roles.xml";
        if (File.Exists(path))
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(path);
            XmlNode root = xml.SelectSingleNode("Role");
            //下面的东西就跟上面创建xml元素是一样的。我们把他复制过来就行了
            XmlElement element = xml.CreateElement("id" + GUID);
            //设置节点的属性
            element.SetAttribute("id", GUID);
            element.SetAttribute("acc", acc);
            element.SetAttribute("roleName", roleName);
            element.SetAttribute("jobType", type);
            element.SetAttribute("loadPath", loadPath);
            root.AppendChild(element);
            xml.AppendChild(root);
            //最后保存文件
            xml.Save(path);
        }
        else
        {
            //没有文件 就创建添加
            CreateXML(acc,GUID, roleName, type, loadPath);
        }
    }
    
    //加载
    public void LoadUserXml()
    {
        //创建xml文档
        
    }
}




/// <summary>
/// 服务器数据
/// </summary>
public class ServerConfigData
{
    public string id;
    public string serverIP;
    public string serverPort;
    public string serverName;
}

/// <summary>
/// 场景布置的物体数据
/// </summary>
public class SceneConfigData
{
    public string id;
    public string loadPath;
    public string position;
    public string type;
    public string width;
}
