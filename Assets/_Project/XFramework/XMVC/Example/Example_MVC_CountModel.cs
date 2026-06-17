using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public class Example_MVC_CountData
{
    public int countSnap = 0; //只是快照
}

public class Example_MVC_CountModel
{
    private const string _jsonName = "Example_MVC_Count.json";
    public UnityAction<Example_MVC_CountModel> onCountChanged;

    //CountPanel 管理的数据：
    private int _count = 0;

    public int Count
    {
        get { return _count; }
        set { _count = value; }
    }


    //初始化Model数据
    public Example_MVC_CountModel()
    {
        Init();
    }

    protected void Init()
    {
        //先判断有没有Json，有Json的话用Json的数据，
        if (File.Exists(Application.persistentDataPath + "/" + _jsonName))
        {
            var obj = JsonManager.Instance.JsonToObj<Example_MVC_CountData>(_jsonName);
            if (obj == null)
            {
                Debug.Log("反序列失败");
                return;
            }

            this.Count = obj.countSnap;
        }
        else //没Json的话就用0，并创建一个Json
        {
            SaveCount();
        }
    }


    //处理数据的规则
    public void AddCount()
    {
        Count++;
        SaveCount();
        onCountChanged?.Invoke(this);
    }

    public void SubCount()
    {
        Count--;
        SaveCount();
        onCountChanged?.Invoke(this);
    }

    public void ResetCount()
    {
        Count = 0;
        SaveCount();
        onCountChanged?.Invoke(this);
    }

    public void SaveCount()
    {
        var data = new Example_MVC_CountData();
        data.countSnap = this.Count;
        JsonManager.Instance.ObjToJson(data, _jsonName);
    }
}