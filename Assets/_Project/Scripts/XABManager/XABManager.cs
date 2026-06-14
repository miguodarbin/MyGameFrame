using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;


/// <summary>
/// 主要是调用里面的LoadABRes这个公开接口，来加载AB包里的资源
/// </summary>
public class XABManager : XSingletonAutoMono<XABManager>
{
    //避免重复加载AB包 - 字典
    private Dictionary<string, AssetBundle> _assetBundleDict = new Dictionary<string, AssetBundle>();


    //AB包路径
    private string _assetBundlePath = Application.streamingAssetsPath;

    public string AssetBundlePath
    {
        get { return _assetBundlePath; }
        set { _assetBundlePath = value; }
    }


    //根据打包平台选择编译的代码
    public string MainAssetBundleName
    {
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "PC";
#endif
        }
    }


    private AssetBundle _mainBundle;
    private AssetBundleManifest _manifest;

    //尝试同步加载主包和主包里的依赖文件
    private bool TryLoadMainBundle()
    {
        if (this._mainBundle != null && _manifest != null)
        {
            return true;
        }

        //如果主包是空的，就加载主包和主包的依赖文件，并把主包记录到字典
        var mainBundle = AssetBundle.LoadFromFile(AssetBundlePath + "/" + MainAssetBundleName);
        if (mainBundle == null)
        {
            Debug.LogError(AssetBundlePath + "/" + MainAssetBundleName + ":加载主包失败");
            return false;
        }

        this._mainBundle = mainBundle;
        _assetBundleDict.Add(MainAssetBundleName, mainBundle);

        //加载依赖文件
        if (_manifest != null)
        {
            Debug.LogWarning("将用新的主包依赖文件覆盖旧的依赖文件");
        }

        _manifest = _mainBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        if (_manifest == null)
        {
            Debug.LogError(_manifest + ":依赖文件加载失败");
            return false;
        }

        return true;
    }

    //尝试异步加载 资源包本身和他的依赖包 用回调函数表示是否加载成功
    private IEnumerator LoadPackageAndDepends(string packageName, UnityAction<bool> callback, bool isSync = false)
    {
        //如果字典里有记录了，就说明有这个资源包了
        if (_assetBundleDict.ContainsKey(packageName))
        {
            if (_assetBundleDict[packageName] == null) //如果这个资源包为空，那就等资源包加载完
            {
                while (_assetBundleDict.ContainsKey(packageName) && _assetBundleDict[packageName] == null)
                {
                    yield return null;
                }

                if (!_assetBundleDict.ContainsKey(packageName))
                {
                    Debug.LogError("资源包加载失败，等待终止：" + packageName);
                    callback?.Invoke(false);
                    yield break;
                }

                callback?.Invoke(true);
            }
            else //如果资源包不为空，那就什么都不做，直接进入下一步异步加载资源
            {
                callback?.Invoke(true);
            }
        }
        else //字典里没有这个资源包的记录，那就先异步加载依赖包，然后再加载资源包
        {
            //1.加载依赖包
            _assetBundleDict.Add(packageName, null);
            var dependInfos = _manifest.GetAllDependencies(packageName);
            foreach (var dependInfo in dependInfos)
            {
                //没这个依赖包的记录
                if (!_assetBundleDict.ContainsKey(dependInfo))
                {
                    //如果是同步加载的话
                    if (isSync)
                    {
                        var bundle = AssetBundle.LoadFromFile(_assetBundlePath + "/" + dependInfo);
                        if (bundle == null)
                        {
                            Debug.LogError("同步加载依赖包失败：" + dependInfo);
                            _assetBundleDict.Remove(packageName);
                            callback?.Invoke(false);
                            yield break;
                        }

                        _assetBundleDict.Add(dependInfo, bundle);
                    }
                    else //如果是异步加载的话
                    {
                        _assetBundleDict.Add(dependInfo, null);
                        var dependRequest = AssetBundle.LoadFromFileAsync(_assetBundlePath + "/" + dependInfo);
                        yield return dependRequest;
                        if (dependRequest.assetBundle == null)
                        {
                            Debug.LogError("加载依赖包失败：" + dependInfo);
                            _assetBundleDict.Remove(dependInfo);
                            _assetBundleDict.Remove(packageName);
                            callback?.Invoke(false);
                            yield break;
                        }

                        _assetBundleDict[dependInfo] = dependRequest.assetBundle;
                    }
                }
                else //有这个依赖包记录
                {
                    if (_assetBundleDict[dependInfo] == null) //依赖包还没加载好
                    {
                        while (_assetBundleDict.ContainsKey(dependInfo) && _assetBundleDict[dependInfo] == null)
                        {
                            yield return null;
                        }

                        if (!_assetBundleDict.ContainsKey(dependInfo))
                        {
                            Debug.LogError("依赖包加载失败，等待终止：" + dependInfo);
                            _assetBundleDict.Remove(packageName);
                            callback?.Invoke(false);
                            yield break;
                        }
                    }
                }
            }

            //2.加载资源包
            if (isSync)
            {
                var bundle = AssetBundle.LoadFromFile(_assetBundlePath + "/" + packageName);
                if (bundle == null)
                {
                    Debug.LogError("同步加载资源包失败,可能资源包的名字或路径不对？");
                    _assetBundleDict.Remove(packageName);
                    callback?.Invoke(false);
                    yield break;
                }

                _assetBundleDict[packageName] = bundle;

                callback?.Invoke(true);
            }
            else
            {
                var packageRequest = AssetBundle.LoadFromFileAsync(_assetBundlePath + "/" + packageName);
                yield return packageRequest;
                if (packageRequest.assetBundle == null)
                {
                    Debug.LogError("加载资源包失败,可能资源包的名字或路径不对？");
                    _assetBundleDict.Remove(packageName);
                    callback?.Invoke(false);
                    yield break;
                }

                _assetBundleDict[packageName] = packageRequest.assetBundle;

                callback?.Invoke(true);
            }
        }
    }


    //异步加载资源 - 用泛型和名字查找资源
    public void GetAssetBundleRes<T>(string packageName, string resName, UnityAction<T> callback, bool isSync = false) where T : Object
    {
        if (!TryLoadMainBundle())
        {
            Debug.LogError("主包或依赖文件未加载成功");
            return;
        }

        StartCoroutine(ReallyLoadAsset(packageName, resName, callback, isSync));
    }

    private IEnumerator ReallyLoadAsset<T>(string packageName, string resName, UnityAction<T> callback, bool isSync = false) where T : Object
    {
        bool isLoadPackageAndDependsSuccess = false;
        yield return LoadPackageAndDepends(packageName, (result) => { isLoadPackageAndDependsSuccess = result; }, isSync);
        if (!isLoadPackageAndDependsSuccess)
        {
            Debug.LogError("加载主包或者依赖包失败");
            yield break;
        }

        //到这一步就说明处理好资源包和依赖包了，处理资源就好了
        if (isSync)
        {
            var asset = _assetBundleDict[packageName].LoadAsset<T>(resName);
            if (asset == null)
            {
                Debug.LogError("同步加载资源失败,可能资源文件的名字或路径不对？");
                yield break;
            }

            callback?.Invoke(asset);
        }
        else
        {
            var resRequest = _assetBundleDict[packageName].LoadAssetAsync<T>(resName);
            yield return resRequest;
            if (resRequest.asset == null)
            {
                Debug.LogError("加载资源失败,可能资源文件的名字或路径不对？");
                yield break;
            }

            callback?.Invoke(resRequest.asset as T);
        }
    }


    //异步加载资源 - 用普通名字查找资源
    public void GetAssetBundleRes(string packageName, string resName, UnityAction<Object> callback, bool isSync = false)
    {
        if (!TryLoadMainBundle())
        {
            Debug.LogError("主包或依赖文件未加载成功");
            return;
        }

        StartCoroutine(ReallyLoadAsset(packageName, resName, callback, isSync));
    }

    private IEnumerator ReallyLoadAsset(string packageName, string resName, UnityAction<Object> callback, bool isSync = false)
    {
        bool isLoadPackageAndDependsSuccess = false;
        yield return LoadPackageAndDepends(packageName, (result) => { isLoadPackageAndDependsSuccess = result; }, isSync);
        if (!isLoadPackageAndDependsSuccess)
        {
            Debug.LogError("加载主包或者依赖包失败");
            yield break;
        }

        //到这一步就说明处理好资源包和依赖包了，处理资源就好了
        if (isSync)
        {
            var asset = _assetBundleDict[packageName].LoadAsset(resName);
            if (asset == null)
            {
                Debug.LogError("同步加载资源失败,可能资源文件的名字或路径不对？");
                yield break;
            }

            callback?.Invoke(asset);
        }
        else
        {
            var resRequest = _assetBundleDict[packageName].LoadAssetAsync(resName);
            yield return resRequest;
            if (resRequest.asset == null)
            {
                Debug.LogError("加载资源失败,可能资源文件的名字或路径不对？");
                yield break;
            }

            callback?.Invoke(resRequest.asset);
        }
    }


    //异步加载资源 - 用Type和名字查找资源
    public void GetAssetBundleRes(string packageName, string resName, Type type, UnityAction<Object> callback, bool isSync = false)
    {
        if (!TryLoadMainBundle())
        {
            Debug.LogError("主包或依赖文件未加载成功");
            return;
        }

        StartCoroutine(ReallyLoadAsset(packageName, resName, type, callback, isSync));
    }

    private IEnumerator ReallyLoadAsset(string packageName, string resName, Type type, UnityAction<Object> callback, bool isSync = false)
    {
        bool isLoadPackageAndDependsSuccess = false;
        yield return LoadPackageAndDepends(packageName, (result) => { isLoadPackageAndDependsSuccess = result; }, isSync);
        if (!isLoadPackageAndDependsSuccess)
        {
            Debug.LogError("加载主包或者依赖包失败");
            yield break;
        }

        //到这一步就说明处理好资源包和依赖包了，处理资源就好了
        if (isSync)
        {
            var asset = _assetBundleDict[packageName].LoadAsset(resName, type);
            if (asset == null)
            {
                Debug.LogError("同步加载资源失败,可能资源文件的名字或路径不对？");
                yield break;
            }

            callback?.Invoke(asset);
        }
        else
        {
            var resRequest = _assetBundleDict[packageName].LoadAssetAsync(resName, type);
            yield return resRequest;
            if (resRequest.asset == null)
            {
                Debug.LogError("加载资源失败,可能资源文件的名字或路径不对？");
                yield break;
            }

            callback?.Invoke(resRequest.asset);
        }
    }


    //TODO:目前的卸载，都无法卸载正在异步加载中的包，以后有需求了再改改这里，现在不继续往下扩了
    //大概是同步卸载，卸载指定AB包
    public void UnloadAssetBundle(string packageName, UnityAction<bool> callback)
    {
        if (_assetBundleDict == null)
        {
            return;
        }

        if (!_assetBundleDict.ContainsKey(packageName))
        {
            callback?.Invoke(false);
            return;
        }

        if (_assetBundleDict[packageName] == null)
        {
            Debug.LogError("资源正在异步加载中，无法卸载");
            callback?.Invoke(false);
        }
        else
        {
            _assetBundleDict[packageName].Unload(false);
            _assetBundleDict.Remove(packageName);
            callback?.Invoke(true);
        }
    }

    //大概是同步卸载，卸载全部AB包
    public void UnloadAllAssetBundle()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        _assetBundleDict.Clear();
        _mainBundle = null;
        _manifest = null;
    }
}