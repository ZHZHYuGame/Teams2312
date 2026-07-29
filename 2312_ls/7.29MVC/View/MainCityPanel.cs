using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 主城面板 - 示例View
/// </summary>
public class MainCityPanel : UIBase
{
    public Text goldText;

    public override void Init()
    {
        base.Init();
        controllerName = "MainCityController";
    }

    public override void OnBtnClick(string btnName)
    {
        base.OnBtnClick(btnName);

        // 刷新UI
        if (goldText != null)
        {
            MainCityModel model = ModelManager.Ins.GetModel<MainCityModel>("MainCityModel");
            goldText.text = "金币: " + model.gold;
        }
    }
}
