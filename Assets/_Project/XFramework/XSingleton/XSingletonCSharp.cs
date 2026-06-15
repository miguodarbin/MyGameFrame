using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 给 Csharp对象 实现单例
/// </summary>
/// <list type="number">
/// <item>
/// <description>外部子类如果重写 Awake / OnDestroy，要记得调用 base.Awake() / base.OnDestroy()</description>
/// </item>
/// </list>
public abstract class XSingletonCSharp<T> where T : XSingletonCSharp<T>
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