using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

/*

0. 这个是挂在ScrollRect对象下的
1. 有一个固定点：PerfectPoint
2. 每个 Item 有自己的 pivot 点
3. 松手后，遍历所有 Item
4. 找到 pivot 点距离 PerfectPoint 最近的 Item
5. 计算这个 Item 的 pivot 点到 PerfectPoint 的偏移量
6. 把 Content 整体移动这段偏移量

 */
public class TestBanner : MonoBehaviour
{
    public RectTransform contentRect; //content 的 RectTransform
    public RectTransform perfectRect; //perfect 的 RectTransform
    private RectTransform _parentRect; //content 和 perfect的 parent 的 RectTransform
    public List<RectTransform> itemList = new List<RectTransform>(); //识别到的 item
    public int snapItemIndex = -1; //本次拖拽的，算出来要吸附到 perfect 的 item的索引号

    private Tween _tween; //拿到 tween 对象，在失活的时候kill 掉

    //初始化的时候就添加自定义事件组件
    private void Awake()
    {
        _parentRect = transform as RectTransform;
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
            if (child.name.Contains("item"))
            {
                var childRect = child as RectTransform;
                itemList.Add(childRect);
            }
        }
    }


    //添加自定义拖动事件
    private EventTrigger.Entry onDragEntry;
    private EventTrigger.Entry onEndDragEntry;

    private void AddDragEvent()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        onDragEntry = new EventTrigger.Entry();
        onDragEntry.eventID = EventTriggerType.Drag;
        eventTrigger.triggers.Add(onDragEntry);

        onEndDragEntry = new EventTrigger.Entry();
        onEndDragEntry.eventID = EventTriggerType.EndDrag;
        eventTrigger.triggers.Add(onEndDragEntry);
    }

    private void OnEnable()
    {
        onDragEntry.callback.AddListener(OnDrag);
        onEndDragEntry.callback.AddListener(OnEndDrag);
    }

    private void OnDisable()
    {
        onDragEntry.callback.RemoveListener(OnDrag);
        onEndDragEntry.callback.RemoveListener(OnEndDrag);
    }


    //====================开始拖拽的逻辑=======================
    private void OnDrag(BaseEventData eventData)
    {
        //处理 content 的拖动
        var data = eventData as PointerEventData;
        var contentAnchoredPosX = contentRect.anchoredPosition.x;
        var targetAnchoredPosX = contentAnchoredPosX + data.delta.x;
        contentRect.anchoredPosition = new Vector2(targetAnchoredPosX, contentRect.anchoredPosition.y);
    }


    //====================结束拖拽的逻辑=======================
    private void OnEndDrag(BaseEventData eventData)
    {
        //2.然后还要判断拖拽速度，维护一个当前吸附的索引号，所以说一开始就要先吸附一次
        //2.1 - 如果速度小于 3000，snapIndex 就是最近的那一个
        //2.2 - 如果速度大于了 3000，最近的那个还是当前吸附的，那就根据方向把当前吸附的索引号+1或-1，去吸下一张
        //2.3 - 如果速度大于了 3000，最近的那个不是当前吸附的，那就吸最近的

        SnapClosestItemToPerfect();
    }


    private int SnapClosestItemToPerfect()
    {
        //1.实现item吸附最近的一个 perfect 点，拖拽结束后，遍历每个 item，看看谁离 perfect 近，
        //算出这个 item 离 perfect 的距离，给到 content，注意要算的是在父坐标系下的中心点

        var perfectCenter = GetCenterPointInParent(perfectRect, _parentRect);
        float minItemToPerfectDistance = 999999; //遍历完得到的这个最小 item 离 perfect 的距离，再给到 content，就能完成吸附了
        float minItemToPerfectOffsetX = 0; //刚才只是算的距离不是偏移，重新算一下带方向的偏移

        for (int i = 0; i < itemList.Count; i++)
        {
            var itemRect = itemList[i];
            var itemCenter = GetCenterPointInParent(itemRect, _parentRect); //获得中心
            var itemToPerfectOffsetX = perfectCenter.x - itemCenter.x; //偏移量
            var itemToPerfectDistance = Mathf.Abs(itemToPerfectOffsetX); //距离
            if (itemToPerfectDistance < minItemToPerfectDistance)
            {
                minItemToPerfectDistance = itemToPerfectDistance;
                minItemToPerfectOffsetX = itemToPerfectOffsetX;
                snapItemIndex = i;
            }
        }

        Debug.Log(snapItemIndex);
        //算出目标 content 位置，重新校正 content
        var contentAnchoredPos = contentRect.anchoredPosition;
        contentAnchoredPos.x += minItemToPerfectOffsetX;
        contentRect.DOAnchorPos(contentAnchoredPos, 0.5f).SetEase(Ease.OutBack);

        return snapItemIndex;
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
}