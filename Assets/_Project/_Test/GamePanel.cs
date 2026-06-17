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
        var button = GetUIControl<Button>("FireButton");
        XUIManager.AddCustomEventTrigger(button, EventTriggerType.PointerEnter, OnPointerEnter);
    }

    public void OnPointerEnter(BaseEventData eventData)
    {
        var data = (PointerEventData)eventData;
        Debug.Log("鼠标进入");
    }
}