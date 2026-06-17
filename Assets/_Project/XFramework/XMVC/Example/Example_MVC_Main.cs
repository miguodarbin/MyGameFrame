using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example_MVC : MonoBehaviour
{
    void Start()
    {
        XUIManager.Instance.ShowPanel<Example_MVC_CountPanelView>(XCustomUILayer.E_Top);
    }

}