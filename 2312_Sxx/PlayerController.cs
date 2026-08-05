using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerModel model;
    private IPlayerView view;
    public PlayerController(PlayerModel model,IPlayerView view) 
    {
        this.model = model;
        this.view = view;

        view.OnLevelUpClicked += HandleLevelUp;
        RefreshView();
    }

    private void RefreshView()
    {
        view.UpdateDisplay(model.Name,model.Level);
    }

    private void HandleLevelUp()
    {
        model.LevelUp();
        RefreshView();
    }
}
