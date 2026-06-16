using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AB资源统一加载入口。
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c>LoadAsset&lt;T&gt;(packageName, resName, callback, isSync)</c>：统一加载AB相关资源</description>
/// </item>
/// <item>
/// <description>Load 可以无脑调用</description>
/// </item>
/// /// <item>
/// <description>要卸载AB包，需要调用 XABManager</description>
/// </item>
/// <item>
/// <description>资源摆放规则：Assets/Editor/ArtRes/packageName/resName，文件夹名对应AB包名</description>
/// </item>
/// </list>
/// </remarks>
public class XABUnifiedManager : XSingletonCSharp<XABUnifiedManager>
{
    private XABUnifiedManager()
    {
    }

    //true的话，在编辑器就用同步加载
    //false的话，在编辑器就用AB异步加载
    //无论是true还是false，运行时都是AB异步加载
    public static bool UseEditorAsset = false;

    public void LoadAsset<T>(string packageName, string resName, UnityAction<T> callback, bool isSync = false) where T : Object
    {
#if UNITY_EDITOR
        if (UseEditorAsset)
        {
            var result = XEditorAssetManager.Instance.LoadEditorAsset<T>(packageName + "/" + resName);
            callback?.Invoke(result);
        }
        else
        {
            XABManager.Instance.LoadAbAsset<T>(packageName, resName, callback, isSync);
        }
#else
        XABManager.Instance.LoadAbAsset<T>(packageName, resName, callback, isSync);
#endif
    }
}