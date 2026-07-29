/// <summary>
/// 面板类型
/// </summary>
public enum PanelType
{
    None,
    Main,       // 主界面
    Normal,     // 普通面板
    HuChi,      // 互斥面板
    Module,     // 模态面板
    popUI,      // 弹出提示
}

/// <summary>
/// 面板名称 - 对应Resources/UIPrefabs下的Prefab名
/// </summary>
public enum PanelName
{
    None,
    AwardPanel,
    BagPanel,
    ChatPanel,
    EmailPanel,
    FriendPanel,
    MainCityPanel,
    ShopPanel,
    WjPanel,
    PopPanel,
    InventoryInfoPanel,
    BuyInfoPanel,
    AlertPanel,
    EmoPanel,
}
