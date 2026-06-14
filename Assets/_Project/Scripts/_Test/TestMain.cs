using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    private void Start()
    {
        XABManager.Instance.GetAssetBundleResAsync<GameObject>("test", "cube", (o => { Instantiate(o); }));
        XABManager.Instance.GetAssetBundleResAsync<GameObject>("test", "cube", (o => { Instantiate(o); }));
    }
}