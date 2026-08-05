using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 无限横向滑动列表（选择器）
/// 核心：用固定数量的格子(displayNumber)展示任意长度的数据(itemInfos)
/// 通过移动 itemParent + 格子内容循环刷新，实现"无限"滚动的视觉效果
/// 中间格子最大最亮，两边格子逐渐缩小变透明
/// </summary>
public class SelectHScrollow : MonoBehaviour,IPointerDownHandler,IPointerUpHandler,IDragHandler
{
    [Serializable]
    private struct ItemInfo
    {
        public string name;

        public ItemInfo(string name)
        {
            this.name = name;
        }
    }
    [Tooltip("选项预制体")]
    [SerializeField] private GameObject itemPrefab;
    [Tooltip("选项父物体(整体被拖动的容器)")]
    [SerializeField] private RectTransform itemParent;

    [Tooltip("选项信息(所有可显示的数据)")] [SerializeField] private ItemInfo[] itemInfos;

    [Tooltip("最大显示数量(尽量填奇数),(偶数会在中间显示俩个)")] [SerializeField] private int displayNumber;

    [Tooltip("选项间隔(每个格子之间的水平距离)")] [SerializeField] private float itemSpace;

    [Tooltip("移动插帧(松手后吸附到目标格子的平滑值,越大吸附越快)")] [SerializeField] private float moveSmooth;
    [Tooltip("拖拽速度(拖动时位移的放大倍率)")] [SerializeField] private float dragSpeed;

    [Tooltip("缩放倍率(距中心越远缩得越小,建议0.001量级)")] [SerializeField] private float scaleMultiplying;

    [Tooltip("透明度倍率(距中心越远越透明,建议0.001量级)")] [SerializeField] private float alphaMultiplying;

    // 选中某个格子时触发的事件,参数为该格子对应的数据索引 infoIndex
    public event Action<int> SelectAction;

    // 实例化出来的所有格子组件
    private SelectHScrollowitem[] items;

    // 所有显示格子的总宽度 = (displayNumber-1) * itemSpace,用于计算格子初始排布
    private float displayWidth;

    // 当前 itemParent 已偏移的"格子数"(向左为正,向右为负)
    // 决定了哪个数据应该显示在中间
    private int offsetTimes;

    // 是否正在拖拽(按下期间为 true,松手后为 false)
    private bool isDrag;

    // 当前最靠近中心(被选中)的格子的 itemIndex
    private int currentItemIndex;

    // 每帧记录每个格子到中心的距离,用于找最小值
    private float[] distances;

    // 点击非中心格子时,记录该格子的世界x坐标(用于选中移动)
    private float selectItemX;

    // 是否处于"点击非中心格子,自动滚动到该格子"的状态
    private bool isSelectMove;

    // 是否已经选中过(防止重复触发 SelectAction)
    private bool isSelected;


    // Start is called before the first frame update
    void Start()
    {
        Init();      // 实例化所有格子
        MoveItems(0); // 初始排布位置 + 赋值文本(中间显示第0个数据)
    }

    /// <summary>
    /// 初始化:计算总宽度,实例化 displayNumber 个格子到 itemParent 下
    /// </summary>
    private void Init()
    {
        // 总宽度 = 格子数-1 个间隔(因为中间格子位置为0,两边对称展开)
        displayWidth = (displayNumber - 1) * itemSpace;
        items = new SelectHScrollowitem[displayNumber];
        for (int i = 0; i < displayNumber; i++)
        {
            // 实例化预制体并挂到 itemParent 下
            SelectHScrollowitem item = Instantiate(itemPrefab, itemParent).GetComponent<SelectHScrollowitem>();
            item.itemIndex = i;  // 记录格子在数组中的位置索引(0~displayNumber-1)
            items[i] = item;
        }
    }

    /// <summary>
    /// 外部调用:设置全部数据(如1~100的字符串),会清空之前的选中状态
    /// </summary>
    public void SetItemsInfo(string[] names)
    {
        itemInfos=new ItemInfo[names.Length];
        for (int i = 0; i < itemInfos.Length; i++)
        {
            itemInfos[i] = new ItemInfo(names[i]);
        }
        SelectAction = null;  // 清空事件,避免上一次的监听残留
        isSelected = false;   // 重置选中状态
    }

