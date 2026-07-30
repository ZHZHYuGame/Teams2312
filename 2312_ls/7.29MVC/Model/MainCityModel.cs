using UnityEngine;

/// <summary>
/// 主城Model - 示例
/// </summary>
public class MainCityModel : ModelBase
{
    public int gold = 1000;
    public int diamond = 100;

    public override void Init()
    {
        Debug.Log("MainCityModel初始化");
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }
}
