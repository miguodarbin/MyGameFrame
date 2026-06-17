using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Example_MVC_HomePanelView : XUIPanelView
{
    public Button CountPanelButton => GetUIControl<Button>("CountButton");
}