    /// <summary>
    /// 核心:根据偏移量重新排布所有格子的位置和显示内容
    /// 1. 先按 offsetTimes 计算每个格子的 x 坐标(实现拖动时的位移)
    /// 2. 再算出"中间格子"应该显示的数据索引 middle
    /// 3. 从中间向右依次赋值,从中间向左依次赋值(形成循环)
    /// </summary>
    /// <param name="offsetTimes">当前偏移的格子数</param>
    private void MoveItems(int offsetTimes)
    {
        // === 第一步:重新设置每个格子的位置 ===
        // 格子i的位置 = 间隔 * (i - offsetTimes) - 总宽度/2
        // i - offsetTimes:格子相对中间的偏移; -displayWidth/2:让整体居中
        for (int i = 0; i < displayNumber; i++)
        {
            float x = itemSpace * (i - offsetTimes) - displayWidth / 2;
            items[i].rectTransform.localPosition = new Vector2(x, items[i].rectTransform.localPosition.y);
        }

        // === 第二步:算出中间格子应该显示哪个数据 ===
        // offsetTimes>0:表示向左拖动了,中间数据应该往回退(数据索引从尾部算)
        // offsetTimes<=0:表示向右拖动了,中间数据应该往前推
        int middle;
        if (offsetTimes>0)
        {
            middle = itemInfos.Length - offsetTimes % itemInfos.Length;
        }
        else
        {
            middle = -offsetTimes % itemInfos.Length;
        }

        // === 第三步:从中间向右赋值(中间 → 最右) ===
        // 中间格子显示 middle,向右每个格子显示下一个数据(到末尾循环回首)
        int infoIndex = middle;
        // Mathf.FloorToInt(displayNumber/2f) 即中间格子的索引,例如7→3,5→2
        for (int i = Mathf.FloorToInt(displayNumber/2f); i <displayNumber; i++)
        {

            if (infoIndex>=itemInfos.Length)  // 越界则循环回首
            {
                infoIndex = 0;
            }
            items[i].SetInfo(itemInfos[infoIndex].name,infoIndex,this);
            infoIndex++;
        }

        // === 第四步:从中间向左赋值(中间左边一个 → 最左) ===
        // 中间左边第一个显示 middle-1,向左每个格子显示上一个数据(到-1循环回尾)
        infoIndex = middle - 1;
        for (int i = Mathf.FloorToInt(displayNumber/2f)-1; i >=0; i--)
        {
            if (infoIndex<=-1)  // 越界则循环回尾
            {
                infoIndex = itemInfos.Length - 1;
            }
            items[i].SetInfo(itemInfos[infoIndex].name,infoIndex,this);
            infoIndex--;
        }
    }


    // Update is called once per frame
    void Update()
    {
        // 松手状态下:检查是否需要刷新格子内容 + 吸附到最近格子
        if (!isDrag)
        {
            // 用 itemParent 的当前x位置反算偏移了多少个格子
            int currentOffsetTimes=Mathf.FloorToInt(itemParent.localPosition.x/itemSpace);
            // 偏移量变化时,说明跨越了一个格子,需要重新刷新内容(让新进入中心的格子显示对应数据)
            if (currentOffsetTimes != offsetTimes)
            {
                offsetTimes = currentOffsetTimes;
                MoveItems(offsetTimes);
            }
            // 平滑吸附到最近的格子位置
            Adsorption();
        }
        // 无论是否拖拽,都实时计算每个格子的缩放和透明度
        // 这样拖动时格子经过中心会有"小→大→小"的渐变效果
        ItemsControl();
    }

