using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

public class XAssetLoadAsyncInfoBase
{
    public int refCount = 0;

    public void AddRefCount()
    {
        ++refCount;
    }

    public void SubRefCount()
    {
        --refCount;

        if (refCount < 0)
        {
            Debug.LogError("引用计数小于0：Load 和 Unload 没有配对");
            refCount = 0;
        }
    }
}

public class XAssetLoadAsyncAsyncInfo<T> : XAssetLoadAsyncInfoBase
{
    public T asset;
    public UnityAction<T> totalCallback;
    public Coroutine coroutineInfo;
    public bool unloadNow; //如果这次卸载的资源是最后一个引用计数，那是否需要立马就卸载掉这个资源
}


/// <summary>
/// Resources 资源加载管理器
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c>LoadAsset&lt;T&gt;(assetName)</c>：同步加载资源 </description>
/// </item>
/// <item>
/// <description><c>LoadAssetAsync&lt;T&gt;(assetName, callback)</c>：异步加载资源 </description>
/// </item>
/// <item>
/// <description><c>UnloadAsset&lt;T&gt;(assetName, callback = null, unloadNow = true)</c>：取消一次资源使用 </description>
/// </item>
/// <item>
/// <description><c>UnloadUnUsedAssets(callback = null)</c>：清理全部未使用资源 </description>
/// </item>
/// </list>
/// </remarks>
/// <remarks>
/// 外部须知：
/// <list type="number">
///  <item>
/// 外部只负责“用的时候 Load，不用的时候 Unload”，不要关心资源是否已经加载过
/// </item>
/// </list>
/// </remarks>
public class XResourcesManager : XSingletonCSharp<XResourcesManager>
{
    private XResourcesManager()
    {
    }

    //字典，存储了所有异步加载的情况信息，Key是 路径+类型。Value是自定义结构类,包括了资源本体，异步加载完毕要触发的回调，这次异步加载的句柄
    private Dictionary<string, XAssetLoadAsyncInfoBase> _loadInfoDict = new Dictionary<string, XAssetLoadAsyncInfoBase>();


    /// <summary>
    /// 异步加载资源 -泛型
    /// 每调用一次，引用计数 +1
    /// 如果同一个资源正在加载中，不会重复开启加载，而是合并 callback.加载完成后，会回调所有仍然有效的 callback
    /// 不推荐使用Lambda表达式传递Callback，因为卸载需要再传这个回调函数
    /// </summary>
    /// <param name="assetName"> 资源路径 </param>
    /// <param name="callback"> 加载完成触发这个回调 </param>
    /// <typeparam name="T"> 要加载的资源类型 </typeparam>
    public void LoadAssetAsync<T>(string assetName, UnityAction<T> callback) where T : Object
    {
        string fullAssetName = assetName + "." + typeof(T).Name;

        if (_loadInfoDict.ContainsKey(fullAssetName))
        {
            var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<T>;
            //这里都靠名字和类型双保险进来了，_loadInfoDict[fullAssetName]的类型一定不会变,这里就不对loadInfo显示判空了,如果为空了，那就是第两次用的加载方法不一样，一次用的泛型异步加载，一次用的Type异步加载
            loadInfo.AddRefCount();
            if (loadInfo.asset != null)
            {
                callback?.Invoke(loadInfo.asset);
            }
            else
            {
                //Debug.Log("正在加载中，已把这次回调添加到总回调中");
                loadInfo.totalCallback += callback;
            }
        }
        else
        {
            var loadInfo = new XAssetLoadAsyncAsyncInfo<T>();
            loadInfo.AddRefCount();
            _loadInfoDict.Add(fullAssetName, loadInfo);
            loadInfo.totalCallback += callback;
            //这句代码跑到StartCoroutine之后，
            //会先跑一遍ReallyLoadAssetAsync得到迭代器对象，注意第一次执行ReallyLoadAssetAsync的时候只会拿到迭代器对象，不跑方法体里的逻辑
            //然后再去跑StartCoroutine方法，此时CPU会一直运行到第一个yield return
            //然后才会回来继续往下执行loadInfo.coroutineInfo = coroutineInfo;
            //所以千万不要在ReallyLoadAssetAsync第一个yield return之前用coroutineInfo
            var coroutineInfo = XMonoManager.Instance.StartCoroutine(ReallyLoadAssetAsync(assetName, fullAssetName, loadInfo));
            loadInfo.coroutineInfo = coroutineInfo;
        }
    }

