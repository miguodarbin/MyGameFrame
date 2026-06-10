using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(ReturnThis), 1);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * (Time.deltaTime * 10));
    }

    public void ReturnThis()
    {
        PoolManager.Instance.ReturnObj(this.gameObject);
    }
}