using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMain : MonoBehaviour
{
    private void Start()
    {
        XUIManager.Instance.ShowPanel<GamePanel>(XCustomUILayer.E_Top);
    }

    private void Update()
    {
    }
}