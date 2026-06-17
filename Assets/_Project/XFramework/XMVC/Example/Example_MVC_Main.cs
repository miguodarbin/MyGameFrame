using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Example_MVC : MonoBehaviour
{
    
    void Start()
    {
        XUIManager.Instance.ShowPanel<Example_MVC_CountPanelView>(XCustomUILayer.E_Top);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            XUIManager.Instance.ShowPanel<Example_MVC_CountPanelView>(XCustomUILayer.E_Top);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            XUIManager.Instance.HidePanel<Example_MVC_CountPanelView>();
        }
    }
}