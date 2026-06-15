using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMain : MonoBehaviour
{
    public Image image;

    private void Start()
    {
        XABUnifiedManager.Instance.LoadAsset<Sprite>("ui", "100", (arg => image.sprite = arg));
    }
}