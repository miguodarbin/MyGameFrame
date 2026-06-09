using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject : SingletonData<TestObject>
{
    private Coroutine _coroutine;

    private TestObject()
    {
    }

    private void MyUpdate()
    {
        Debug.Log("MyUpdate");
    }

    public void StartUpdate()
    {
        MonoManager.Instance.OnUpdateAddListener(MyUpdate);
    }

    public void StopUpdate()
    {
        MonoManager.Instance.OnUpdateRemoveListener(MyUpdate);
    }

    public void StartCoroutine()
    {
        _coroutine = MonoManager.Instance.StartCoroutine(MyCoroutine());
    }

    public void StopCoroutine()
    {
        MonoManager.Instance.StopCoroutine(_coroutine);
    }

    private IEnumerator MyCoroutine()
    {
        Debug.Log("MyCoroutine");
        yield return new WaitForSeconds(3);
        Debug.Log("MyCoroutine 3S");
    }
}