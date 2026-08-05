using System;
using System.Collections.Generic;
using UnityEngine;

namespace MVC.Core
{
    /// <summary>
    /// UI 层级枚举，从底到顶。
    /// </summary>
    public enum UILayer
    {
        Background = 0,
        Normal = 1,
        PopUp = 2,
        Top = 3,
    }

    /// <summary>
    /// UI 面板配置：预制体路径和层级。
    /// </summary>
    public struct UIConfig
    {
        public string prefabPath;
        public UILayer layer;

        public UIConfig(string path, UILayer uiLayer)
        {
            prefabPath = path;
            layer = uiLayer;
        }
    }

    /// <summary>
    /// UI 面板注册表。在此注册所有面板：
    /// UIType.Register("MainPanel", "UI/MainPanel", UILayer.Normal);
    /// </summary>
    public static class UIType
    {
        private static readonly Dictionary<string, UIConfig> _configs = new Dictionary<string, UIConfig>();

        public static UIConfig GetConfig(string panelName)
        {
            if (_configs.TryGetValue(panelName, out var config))
                return config;
            Debug.LogWarning($"[UIType] Panel '{panelName}' is not registered.");
            return default;
        }

        public static void Register(string panelName, string prefabPath, UILayer layer)
        {
            _configs[panelName] = new UIConfig(prefabPath, layer);
        }
    }
}