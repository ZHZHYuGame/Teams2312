using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI管理器
/// </summary>
public class UIManager : Singleton<UIManager>
{
    // 缓存所有UI
    Dictionary<string, UIBase> allUI = new Dictionary<string, UIBase>();

    // 普通面板
    Dictionary<string, UIBase> normalUI = new Dictionary<string, UIBase>();

    // 互斥面板
    Stack<UIBase> hcUI = new Stack<UIBase>();

    // 模态面板
    Stack<UIBase> moduleUI = new Stack<UIBase>();

    public Transform canvas;

    public UIManager()
    {
        canvas = GameObject.Find("Canvas").transform;
    }

    /// <summary>
    /// 加载UI
    /// </summary>
    UIBase LoadUI(PanelName panelName)
    {
        string fileName = panelName.ToString();
        UIBase myPanel = null;

        if (allUI.ContainsKey(fileName))
        {
            return allUI[fileName];
        }

        myPanel = ResourceManager.Ins.LoadRes<UIBase>("UIPrefabs", fileName);
        if (myPanel == null)
        {
            Debug.LogError("找不到UIBase脚本: " + fileName);
        }

        myPanel = GameObject.Instantiate(myPanel, canvas);
        allUI.Add(fileName, myPanel);
        myPanel.Init();

        return myPanel;
    }

    /// <summary>
    /// 打开UI
    /// </summary>
    public void OpenUI(PanelName panelName)
    {
        UIBase myPanel = LoadUI(panelName);

        switch (myPanel.panelType)
        {
            case PanelType.Main:
                break;

            case PanelType.Normal:
                if (hcUI.Count > 0)
                {
                    return;
                }
                if (!normalUI.ContainsKey(panelName.ToString()))
                {
                    normalUI.Add(panelName.ToString(), myPanel);
                }
                break;

            case PanelType.HuChi:
                // 隐藏所有普通面板
                foreach (var item in normalUI)
                {
                    item.Value.Hide();
                }
                // 隐藏当前互斥栈顶
                if (hcUI.Count > 0)
                {
                    hcUI.Peek().Hide();
                }
                hcUI.Push(myPanel);
                break;

            case PanelType.Module:
                moduleUI.Push(myPanel);
                break;
        }

        myPanel.Show();
        myPanel.transform.SetAsLastSibling();
    }

    /// <summary>
    /// 关闭UI
    /// </summary>
    public UIBase CloseUI(PanelName panelName)
    {
        UIBase myPanel = allUI[panelName.ToString()];

        switch (myPanel.panelType)
        {
            case PanelType.Main:
                break;

            case PanelType.Normal:
                if (normalUI.ContainsKey(panelName.ToString()))
                {
                    normalUI.Remove(panelName.ToString());
                }
                break;

            case PanelType.HuChi:
                hcUI.Pop().Hide();
                if (hcUI.Count > 0)
                {
                    hcUI.Peek().Show();
                }
                else
                {
                    foreach (var item in normalUI)
                    {
                        item.Value.Show();
                    }
                }
                break;

            case PanelType.Module:
                if (moduleUI.Peek() != myPanel)
                {
                    return null;
                }
                moduleUI.Pop().Hide();
                break;
        }

        myPanel.Hide();
        return myPanel;
    }
}
