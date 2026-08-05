using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerView
{
    void UpdateDisplay(string name,int level);
    event Action OnLevelUpClicked;//用户点击升级按钮


}
