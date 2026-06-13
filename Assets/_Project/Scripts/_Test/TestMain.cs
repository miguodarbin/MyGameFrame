using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    private void Start()
    {
        XResourcesManager.Instance.LoadAsset<GameObject>("Test");
        XPoolManager.Instance.Clear();
        
    }

    public void OnPrefabLoaded1(GameObject go)
    {
        Instantiate(go);
    }

    public void OnPrefabLoaded2(GameObject go)
    {
        Instantiate(go);
    }


}