using UnityEngine;

/// <summary>
/// 主城Controller - 示例
/// </summary>
public class MainCityController : ControllerBase
{
    MainCityModel model;

    public override void Init()
    {
        model = ModelManager.Ins.GetModel<MainCityModel>("MainCityModel");
    }

    public override void HandleAction(string actionName, object param = null)
    {
        switch (actionName)
        {
            case "BtnShop":
                UIManager.Ins.OpenUI(PanelName.ShopPanel);
                break;
            case "BtnBag":
                UIManager.Ins.OpenUI(PanelName.BagPanel);
                break;
            case "BtnAddGold":
                model.AddGold(100);
                Debug.Log("金币: " + model.gold);
                break;
            default:
                Debug.Log("未处理的操作: " + actionName);
                break;
        }
    }
}
