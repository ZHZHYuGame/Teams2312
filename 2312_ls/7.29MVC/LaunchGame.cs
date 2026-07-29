using UnityEngine;

/// <summary>
/// 游戏启动类
/// </summary>
public class LaunchGame : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        // 1: 加载配置数据
        ConfigManager.Ins.LoadAllConfig();

        // 2: 注册Model
        ModelManager.Ins.AddModel("MainCityModel", new MainCityModel());

        // 3: 注册Controller
        ControllerManager.Ins.AddController("MainCityController", new MainCityController());

        // 4: 打开主城面板
        UIManager.Ins.OpenUI(PanelName.MainCityPanel);
    }
}