    /// <summary>
    /// 吸附:松手后,把 itemParent 平滑移动到最近的格子整数倍位置
    /// 例如 itemSpace=400,x=450 会吸附到 400;x=250 会吸附到 400或0(看谁更近)
    /// </summary>
    private void Adsorption()
    {
        float targetX;
        if (!isSelectMove)  // 不在"点击选中自动滚动"状态时才吸附
        {
            // distance = 当前x对itemSpace取余,范围 (-itemSpace, itemSpace)
            float distance = itemParent.localPosition.x % itemSpace;
            // times = 当前x包含多少个完整的itemSpace(向下取整)
            int times=Mathf.FloorToInt(itemParent.localPosition.x/itemSpace);

            // 根据余数判断该吸附到左边(times*itemSpace)还是右边((times+1)*itemSpace)
            if (distance>=0)
            {
                if (distance<itemSpace/2)
                {
                    // 余数小于半格,吸附到左边
                    targetX = times * itemSpace;
                }
                else
                {
                    // 余数大于半格,吸附到右边
                    targetX = (times + 1) * itemSpace;
                }
            }
            else
            {
                if (distance<-itemSpace/2)
                {
                    // 负方向超过半格,吸附到左边
                    targetX = times * itemSpace;
                }
                else
                {
                    // 负方向小于半格,吸附到右边
                    targetX = (times + 1) * itemSpace;
                }
            }

            // 用 Lerp 平滑过渡到目标位置,moveSmooth/10 控制平滑度
            itemParent.localPosition = new Vector2(Mathf.Lerp(itemParent.localPosition.x, targetX, moveSmooth / 10),
                itemParent.localPosition.y);
        }
    }

    /// <summary>
    /// 每帧控制:根据每个格子距中心的距离,计算缩放、透明度,并找出当前中心格子
    /// </summary>
    private void ItemsControl()
    {
        distances = new float[displayNumber];
        // 遍历所有格子,根据距中心距离设置缩放和透明度
        for (int i = 0; i < displayNumber; i++)
        {
            // 格子世界x - 自身世界x = 格子距中心的绝对距离
            float distance=Mathf.Abs(items[i].rectTransform.position.x - transform.position.x);
            distances[i] = distance;
            // 距离越远,scale越小(1 - 距离*倍率)
            float scale = 1 - distance * scaleMultiplying;
            items[i].rectTransform.localScale = new Vector3(scale, scale, 1);
            // 距离越远,alpha越小(越透明)
            items[i].SetAlpha(1-distance*alphaMultiplying);
        }

        // 找出距中心最近的格子,作为当前选中项
        float minDistance = itemSpace * displayNumber;
        int minIndex = 0;
        for (int i = 0; i < displayNumber; i++)
        {
            if (distances[i] < minDistance)
            {
             minDistance=distances[i];
                minIndex = i;
            }
        }
        // 记录当前中心格子的 itemIndex
        currentItemIndex=items[minIndex].itemIndex;
    }

    /// <summary>
    /// 拖拽中:按拖拽增量移动 itemParent(所有格子作为子物体一起移动)
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        isSelectMove = false;  // 用户主动拖拽时,取消"自动选中滚动"状态
        // 在当前x基础上加上拖拽增量*速度
        itemParent.localPosition = new Vector2(itemParent.localPosition.x + eventData.delta.x * dragSpeed,
            itemParent.localPosition.y);
    }

    /// <summary>
    /// 按下:标记正在拖拽,Update中停止吸附和内容刷新
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
    }

    /// <summary>
    /// 松手:标记结束拖拽,Update中恢复吸附和内容刷新
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        isDrag = false;
    }

    /// <summary>
    /// 点击格子的处理(由 SelectHScrollowitem.OnPointerUp 转发调用)
    /// - 若点的是当前中心格子:触发选中事件
    /// - 若点的不是中心格子:进入"自动滚动到该格子"状态
    /// </summary>
    public void Select(int itemIndex, int infoIndex, RectTransform rectTransform)
    {
        // 还没选中过 + 点的就是当前中心格子 → 触发选中事件
        if (!isSelected&&itemIndex==currentItemIndex)
        {
            SelectAction?.Invoke(infoIndex);
            isSelected = true;
        }
        else
        {
            // 点的不是中心格子,标记自动滚动,记录目标格子的世界x
            isSelectMove = true;
            selectItemX=rectTransform.position.x;
        }
    }
}
