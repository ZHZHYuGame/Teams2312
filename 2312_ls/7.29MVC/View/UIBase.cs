using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI基类
/// </summary>
public class UIBase : MonoBehaviour
{
    public PanelType panelType;

    // 绑定的Controller名称（可选）
    public string controllerName;

    // 初始化
    public virtual void Init()
    {
        Button[] btns = GetComponentsInChildren<Button>();
        foreach (Button btn in btns)
        {
            btn.onClick.AddListener(() =>
            {
                OnBtnClick(btn.name);
            });
        }
    }

    // 按钮点击 - 转发给Controller
    public virtual void OnBtnClick(string btnName)
    {
        if (!string.IsNullOrEmpty(controllerName))
        {
            ControllerBase controller = ControllerManager.Ins.GetController<ControllerBase>(controllerName);
            if (controller != null)
            {
                controller.HandleAction(btnName);
            }
        }
    }

    // 显示
    public void Show()
    {
        gameObject.SetActive(true);
    }

    // 隐藏
    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