    private IEnumerator ReallyLoadAssetAsync<T>(string assetName, string fullAssetName, XAssetLoadAsyncAsyncInfo<T> loadInfo) where T : Object
    {
        var request = Resources.LoadAsync<T>(assetName);
        yield return request;


        if (request.asset != null && request.asset is T asset)
        {
            loadInfo.asset = asset;
            if (loadInfo.refCount == 0 && loadInfo.unloadNow)
            {
                if (loadInfo.asset is GameObject || loadInfo.asset is Component || loadInfo.asset is AssetBundle)
                {
                    // GameObject / Component / AssetBundle 不能用 Resources.UnloadAsset 单独卸,只是让Manager不再持有它，只能等之后调用 Resources.UnloadUnusedAssets的时候实现真正的卸载
                    _loadInfoDict.Remove(fullAssetName);
                }
                else
                {
                    Resources.UnloadAsset(loadInfo.asset);
                    _loadInfoDict.Remove(fullAssetName);
                }

                yield break;
            }

            loadInfo.totalCallback?.Invoke(asset);
            loadInfo.totalCallback = null;
            loadInfo.coroutineInfo = null;
        }
        else
        {
            Debug.LogError("加载失败");
            _loadInfoDict.Remove(fullAssetName);
        }
    }


    //异步加载资源 -Type,传type只是为了来找对应的类型资源，并不是直接返回type类型的asset
    [Obsolete("loadInfo如果为空了，那就是第两次用的加载方法不一样，一次用的泛型异步加载，一次用的Type异步加载")]
    public void LoadAssetAsync(string assetName, Type type, UnityAction<Object> callback)
    {
        string fullAssetName = assetName + "." + type.Name;

        if (_loadInfoDict.ContainsKey(fullAssetName))
        {
            var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<Object>;
            loadInfo.AddRefCount();
            //这里都靠名字和类型双保险进来了，_loadInfoDict[fullAssetName]的类型一定不会变,这里就不对loadInfo显示判空了
            if (loadInfo.asset != null)
            {
                callback?.Invoke(loadInfo.asset);
            }
            else
            {
                //Debug.Log("正在加载中，已把这次回调添加到总回调中");
                loadInfo.totalCallback += callback;
            }
        }
        else
        {
            var loadInfo = new XAssetLoadAsyncAsyncInfo<Object>();
            loadInfo.AddRefCount();
            _loadInfoDict.Add(fullAssetName, loadInfo);
            loadInfo.totalCallback += callback;
            //这句代码跑到StartCoroutine之后，
            //会先跑一遍ReallyLoadAssetAsync得到迭代器对象，注意第一次执行ReallyLoadAssetAsync的时候只会拿到迭代器对象，不跑方法体里的逻辑
            //然后再去跑StartCoroutine方法，此时CPU会一直运行到第一个yield return
            //然后才会回来继续往下执行loadInfo.coroutineInfo = coroutineInfo;
            //所以千万不要在ReallyLoadAssetAsync第一个yield return之前用coroutineInfo
            var coroutineInfo = XMonoManager.Instance.StartCoroutine(ReallyLoadAssetAsync(assetName, type, fullAssetName, loadInfo));
            loadInfo.coroutineInfo = coroutineInfo;
        }
    }

