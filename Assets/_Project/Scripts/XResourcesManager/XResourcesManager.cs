using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

//首先目前的问题是什么？现在如果异步加载一个资源的话，会正常异步加载，会在下几帧加载完毕，但是在加载完毕之前如果再继续加载同一个资源，那不就浪费了吗，
//所以想着能不能就是如果检测到了已经正在加载这个资源了，那好，我就不加载了，
//但我也不报错，我看你传过来的回调是啥，等第一次加载完的那个协程走到加载完毕时运行回调那一步，我把你这次传来的回调，也加到第一次触发的回调里面，也算用了委托的多播
//行，大概思路有了，这时候要细化一下，怎么检测这个资源有没有被正在加载？
//这样吧，在Manager内部维护一个已加载资源的字典
//key就用 资源名就用路径+类型，以防不同类型但是同名的不被加载
//value就用 这个异步加载的asset用来判断这个异步加载有没有完，对了，还要有这个异步加载的总回调，到时候如果有多个同时加载，就统一用这个value的回调函数来触发，对了还有有个协程句柄，用来关闭重复的协程（这个可能涉及到同步加载那边，这一版先不管这个协程句柄能不能用到）
//一开始我觉得等到真的去加载了，再先去判断这个字典有没有初始化，没有的话先初始化。但后来想了想，算了不这样搞了，反正这个资源加载肯定要用，还不如一开始就初始化好，省得每次调方法浪费性能去判空了。
//然后判断这个字典里有没有这个资源名的value，
//- 如果有的话，就说明这个资源被加载了或者正在加载，靠字典value里的asset是否为空判断是否加载完毕，
//  --加载完毕了，那就直接触发外部给的回调，把asset传出去
//  --没加载完，那就直接把外部给的回调添加到这个资源名的value里的总回调函数里
//- 没有这个资源名的value就说明这个资源没有被加载
//  -然后就去异步加载这个资源，把资源名字、回调函数、协程句柄注册到字典里
// 然后都加载完了要把这个字典里的资源名的value里的Callback置空一下，否则这边长期揪着外部监听对象引用不放，外部监听对象被Destory了，GC看到这里有东西引用着，都无法回收，稍等啊，这种情况
// 基本不可能发生啊，一般GC都是过场景的时候GC，而过场景的时候，我都需要把整个字典都清理了？好吧万一还有下个场景我需要用的资源，我可能不清，所以这个做法做得对，但似乎课上的唐老师没有写一部分逻辑？
//好了，接下来继续做了，首先重复一下我们的目标，就是在同时加载两个相同的资源的时候，能正确处理异步加载、同步加载、单个资源卸载，刚才完成了处理异步加载，
//接下来要处理同步加载了
//分两个情况，第一种，同步加载了，再来一个同步，第二种情况，异步加载了，再来一个同步加载
//情况1，第一次资源加载完成了，然后第二次又再同步加载A资源或者异步加载A资源，这时候就需要每次加载前都判断一下，目前字典里有没有加载过，
//如果加载过就直接用加载的资源给到外部，没加载过再正常走同步加载或者异步加载
//情况2 第一次资源A异步加载到一半，又来了一个同步加载资源A，此时应该取消异步协程，让Unity继续加载这个资源，同时用同步加载的方法去拿资源
//同步方法处理完了，接下来处理单个资源卸载
//要想卸载指定的一个资源，首先确定一下这个资源目前可能的状态，有可能是正在同步加载中？不可能。
//可能的情况：异步加载中的资源，加载成功的资源
//对于异步加载中的资源，由于Unity底层已经发布号令去加载了，C#这边没有组织Unity取消加载的API，所以只能先给加载信息里面加一个字段，比如删除标记给到true，
//然后再协程最后满足了结束条件的那个case里，判断一下删除标记，如果true就删掉，卸载完还要把字典记录清除
//对于已经加载成功的资源，就直接调用Unity的卸载资源的API就行，卸载完还要把字典记录清除


public class XAssetLoadAsyncInfoBase
{
}

