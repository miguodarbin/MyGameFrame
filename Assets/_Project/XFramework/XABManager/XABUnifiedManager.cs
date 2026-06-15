using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AB资源统一加载入口。
/// 在编辑器下可以选择从 Editor/ArtRes 直接加载，或从 AB 包加载；
/// 发布后始终从 AB 包加载。
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c>LoadAsset&lt;T&gt;(packageName, resName, callback, isSync)</c>：统一加载AB相关资源。</description>
/// </item>
/// <item>
/// <description><c>UseEditorAsset = true</c>：编辑器下从 Assets/Editor/ArtRes/packageName 加载。</description>
/// </item>
/// <item>
/// <description><c>UseEditorAsset = false</c>：编辑器下也走 AB 包加载，从 Assets/StreamingAssets 加载。</description>
/// </item>
/// <item>
/// <description>发布后不管开关是什么，都会走 AB 包加载。</description>
/// </item>
/// <item>
/// <description>资源摆放规则：Assets/Editor/ArtRes/packageName/resName，文件夹名对应AB包名。</description>
/// </item>
/// </list>
/// </remarks>
public class XABUnifiedManager : XSingletonCSharp<XABUnifiedManager>
{
    private XABUnifiedManager()
    {
    }

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