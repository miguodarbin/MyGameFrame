using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//主要用来写显示逻辑
public class Example_MVC_CountPanelView : XUIPanelView
{
    //声明控件，并把需要有交互监听的控件暴露给外面，让外面的Controller来监听
    private TextMeshProUGUI CountPanelView => GetUIControl<TextMeshProUGUI>("CountNumber");
    public Button AddButton => GetUIControl<Button>("AddButton");
    public Button SubButton => GetUIControl<Button>("SubButton");
    public Button ResetButton => GetUIControl<Button>("ResetButton");
    public Button CloseButton => GetUIControl<Button>("CloseButton");
    private Image DragArea => GetUIControl<Image>("DragArea");
    public Image Window => GetUIControl<Image>("WindowBG");
    public Image Mask => GetUIControl<Image>("Mask");
    public Image Root => GetUIControl<Image>("Example_MVC_CountPanelView");


    //自定义交互事件
    public EventTrigger.Entry OnDraging;
    public EventTrigger.Entry OnEndDrag;

    //初始化View的时候调用
    protected override void InitPanelView()
    {
        //为DragArea添加自定义拖动事件
        var eventTrigger = DragArea.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = DragArea.gameObject.AddComponent<EventTrigger>();
        }

        OnDraging = new EventTrigger.Entry();
        OnDraging.eventID = EventTriggerType.Drag;

        OnEndDrag = new EventTrigger.Entry();
        OnEndDrag.eventID = EventTriggerType.EndDrag;
        eventTrigger.triggers.Add(OnDraging);
        eventTrigger.triggers.Add(OnEndDrag);
    }

    public void RefreshUI(Example_MVC_CountModel model)
    {
        CountPanelView.text = model.Count.ToString();
    }
}