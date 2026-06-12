using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    private void Start()
    {
        XResourcesManager.Instance.LoadAssetAsync<GameObject>("Capsule", (asset) =>
        {
            var go = Instantiate(asset);
            go.transform.position = Vector3.right;
        });
    }
}