    private IEnumerator ReallyLoadAssetAsync(string assetName, Type type, string fullAssetName, XAssetLoadAsyncAsyncInfo<Object> loadInfo)
    {
        var request = Resources.LoadAsync(assetName, type);
        yield return request;
        if (request.asset != null)
        {
            loadInfo.asset = request.asset;
            if (loadInfo.refCount == 0 && loadInfo.unloadNow)
            {
                if (loadInfo.asset is GameObject || loadInfo.asset is Component || loadInfo.asset is AssetBundle)
                {
                    // GameObject / Component / AssetBundle 不能用 Resources.UnloadAsset 单独卸,只是让Manager不再持有它，只能等之后调用 Resources.UnloadUnusedAssets的时候实现真正的卸载
                    _loadInfoDict.Remove(fullAssetName);
                }
                else
                {
                    Resources.UnloadAsset(loadInfo.asset);
                    _loadInfoDict.Remove(fullAssetName);
                }

                yield break;
            }

            loadInfo.totalCallback?.Invoke(loadInfo.asset);
            loadInfo.totalCallback = null;
            loadInfo.coroutineInfo = null;
        }
        else
        {
            Debug.LogError("加载失败");
            _loadInfoDict.Remove(fullAssetName);
        }
    }


    /// <summary>
    /// 同步加载资源 -泛型
    /// 每调用一次，引用计数 +1
    /// 如果资源已经加载过，直接返回缓存资源
    /// 如果资源正在异步加载中，会用同步加载接管，并触发之前等待的异步回调
    /// </summary>
    /// <param name="assetName"> 资源路径 </param>
    /// <typeparam name="T"> 想要加载的资源类型 </typeparam>
    /// <returns>资源</returns>
    public T LoadAsset<T>(string assetName) where T : Object
    {
        string fullAssetName = assetName + "." + typeof(T).Name;
        if (_loadInfoDict.ContainsKey(fullAssetName)) //先看看有没有加载过
        {
            //加载的状态如何，如果加载完了，那就直接用，没加载完那就取消协程，但不取消Unity加载，让同步方法这里拿到资源对象,并代替执行之前协程的回调函数
            var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<T>;
            loadInfo.AddRefCount();
            if (loadInfo.asset != null)
            {
                return loadInfo.asset;
            }

            var asset = Resources.Load<T>(assetName);
            if (asset == null)
            {
                Debug.LogError("加载失败");
                _loadInfoDict.Remove(fullAssetName);
                return null;
            }

            XMonoManager.Instance.StopCoroutine(loadInfo.coroutineInfo);
            loadInfo.coroutineInfo = null;
            loadInfo.asset = asset;
            loadInfo.totalCallback?.Invoke(asset);
            loadInfo.totalCallback = null;
            return asset;
        }
        else
        {
            var asset = Resources.Load<T>(assetName);
            if (asset == null)
            {
                Debug.LogError("加载失败");
                _loadInfoDict.Remove(fullAssetName);
                return null;
            }

            var loadInfo = new XAssetLoadAsyncAsyncInfo<T>();
            loadInfo.AddRefCount();
            _loadInfoDict.Add(fullAssetName, loadInfo);
            loadInfo.asset = asset;
            return asset;
        }
    }


