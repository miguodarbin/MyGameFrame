using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public Transform content;
    public RectTransform perfectPoint;
    public List<RectTransform> itemList = new List<RectTransform>();

    private void Awake()
    {
        //初始化的时候就添加自定义事件组件
        AddDragEvent();
    }

    private void Start()
    {
        //开始的时候再获得组件，担心没ui加载完
        GetAllItems();
    }


    //先获得所有的Items
    private void GetAllItems()
    {
        foreach (Transform child in content.transform)
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

    private void OnDrag(BaseEventData eventData)
    {
    }

    private void OnEndDrag(BaseEventData eventData)
    {
        int closetIndex = -1;
        float closetDistance = 9999999;
        Vector3 toPerfectOffset = Vector3.zero;

        var scrollRect = transform as RectTransform;
        var contentRect =  content as RectTransform;
        for (int i = 0; i < itemList.Count; i++)
        {
            var itemPosInScrollRect = scrollRect.InverseTransformPoint(itemList[i].position);
            var perfectPosInScrollRect = scrollRect.InverseTransformPoint(perfectPoint.position);
            var distance = Vector2.Distance(itemPosInScrollRect, perfectPosInScrollRect);
            if (distance < closetDistance)
            {
                closetIndex = i;
                closetDistance = distance;
                toPerfectOffset = perfectPoint.position - itemList[i].position;
            }
        }

        var contentAnchoredPos = contentRect.anchoredPosition;
        contentAnchoredPos.x += toPerfectOffset.x;
        contentRect.DOAnchorPos(contentAnchoredPos, 0.5f);
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
}