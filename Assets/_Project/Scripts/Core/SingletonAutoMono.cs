using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
/// <summary>
/// 实现自动访问属性自动生成对象并挂载单例组件的类
/// </summary>
/// <typeparam name="T"></typeparam>

public class SingletonAutoMono<T> : MonoBehaviour where T : SingletonAutoMono<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject haveInstanceObj = new GameObject();
                haveInstanceObj.AddComponent<T>();
                haveInstanceObj.name = typeof(T).Name;
            }

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
    }

    protected virtual void OnDestroy()
    {
        if (this as T != _instance)
        {
            return;
        }

        _instance = null;
    }
}