using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GamePanel : XUIPanel
{
    private void Start()
    {
        var eventTrigger = GetUIControl<Button>("FireButton").AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener(OnPointerEnter);
        eventTrigger.triggers.Add(entry);
    }

    public void OnPointerEnter(BaseEventData eventData)
    {
        var data = (PointerEventData)eventData;
        Debug.Log(data.position);
    }
}