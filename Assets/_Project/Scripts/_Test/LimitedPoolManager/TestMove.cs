using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMove : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        transform.position = Vector3.zero;
        Invoke(nameof(ReturnThis), 100);
    }

    void OnDisable()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * (Time.deltaTime * 10));
    }

    public void ReturnThis()
    {
        XPoolManager.Instance.ReturnGameObject(gameObject);
    }
}