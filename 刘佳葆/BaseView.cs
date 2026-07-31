using System;
using UnityEngine;

/// <summary>
/// view基类
/// </summary>
public abstract class BaseView : MonoBehaviour
{
    /// <summary>
    /// 是否已经初始化
    /// </summary>
     public bool isInited { get; private set; }
    /// <summary>
    /// 初始化view
    /// </summary>
    public void Init()
    {
        if (isInited) return;
        isInited = true;
        OnInit();
    }
    /// <summary>
    /// 显示View
    /// </summary>
    public void Show()
    {
        if (!isInited) Init();
        gameObject.SetActive(true);
        OnShow();
    }
    /// <summary>
    /// 隐藏View
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
        OnHide();
    }
    /// <summary>
    /// 销毁View
    /// </summary>
    public void Destroy()
    {
        OnDestroyView();
        if (gameObject != null) Destroy(gameObject);
    }
    protected virtual void OnInit() { }
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }
    protected virtual void OnDestroyView() { }
}
