using Data;
using PureMVC.Patterns.Facade;
/// <summary>
/// 背包门面
/// </summary>
public class BagFacade : Facade
{
    public string bagFacade { get;private set; }
    public BagFacade(string key) : base(key)
    {
        bagFacade = key; 
    }

    /// <summary>
    /// BagFacade启动初始化
    /// </summary>
    public void StartUp()
    {
        //注册背包Proxy逻辑代理
        RegisterProxy(new BagProxy("msg_Add"));
        //注册购买命令
        RegisterCommand(NotificationName.BAG_ADDITEM,()=>new AddToBagCommand());
    }

   
}