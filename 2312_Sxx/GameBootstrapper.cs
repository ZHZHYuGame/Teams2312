using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    public PlayerView playerView; // 在 Inspector 中拖入 UI 面板

    void Start()
    {
        PlayerModel model = new PlayerModel("勇者", 1);
        PlayerController controller = new PlayerController(model, playerView);
        // 此时 View、Model、Controller 已完全绑定，可以正常运行
    }
}
