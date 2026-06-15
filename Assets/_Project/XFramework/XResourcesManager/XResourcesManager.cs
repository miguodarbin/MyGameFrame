using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

/*

* XResourcesManager 具体说明
*
* 一、这个 Manager 解决什么问题？
*
* 1. 统一封装 Resources 的同步 / 异步加载。
* 2. 同一个资源重复加载时，Manager 内部只维护一份加载记录。
* 3. 如果资源正在异步加载，后续相同资源请求不会重复开启新加载，而是合并回调。
* 4. 如果资源已经加载完成，后续加载会直接返回缓存的 asset。
* 5. 通过 refCount 记录“有多少处声明正在使用这个资源”。
* 6. 只有 refCount 归零时，才允许真正移除 / 卸载资源。
*
*
* 二、外部使用规则
*
* 核心原则：
*
* ```
  谁 Load，谁负责 Unload。
  ```
* ```
  Load 和 Unload 必须配对。
  ```
*
* 外部不要关心资源之前有没有被别的地方加载过。
* 哪个模块要用资源，哪个模块就直接调用 Load。
* 不要为了复用资源，去从别的面板、别的 Controller、别的系统里拿资源。
*
* 正确理解：
*
* ```
  外部只表达需求：
  ```
* ```
      我要用这个资源。
  ```
* ```
      我不用这个资源了。
  ```
*
* ```
  Manager 负责内部状态：
  ```
* ```
      资源有没有加载过？
  ```
* ```
      资源是不是正在加载？
  ```
* ```
      有几个地方正在等它？
  ```
* ```
      有几个地方还在用它？
  ```
* ```
      什么时候可以卸载？
  ```
*
*
* 三、同步加载用法
*
* ```
  GameObject prefab = XResourcesManager.Instance.LoadAsset<GameObject>("路径/资源名");
  ```
*
* 注意：
* 1. 路径从 Resources 文件夹内部开始写。
* 2. 不写 Resources。
* 3. 不写文件后缀。
*
* 示例：
*
* ```
  Assets/Resources/Prefabs/Cube.prefab
  ```
*
* 应写成：
*
* ```
  LoadAsset<GameObject>("Prefabs/Cube");
  ```
*
*
* 四、异步加载用法
*
* ```
  XResourcesManager.Instance.LoadAssetAsync<GameObject>("Prefabs/Cube", OnLoadCube);
  ```
*
* ```
  private void OnLoadCube(GameObject prefab)
  ```
* ```
  {
  ```
* ```
      Instantiate(prefab);
  ```
* ```
  }
  ```
*
* 异步加载注意：
*
* 1. 如果资源正在加载中，Manager 会把多个 callback 合并。
* 2. 加载完成后，会统一回调所有还有效的 callback。
* 3. 如果异步加载还没完成时某个调用者不用了，要把当初传入的 callback 传回 UnloadAsset。
*
* 推荐写法：
*
* ```
  LoadAssetAsync<GameObject>("Prefabs/Cube", OnLoadCube);
  ```
* ```
  UnloadAsset<GameObject>("Prefabs/Cube", OnLoadCube);
  ```
*
* 不推荐写法：
*
* ```
  LoadAssetAsync<GameObject>("Prefabs/Cube", obj => { ... });
  ```
*
* 因为匿名 lambda 后续不好传回 UnloadAsset，无法精确移除这一次异步等待回调。
*
*
* 五、卸载用法
*
* ```
  XResourcesManager.Instance.UnloadAsset<GameObject>("Prefabs/Cube");
  ```
*
* 或者异步加载中取消某个 callback：
*
* ```
  XResourcesManager.Instance.UnloadAsset<GameObject>("Prefabs/Cube", OnLoadCube);
  ```
*
* UnloadAsset 的真实含义不是“立刻卸载资源”。
*
* 它的真实含义是：
*
* ```
  当前这个使用者不用了，所以 refCount--。
  ```
*
* 真正是否卸载，要看：
*
* ```
  refCount == 0 && unloadNow == true
  ```
*
*
* 六、unloadNow 参数说明
*
* ```
  UnloadAsset<T>(assetName, callback, unloadNow);
  ```
*
* unloadNow 表示：
*
* ```
  当这次 Unload 后 refCount 归零时，要不要立刻移除 / 卸载资源。
  ```
*
* unloadNow = true：
*
* ```
  refCount 归零后，立刻从 Manager 字典中移除。
  ```
* ```
  如果资源类型允许，会调用 Resources.UnloadAsset。
  ```
*
* unloadNow = false：
*
* ```
  refCount 归零后，暂时不移除。
  ```
* ```
  资源继续留在 Manager 缓存中。
  ```
* ```
  下次再 Load 同一个资源，可以直接复用，避免频繁加载 / 卸载。
  ```
*
* 使用建议：
*
* ```
  常用 UI 图标、常用音效、小资源：
  ```
* ```
      可以 unloadNow = false，先留缓存。
  ```
*
* ```
  大贴图、大音频、临时关卡资源、Boss 专用资源：
  ```
* ```
      可以 unloadNow = true，用完尽快释放。
  ```
*
*
* 七、UnloadUnUsedAssets 用法
*
* ```
  XResourcesManager.Instance.UnloadUnUsedAssets();
  ```
*
* 这个方法做两件事：
*
* 1. 先清理 Manager 字典里 refCount == 0 的资源记录。
* 这一步是让 Manager 不再持有 asset 引用。
*
* 2. 再调用 Resources.UnloadUnusedAssets。
* 这一步是让 Unity 真正扫描并卸载没人引用的资源。
*
* 注意：
*
* ```
  ClearZeroRefCountInfo 只是“松手”。
  ```
* ```
  Resources.UnloadUnusedAssets 才是“让 Unity 回收”。
  ```
*
* 所以 UnloadUnUsedAssets 适合在这些时机调用：
*
* ```
  过场景
  ```
* ```
  进入新关卡
  ```
* ```
  关闭大型模块
  ```
* ```
  明确需要清理内存时
  ```
*
*
* 八、关于 GameObject / Component / AssetBundle
*
* GameObject、Component、AssetBundle 不适合直接用 Resources.UnloadAsset 单独卸载。
*
* 当前 Manager 的处理是：
*
* ```
  对这类资源，refCount 归零时只从字典中移除记录。
  ```
* ```
  真正内存释放交给后续 Resources.UnloadUnusedAssets 处理。
  ```
*
* 注意：
*
* ```
  Resources 加载出来的 Prefab asset 和 Instantiate 出来的场景实例不是一回事。
  ```
*
* ```
  Manager 管的是资源 asset。
  ```
* ```
  Instantiate 出来的实例对象，需要外部自己 Destroy。
  ```
*
*
* 九、关于 refCount
*
* refCount 不是 C# GC 的引用。
* refCount 是 Manager 自己维护的“使用票数”。
*
* ```
  Load 一次，refCount +1。
  ```
* ```
  Unload 一次，refCount -1。
  ```
* ```
  refCount > 0，说明还有地方声明正在使用。
  ```
* ```
  refCount == 0，说明当前没有地方声明正在使用。
  ```
*
* 如果出现 refCount 小于 0：
*
* ```
  说明 Unload 调多了。
  ```
* ```
  或者 Load / Unload 没有配对。
  ```
*
*
* 十、Type 版本注意
*
* Type 版本已标记 Obsolete。
*
* 不要混用：
*
* ```
  LoadAssetAsync<GameObject>("A", callback);
  ```
* ```
  LoadAssetAsync("A", typeof(GameObject), callback);
  ```
*
* 因为泛型版和 Type 版内部 loadInfo 类型不同，混用可能导致类型转换失败。
*
* 实际开发中优先使用泛型版本：
*
* ```
  LoadAsset<T>
  ```
* ```
  LoadAssetAsync<T>
  ```
* ```
  UnloadAsset<T>
  ```
*
*
* 十一、最重要的使用约定
*
* 1. 不要绕过 Manager 直接 Resources.Load 同一个资源。
* 2. 谁 Load，谁 Unload。
* 3. 异步取消时，要传回同一个 callback。
* 4. 不想立刻卸载就 unloadNow = false。
* 5. 大清理时调用 UnloadUnUsedAssets。
* 6. Manager 管 asset，不管 Instantiate 出来的实例。
     */


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
/// <item>
/// 外部只管加载调用，不用去想重复加载
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