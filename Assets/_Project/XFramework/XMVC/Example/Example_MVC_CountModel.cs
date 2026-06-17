using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Example_MVC_CountModel : XUIModel
{
    private const string _jsonName = "Example_MVC_Count.json";

    public Example_MVC_CountModel()
    {
        OnInit();
    }


    //CountPanel所 管理的数据：
    private int _count;

    protected override void OnInit()
    {
        //先判断有没有Json，有Json的话用Json的数据，没Json的话就用0，并创建一个Json
        if (File.Exists(Application.persistentDataPath + "/" + _jsonName))
        {
            JsonManager.Instance.JsonToObj<Example_MVC_CountModel>(_jsonName);
        }
        else
        {
            _count = 0;
            JsonManager.Instance.ObjToJson(this, _jsonName);
        }
    }

    public void Test()
    {
        Debug.Log("Test");
    }

    //处理数据的规则
    private void AddCount()
    {
    }
}