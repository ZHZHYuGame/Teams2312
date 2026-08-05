using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 单个格子脚本:挂在每个滑动列表项上
/// 作用:接收UI事件(按下/拖拽/松开),转发给主控 SelectHScrollow 处理
/// 同时负责显示自身文本和控制透明度
/// </summary>
public class SelectHScrollowitem : MonoBehaviour,IDragHandler,IPointerDownHandler,IPointerUpHandler
{
    [Tooltip("名称名字(显示文本)")]  [SerializeField] private Text nameText;
    [Tooltip("画布组(用于控制透明度)")] [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("选项索引(格子在数组中的位置0~displayNumber-1)")] public int itemIndex;
    [Tooltip("信息索引(该格子当前显示的数据在itemInfos中的位置)")] public int infoIndex;
    // 主控脚本引用(在 SetInfo 时由主控传入)
    private SelectHScrollow selectHScrollow;
    [HideInInspector] public RectTransform rectTransform;
    // 本格子是否发生了拖拽(用于区分"点击"和"拖拽后松开")
    private bool isDrag;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 由主控调用:设置该格子显示的文本、数据索引、主控引用
    /// </summary>
    public void SetInfo(string name, int infoIndex, SelectHScrollow selectHScrollow)
    {
        nameText.text = name;
        this.infoIndex = infoIndex;
        this.selectHScrollow = selectHScrollow;
    }

    /// <summary>
    /// 设置透明度(由主控 ItemsControl 每帧调用,距中心越远越透明)
    /// </summary>
    public void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }

    /// <summary>
    /// 拖拽中:标记本格子已拖拽,并把事件转发给主控处理位移
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        isDrag = true;  // 标记为拖拽,松开时不触发选中
        selectHScrollow.OnDrag(eventData);  // 转发给主控移动 itemParent
    }

    /// <summary>
    /// 按下:重置拖拽标记,转发给主控标记 isDrag=true
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = false;  // 重置,等待 OnDrag 判定
        selectHScrollow.OnPointerDown(eventData);
    }

    /// <summary>
    /// 松开:如果没有拖拽过(即点击),触发选中逻辑;否则只是拖拽结束
    /// 最后转发给主控标记 isDrag=false
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        // 没拖拽过 = 纯点击,尝试选中
        if (!isDrag)
        {
            selectHScrollow.Select(itemIndex, infoIndex, rectTransform);
        }

        selectHScrollow.OnPointerUp(eventData);
    }
}
