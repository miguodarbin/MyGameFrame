using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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


    public void RefreshUI(Example_MVC_CountModel model)
    {
        CountPanelView.text = model.Count.ToString();
    }
}