using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 不继承MonoBehaviour的单例模式基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonData<T> where T : SingletonData<T>, new()
{
    private static T _instance;

    //似乎对子类的构造函数进行private修饰是无解的？？

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new T();
            }

            return _instance;
        }
    }

    // public static T GetInstance()
    // {
    //     if (_instance == null)
    //     {
    //         _instance = new T();
    //     }
    //
    //     return _instance;
    // }
}