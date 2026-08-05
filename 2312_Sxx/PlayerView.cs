using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour,IPlayerView
{
    public Text nameText;
    public Text levelText;
    public Button levelUpBtn;

    public event Action OnLevelUpClicked;
    void Start() 
    {
        levelUpBtn.onClick.AddListener(() => OnLevelUpClicked?.Invoke());
    }
    public void UpdateDisplay(string name, int level)
    {
        nameText.text = "名称:" + name;
        levelText.text = "等级:" + level;

    }

}
