using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


/// <summary>
/// 不继承MonoBehaviour的单例模式基类
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class SingletonData<T> where T : SingletonData<T>
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                Type type = typeof(T);
                var typeConstructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
                if (typeConstructor == null)
                {
                    Debug.Log("Could not find constructor for " + typeof(T).Name);
                    return null;
                }

                _instance = (T)typeConstructor.Invoke(null);
            }

            return _instance;
        }
    }
}