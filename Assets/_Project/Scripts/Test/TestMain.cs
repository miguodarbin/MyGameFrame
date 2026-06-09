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
            var go = PoolManager.Instance.GetObj("Cube");
            StartCoroutine(ReturnObject("Cube", go, 1f));
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            var go = PoolManager.Instance.GetObj("Sphere");
            StartCoroutine(ReturnObject("Sphere", go, 1f));
        }
    }

    private IEnumerator ReturnObject(string poolName, GameObject go, float time)
    {
        yield return new WaitForSeconds(time);
        PoolManager.Instance.ReturnObj(go);
    }
}