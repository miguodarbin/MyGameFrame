using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestObject.Instance.StartCoroutine();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            TestObject.Instance.StopCoroutine();
        }
    }
}