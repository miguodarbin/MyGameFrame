using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Example_MVC : MonoBehaviour
{
    
    void Start()
    {
        XUIManager.Instance.ShowPanel<Example_MVC_HomePanelView>(XCustomUILayer.E_Top);
    }

}