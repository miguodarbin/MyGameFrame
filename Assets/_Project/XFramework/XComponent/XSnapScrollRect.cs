using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

/*

0. 这个是挂在ScrollRect对象下的
1. 有一个固定点：PerfectPoint
2. 每个 Item 有自己的 pivot 点
3. 松手后，遍历所有 Item
4. 找到 pivot 点距离 PerfectPoint 最近的 Item
5. 计算这个 Item 的 pivot 点到 PerfectPoint 的偏移量
6. 把 Content 整体移动这段偏移量

 */
public class XSnapScrollRect : MonoBehaviour
{
    public RectTransform contentRect; //content 的 RectTransform
    public RectTransform perfectRect; //perfect 的 RectTransform
    public float snapSkipVelocityThreshold = 3000f;
    private RectTransform _parentRect; //content 和 perfect的 parent 的 RectTransform
    public List<RectTransform> itemList = new List<RectTransform>(); //识别到的 item
    private int _currentSnapedIndex = -1;
    private Vector2 _perfectCenter;


    private Tween _tween; //拿到 tween 对象，在失活的时候kill 掉

    //初始化的时候就添加自定义事件组件
    private void Awake()
    {
        _parentRect = transform as RectTransform;
        _perfectCenter = GetCenterPointInParent(perfectRect, _parentRect);
        AddDragEvent();
    }

    //开始的时候再获得组件，担心没ui加载完
    private void Start()
    {
        GetAllItems();
    }


    //先获得所有的Items
    private void GetAllItems()
    {
        foreach (Transform child in contentRect.transform)
        {
            if (child.name.Contains("Item"))
            {
                var childRect = child as RectTransform;
                itemList.Add(childRect);
            }
        }
    }


    //添加自定义拖动事件
    private EventTrigger.Entry onBeginDragEntry;
    private EventTrigger.Entry onDragEntry;
    private EventTrigger.Entry onEndDragEntry;

    private void AddDragEvent()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        onBeginDragEntry = new EventTrigger.Entry();
        onBeginDragEntry.eventID = EventTriggerType.BeginDrag;
        eventTrigger.triggers.Add(onBeginDragEntry);

        onDragEntry = new EventTrigger.Entry();
        onDragEntry.eventID = EventTriggerType.Drag;
        eventTrigger.triggers.Add(onDragEntry);