public class XAssetLoadAsyncAsyncInfo<T> : XAssetLoadAsyncInfoBase
{
    public T asset;
    public UnityAction<T> totalCallback;
    public Coroutine coroutineInfo;
    public bool deleteFlag = false;
}


/// <summary>
/// string assetName也算是文件的完整路径，如果asset在Resources的别的文件夹下，由外部去写路径
/// </summary>
public class XResourcesManager : XSingletonCSharp<XResourcesManager>
{
    private XResourcesManager()
    {
    }

    //字典，存储了所有异步加载的情况信息，Key是 路径+类型。Value是自定义结构类,包括了资源本体，异步加载完毕要触发的回调，这次异步加载的句柄
    private Dictionary<string, XAssetLoadAsyncInfoBase> _loadInfoDict = new Dictionary<string, XAssetLoadAsyncInfoBase>();

    //异步加载资源 -泛型,可以按泛型参数给的类型查找资源，不同类型同名也没问题
    public void LoadAssetAsync<T>(string assetName, UnityAction<T> callback) where T : Object
    {
        string fullAssetName = assetName + "." + typeof(T).Name;

        if (_loadInfoDict.ContainsKey(fullAssetName))
        {
            var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<T>;
            //这里都靠名字和类型双保险进来了，_loadInfoDict[fullAssetName]的类型一定不会变,这里就不对loadInfo显示判空了,如果为空了，那就是第两次用的加载方法不一样，一次用的泛型异步加载，一次用的Type异步加载
            if (loadInfo.asset != null)
            {
                callback.Invoke(loadInfo.asset);
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
            if (loadInfo.deleteFlag)
            {
                Resources.UnloadAsset(loadInfo.asset);
                _loadInfoDict.Remove(fullAssetName);
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
            //这里都靠名字和类型双保险进来了，_loadInfoDict[fullAssetName]的类型一定不会变,这里就不对loadInfo显示判空了
            if (loadInfo.asset != null)
            {
                callback.Invoke(loadInfo.asset);
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
            if (loadInfo.deleteFlag)
            {
                Resources.UnloadAsset(loadInfo.asset);
                _loadInfoDict.Remove(fullAssetName);
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


    //同步加载资源 -泛型
    public T LoadAsset<T>(string assetName) where T : Object
    {
        string fullAssetName = assetName + "." + typeof(T).Name;
        if (_loadInfoDict.ContainsKey(fullAssetName)) //先看看有没有加载过
        {
            //加载的状态如何，如果加载完了，那就直接用，没加载完那就取消协程，但不取消Unity加载，让同步方法这里拿到资源对象,并代替执行之前协程的回调函数
            var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<T>;
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
            _loadInfoDict.Add(fullAssetName, loadInfo);
            loadInfo.asset = asset;
            return asset;
        }
    }

    //异步卸载未使用资源
    public void UnloadUnUsedAssets(UnityAction callback)
    {
        XMonoManager.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callback));
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callback)
    {
        var request = Resources.UnloadUnusedAssets();
        yield return request;
        callback.Invoke();
    }

    //同步卸载指定资源-泛型
    public void UnloadAsset<T>(string assetName) where T : Object
    {
        string fullAssetName = assetName + "." + typeof(T).Name;
        if (!_loadInfoDict.ContainsKey(fullAssetName))
        {
            return;
        }

        var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<T>;
        if (loadInfo.asset == null)
        {
            loadInfo.deleteFlag = true;
        }
        else
        {
            Resources.UnloadAsset(loadInfo.asset);
            _loadInfoDict.Remove(fullAssetName);
        }
    }

    //同步卸载指定资源-Type
    public void UnloadAsset(string assetName, Type type)
    {
        string fullAssetName = assetName + "." + type.Name;
        if (!_loadInfoDict.ContainsKey(fullAssetName))
        {
            return;
        }

        var loadInfo = _loadInfoDict[fullAssetName] as XAssetLoadAsyncAsyncInfo<Object>;
        if (loadInfo.asset == null)
        {
            loadInfo.deleteFlag = true;
        }
        else
        {
            Resources.UnloadAsset(loadInfo.asset);
            _loadInfoDict.Remove(fullAssetName);
        }
    }
}