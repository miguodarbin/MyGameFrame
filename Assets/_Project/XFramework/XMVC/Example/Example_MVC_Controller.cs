using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example_MVC_Controller : XUIController<Example_MVC_CountPanelView,Example_MVC_CountModel>
{
    private void Start()
    {
        PanelModel.Test();
    }
}
