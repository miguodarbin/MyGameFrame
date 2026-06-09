using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 不继承MonoBehaviour的单例模式基类，子类自己私有化构造函数
/// </summary>
/// <typeparam name="T">单例Instance类型</typeparam>
public abstract class SingletonCSharp<T> where T : SingletonCSharp<T>
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
                    Debug.LogError("无法找到非公开的构造函数：" + typeof(T).Name);
                    return null;
                }

                _instance = (T)typeConstructor.Invoke(null);
            }

            return _instance;
        }
    }
}