        onEndDragEntry = new EventTrigger.Entry();
        onEndDragEntry.eventID = EventTriggerType.EndDrag;
        eventTrigger.triggers.Add(onEndDragEntry);
    }

    private void OnEnable()
    {
        onBeginDragEntry.callback.AddListener(OnBeginDrag);
        onDragEntry.callback.AddListener(OnDrag);
        onEndDragEntry.callback.AddListener(OnEndDrag);
        if (itemList.Count == 0)
        {
            StartCoroutine(DelayFirstSnap());
        }
    }

    //一开可能还没找到item并且Layout还没有排完，不过问题不大，延迟吸附
    private IEnumerator DelayFirstSnap()
    {
        while (itemList.Count == 0)
        {
            yield return null;
        }

        // 让整个 UI 系统先把该刷新的东西刷一下
        Canvas.ForceUpdateCanvases();
        // 明确告诉 content：你下面的 Layout 现在马上重新排版
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);

        SnapItemToPerfect(CalculateClosestItemIndex(), false);
    }

    private void OnDisable()
    {
        _tween?.Kill();
        onBeginDragEntry.callback.RemoveListener(OnBeginDrag);
        onDragEntry.callback.RemoveListener(OnDrag);
        onEndDragEntry.callback.RemoveListener(OnEndDrag);
    }

    //====================开始拖拽的逻辑=======================
    private void OnBeginDrag(BaseEventData eventData)
    {
        //开始拖拽的时候就要情况一下字典了,防止脏数据
        _itemToPerfectOffsetXDict.Clear();
        _tween?.Kill();
    }


    //====================拖拽中的逻辑=======================
    private void OnDrag(BaseEventData eventData)
    {
        //处理 content 的拖动逻辑
        var data = eventData as PointerEventData;
        var contentAnchoredPosX = contentRect.anchoredPosition.x;
        var targetAnchoredPosX = contentAnchoredPosX + data.delta.x;
        contentRect.anchoredPosition = new Vector2(targetAnchoredPosX, contentRect.anchoredPosition.y);

        //算一下总速度
        CalculateTotalDragSpeedXOnDraging(eventData as PointerEventData);
    }


    //====================结束拖拽的逻辑=======================
    private void OnEndDrag(BaseEventData eventData)
    {
        //2.然后还要判断拖拽速度，维护一个当前吸附的索引号，所以说一开始就要先吸附一次
        //2.1 - 如果速度小于 3000，snapIndex 就是最近的那一个
        //2.2 - 如果速度大于了 3000，最近的那个还是当前吸附的，那就根据方向把当前吸附的索引号+1或-1，去吸别的
        //2.3 - 如果速度大于了 3000，最近的那个不是当前吸附的，那就吸最近的

        //算一下平均速度、当前离perfect最近的item的index
        CalculateAverageDragSpeedXOnEndDrag();
        int closetItemIndex = CalculateClosestItemIndex();

        //分情况判断
        if (Mathf.Abs(_averageDragSpeedX) < snapSkipVelocityThreshold) //如果拖动的速度小于阈值的话，直接吸附最近的Item到perfect点
        {
            SnapItemToPerfect(closetItemIndex);
        }
        else //如果拖动的速度大于阈值的话，分情况讨论
        {
            if (closetItemIndex == _currentSnapedIndex) //如果最近的还是当前吸附的，那就根据方向把吸附索引号+1或-1
            {
                if (_averageDragSpeedX > 0) //说明往右拖,index应该减
                {
                    var needSnapIndex = closetItemIndex - 1;
                    needSnapIndex = Mathf.Clamp(needSnapIndex, 0, itemList.Count - 1);
                    SnapItemToPerfect(needSnapIndex);
                }
                else //说明往左拖,index应该加
                {
                    var needSnapIndex = closetItemIndex + 1;
                    needSnapIndex = Mathf.Clamp(needSnapIndex, 0, itemList.Count - 1);
                    SnapItemToPerfect(needSnapIndex);
                }
            }
            else //如果最近的不是当前吸附的，那就吸附最近的就行了
            {
                SnapItemToPerfect(closetItemIndex);
            }
        }
    }


    //====================算出所有 item 哪个离 perfect近，并吸过去=======================
    private Dictionary<int, float> _itemToPerfectOffsetXDict = new Dictionary<int, float>(); //key是index，value是item到perfect的偏移量

    private void SnapItemToPerfect(int index, bool useTween = true)
    {
        //这个方法是主要是负责吸附到指定index的item
        var contentAnchoredPos = contentRect.anchoredPosition;
        contentAnchoredPos.x += _itemToPerfectOffsetXDict[index];
        if (useTween)
        {
            _tween = contentRect.DOAnchorPos(contentAnchoredPos, 0.2f).SetEase(Ease.OutCubic);
        }
        else
        {
            contentRect.anchoredPosition = contentAnchoredPos;
        }

        _currentSnapedIndex = index;
    }

    private int CalculateClosestItemIndex()
    {
        //这个方法主要是算出离perfect最近的Item的索引号

        float minItemToPerfectDistance = 999999; //遍历完得到的这个最小 item 离 perfect 的距离，再给到 content，就能完成吸附了
        int closedItemIndex = -1; //本次拖拽的，算出来离 perfect 最近的 item的索引号

        if (itemList.Count == 0)
        {
            Debug.Log("没有找到item");
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            var itemRect = itemList[i];
            var itemCenter = GetCenterPointInParent(itemRect, _parentRect); //获得中心
            var itemToPerfectOffsetX = _perfectCenter.x - itemCenter.x; //偏移量
            var itemToPerfectDistance = Mathf.Abs(itemToPerfectOffsetX); //距离
            _itemToPerfectOffsetXDict.Add(i, itemToPerfectOffsetX); //每一次算的item的偏移量都要记录到字典
            if (itemToPerfectDistance < minItemToPerfectDistance)
            {
                minItemToPerfectDistance = itemToPerfectDistance;
                closedItemIndex = i;
            }
        }

        return closedItemIndex;
    }

    private Vector2 GetCenterPointInParent(RectTransform childRect, RectTransform parentRect)
    {
        //获得某个子 rect 在父 rect 下的中心点在哪
        //拿 0 和 1 的角坐标的y 相加/2就是中心点的 y
        //拿 1 和 2 的角坐标的 x 相加/2就是中心点的 x


        Vector3[] corners = new Vector3[4];
        childRect.GetWorldCorners(corners); //这只是是世界坐标，还要转换到父坐标系下
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = parentRect.InverseTransformPoint(corners[i]);
        }

        var centerPoint = Vector2.zero;
        centerPoint.x = (corners[1].x + corners[2].x) / 2;
        centerPoint.y = (corners[0].y + corners[1].y) / 2;
        return centerPoint;
    }


    //====================拖动速度相关逻辑=======================
    private float _averageDragSpeedX = 0;
    private float _totalDragSpeedX = 0;
    private float _dragSpeedPerInvoke = 0;
    private int _invokeCount = 0;


    private void CalculateTotalDragSpeedXOnDraging(PointerEventData data)
    {
        _dragSpeedPerInvoke = data.delta.x / Time.unscaledDeltaTime;
        _totalDragSpeedX += _dragSpeedPerInvoke;
        _invokeCount++;
    }

    private void CalculateAverageDragSpeedXOnEndDrag()
    {
        _averageDragSpeedX = _totalDragSpeedX / _invokeCount;
        _totalDragSpeedX = 0;
        _dragSpeedPerInvoke = 0;
        _invokeCount = 0;
    }

    // private void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle();
    //     style.fontSize = 40;
    //     GUI.Label(new Rect(10, 10, 100, 100), "_averageDragSpeedX:   " + _averageDragSpeedX, style);
    //     GUI.Label(new Rect(10, 110, 100, 100), "_totalDragSpeedX:   " + _totalDragSpeedX, style);
    //     GUI.Label(new Rect(10, 210, 100, 100), "_dragSpeedPerInvoke:   " + _dragSpeedPerInvoke, style);
    //     GUI.Label(new Rect(10, 310, 100, 100), "_invokeCount:   " + _invokeCount, style);
    // }


    //==================== 根据离Perfect点控制 item大小 =======================
    /*
     * 首先要得到场上的全部item的中心点
     * 然后还要得到perfect的中心点
     * 然后遍历全部的item：
     *  -因为距离越小，想越大，所以需要一个映射关系，比如weight，当距离大于阈值的时候，weight是0，当距离小于阈值的时候，weight从0变化到1
     *   ## weight = 1 - (距离/影响距离),这个关系式是核心，当距离超过影响距离的时候，weight就会变为0，当距离为0时，权重最大
     *  得到权重之后，柔化一下权重，weight = SmoothStep(0,1,weight)这行代码的语义就是说：在0到1这个变化过程中，我传进去了一个归一化的线性变化进度weight，给我一个这个线性变化进度的柔化版本weight，也是代表从0到1的进度点，但这个进度点是柔性变化点
     *  算出根据权重进度得到的目标缩放进度
     *  然后把这个目标缩放进度给到item的缩放
     *  以上说的所有计算都是在父空间完成的
     */
    [Header("ScaleController")] public float scaleAffectedArea = 300f;

    public float maxScaleFactor = 1.2f; //最大缩放系数
    public float normalScaleFactor = 1f; //普通缩放系数
    public float scaleSpeed = 10;

    private void ControlSelectedAreaItemSize()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            var itemRect = itemList[i];
            var itemCenter = GetCenterPointInParent(itemRect, _parentRect); //获得中心
            var itemToPerfectOffsetX = _perfectCenter.x - itemCenter.x; //偏移量
            var itemToPerfectDistance = Mathf.Abs(itemToPerfectOffsetX); //距离

            //将距离关系抽象成一个进度，之后拿这个进度去描述从normalScale变化成maxScale就行了
            var progress = 1 - (itemToPerfectDistance / scaleAffectedArea);

            //归一化进度
            progress = Mathf.Clamp01(progress);

            //将线性进度，柔化成柔性进度。并用来描述从normalScale到maxScale 缩放系数的变化进度,得到目标缩放系数
            var targetScaleFactor = Mathf.SmoothStep(normalScaleFactor, maxScaleFactor, progress);

            //从当前的缩放，插值到目标的缩放。
            //Lerp的第三个参数的含义是：从 current 到 target 这段“当前剩余距离”里，每一帧走百分之多少？走多少，结果可以抽象成路程，这个路程是不能超过1的，时间差不多每帧是0.016，换算一下速度是大概不能超过60的，把速度抛给外部处理吧
            //也就是说，这个参数要外部定，
            var currentScale = itemRect.localScale;
            currentScale = Vector3.Lerp(currentScale, targetScaleFactor * Vector3.one, Time.deltaTime * scaleSpeed);

            //最后对item的Scale赋值
            itemRect.localScale = currentScale;
        }
    }

    private void Update()
    {
        ControlSelectedAreaItemSize();
    }
}