    /// <summary>
    /// 同步卸载指定资源-泛型
    /// 每调用一次，引用计数 -1
    /// 这个方法不一定会立刻卸载资源，只有 refCount == 0 且 unloadNow == true 时才会尝试卸载
    /// </summary>
    /// <param name="assetName"></param>
    /// <param name="callback">你想卸载的这个资源，当时加载的时候用的哪个回调，传给这个参数，用来断开这个回调等待加载完成。同步加载的资源不用传这个参数</param>
    /// <param name="unloadNow">引用计数归零后是否立刻移除，如果卸载之后，之后可能还会加载，选False</param>
    /// <typeparam name="T">想要卸载的那个资源类型</typeparam>
    public void UnloadAsset<T>(string assetName, UnityAction<T> callback = null, bool unloadNow = true) where T : Object
    {
        string fullAssetName = assetName + "." + typeof(T).Name;
        if (!_loadInfoDict.ContainsKey(fullAssetName))
        {
            return;
        }

        var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<T>;
        loadInfo.unloadNow = unloadNow;
        loadInfo.SubRefCount();


        //资源没加载完，引用计数等于零或者大于零的情况是一致的
        //资源加载完了，引用计数等于零和大于零是要讨论了
        if (loadInfo.asset == null)
        {
            loadInfo.totalCallback -= callback;
        }
        else
        {
            if (loadInfo.refCount == 0 && loadInfo.unloadNow)
            {
                if (loadInfo.asset is GameObject || loadInfo.asset is Component || loadInfo.asset is AssetBundle)
                {
                    // GameObject / Component / AssetBundle 不能用 Resources.UnloadAsset 单独卸,只是让Manager不再持有它，只能等之后调用 Resources.UnloadUnusedAssets的时候实现真正的卸载
                    _loadInfoDict.Remove(fullAssetName);
                }
                else
                {
                    Resources.UnloadAsset(loadInfo.asset);
                    _loadInfoDict.Remove(fullAssetName);
                }
            }
        }
    }

    //同步卸载指定资源-Type
    public void UnloadAsset(string assetName, Type type, UnityAction<Object> callback = null, bool unloadNow = true)
    {
        string fullAssetName = assetName + "." + type.Name;
        if (!_loadInfoDict.ContainsKey(fullAssetName))
        {
            return;
        }

        var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<Object>;
        loadInfo.SubRefCount();
        loadInfo.unloadNow = unloadNow;
        if (loadInfo.asset == null)
        {
            loadInfo.totalCallback -= callback;
        }
        else
        {
            if (loadInfo.refCount == 0 && loadInfo.unloadNow)
            {
                if (loadInfo.asset is GameObject || loadInfo.asset is Component || loadInfo.asset is AssetBundle)
                {
                    // GameObject / Component / AssetBundle 不能用 Resources.UnloadAsset 单独卸,只是让Manager不再持有它，只能等之后调用 Resources.UnloadUnusedAssets的时候实现真正的卸载
                    _loadInfoDict.Remove(fullAssetName);
                }
                else
                {
                    Resources.UnloadAsset(loadInfo.asset);
                    _loadInfoDict.Remove(fullAssetName);
                }
            }
        }
    }

    public int ShowRefCount<T>(string assetName) where T : Object
    {
        string fullAssetName = assetName + "." + typeof(T).Name;
        if (!_loadInfoDict.ContainsKey(fullAssetName))
        {
            return 0;
        }
        else
        {
            return _loadInfoDict[fullAssetName].refCount;
        }
    }


    /// <summary>
    /// 异步卸载当前没有使用的 Resources 资源
    /// 过场景、切换大模块、关闭大型界面、需要主动清理内存时。
    /// </summary>
    /// <param name="callback">清理完成调用的回调函数</param>
    public void UnloadUnUsedAssets(UnityAction callback = null)
    {
        if (_loadInfoDict == null)
        {
            return;
        }

        ClearZeroRefCountInfo();

        XMonoManager.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callback));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callback)
    {
        var request = Resources.UnloadUnusedAssets();
        yield return request;
        callback?.Invoke();
    }

    //将0引用计数的asset从字典中移除，一定要配合UnloadUnUsedAssets() 使用才完整
    private void ClearZeroRefCountInfo()
    {
        List<string> zeroRefLoadInfoKeys = new List<string>();
        foreach (var loadInfoPair in _loadInfoDict)
        {
            if (loadInfoPair.Value.refCount == 0)
            {
                zeroRefLoadInfoKeys.Add(loadInfoPair.Key);
            }
        }

        foreach (var loadInfoKey in zeroRefLoadInfoKeys)
        {
            _loadInfoDict.Remove(loadInfoKey);
        }
    }
}