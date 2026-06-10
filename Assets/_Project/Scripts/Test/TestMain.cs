using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var go = PoolManager.Instance.GetObj("Cube",3);
            go.transform.position = Vector3.zero;
            
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            var go = PoolManager.Instance.GetObj("Sphere",3);
            
        }
    }
    
}