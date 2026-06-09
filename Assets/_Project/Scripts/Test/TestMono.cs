using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMono : MonoBehaviour
{
    private void MyUpdate()
    {
        Debug.Log("XXX");
    }

    void Start()
    {
        MonoManager.Instance.OnUpdateAddListener(MyUpdate);
    }
}
