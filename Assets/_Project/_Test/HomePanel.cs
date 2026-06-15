using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomePanel : XUIPanel
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var item in _uiDict)
        {
            Debug.Log(item.Key);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}