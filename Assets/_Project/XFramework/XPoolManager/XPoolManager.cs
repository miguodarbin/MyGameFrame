using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class XGameObjectPoolWrapper
{
    private string _poolName;
    private ObjectPool<GameObject> _pool;
    private GameObject _root;
    private GameObject _prefab;
    private int _limitedUsingCount = 0;
    private List<GameObject> _usingList;
    public bool IsValid { get; private set; } = true;

    public XGameObjectPoolWrapper(string poolName, GameObject father = null)
    {
        _poolName = poolName;
        _pool = new ObjectPool<GameObject>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, true);
        _usingList = new List<GameObject>();

        var prefab = Resources.Load<GameObject>("PoolPrefabs/" + _poolName);
        if (prefab == null)
        {
            Debug.LogError("加载失败");
            IsValid = false;
            return;
        }

        _prefab = prefab;

        var objData = _prefab.GetComponent<XPoolGameObjectData>();
        if (objData != null)
        {
            _limitedUsingCount = objData.LimitedUsingCount;
        }

        if (XPoolManager.PrettyShow)
        {
            if (father == null)
            {
                Debug.LogError("池子节点的Root为空");
            }
            else
            {
                _root = new GameObject(poolName);
                _root.transform.SetParent(father.transform);
            }
        }
    }


    public void Release(GameObject obj)
    {
        _pool.Release(obj);
    }

    public GameObject Get(bool limited = false)
    {
        //达到限制的创建，直接把最老的返回出去,但不会重置状态，外部需要重新处理状态
        if (_limitedUsingCount > 0 && limited)
        {
            if (_usingList.Count >= _limitedUsingCount)
            {
                var oldest = _usingList[0];
                _usingList.RemoveAt(0);
                OutPoolRule(oldest);
                return oldest;
            }
        }

        //普通get
        return _pool.Get();
    }

    public void ClearPool()
    {
        if (_usingList != null)
        {
            for (int i = 0; i < _usingList.Count; i++)
            {
                Object.Destroy(_usingList[i]);
                _usingList[i] = null;
            }
        }

        _pool.Clear();
        if (_root != null)
        {
            Object.Destroy(_root);
            _root = null;
        }

        _usingList.Clear();
        _usingList = null;
    }


    private GameObject CreateFunc()
    {
        if (_prefab == null)
        {
            return null;
        }

        var obj = Object.Instantiate(_prefab);
        obj.name = _poolName;


        var objData = obj.GetComponent<XPoolGameObjectData>();
        if (objData == null)
        {
            objData = obj.AddComponent<XPoolGameObjectData>();
        }

        objData.Init(_poolName);

        return obj;
    }

    private void ActionOnGet(GameObject obj)
    {
        OutPoolRule(obj);
    }


    private void ActionOnRelease(GameObject obj)
    {
        InPoolRule(obj);
    }

    private void ActionOnDestroy(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("未知情况");
            return;
        }

        Object.Destroy(obj);
    }

    private void OutPoolRule(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("原生对象池给的是空对象");
            return;
        }

        obj.transform.SetParent(null);
        _usingList.Add(obj);
        obj.SetActive(true);
    }

    private void InPoolRule(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("回收的是空对象");
            return;
        }

        if (_root != null)
        {
            obj.transform.SetParent(_root.transform);
        }

        _usingList.Remove(obj);
        obj.SetActive(false);
    }
}

public abstract class XCSharpPoolWrapperBase
{
    public abstract void ClearPool();
}


public class XCSharpPoolWrapper<T> : XCSharpPoolWrapperBase where T : class, IXPoolObject, new()
{
    private ObjectPool<T> _pool;

    private List<T> _usingList;

    public ObjectPool<T> Pool
    {
        get { return _pool; }
    }

    public XCSharpPoolWrapper()
    {
        _pool = new ObjectPool<T>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, true);
        _usingList = new List<T>();
    }

    public T CreateFunc()
    {
        return new T();
    }

    public void ActionOnGet(T obj)
    {
        _usingList.Add(obj);
        obj.ResetInfo();
    }

    public void ActionOnRelease(T obj)
    {
        _usingList.Remove(obj);
        obj.Invalid();
    }

    public void ActionOnDestroy(T obj)
    {
        obj.Invalid();
    }

    public override void ClearPool()
    {
        if (_usingList != null)
        {
            foreach (var obj in _usingList)
            {
                obj.Invalid();
            }
        }

        _usingList.Clear();
        _pool.Clear();
    }
}

/// <summary>
/// 对象池管理器
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c>GetGameObject(poolName, isLimited = false)</c>：从指定 GameObject 池中取对象 </description>
/// </item>
/// <item>
/// <description><c>ReturnGameObject(obj)</c>：归还 GameObject 到它所属的池子 </description>
/// </item>
/// <item>
/// <description><c> GetCsharpObject&lt;T&gt;()</c>： 从指定 C# 类型池中取一个对象 </description>
/// </item>
/// <item>
/// <description><c> ReturnCsharpObject&lt;T&gt;(obj)</c>：归还普通 C# 对象到对应类型的池子 </description>
/// </item>
/// <item>
/// <description><c> Clear() </c>： 清空所有 GameObject 池和 C# 对象池 </description>
/// </item>
/// </list>
/// </remarks>
/// /// <remarks>
/// 外部须知：
/// <list type="number">
///  <item>
/// GameObject 预制体必须放在 Resources/PoolPrefabs/ 下，且文件名要等于 poolName
/// </item>
///  <item>
/// 外部需要自己重置对象的位置、旋转、状态、数据
/// </item>
///  <item>
/// isLimited = true 时，如果达到数量上限，会直接复用最早取出的对象，外部必须重新初始化它
/// </item>
///  <item>
/// 普通 C# 池对象必须实现 IXPoolObject，并提供无参构造函数
/// </item>
/// </list>
/// </remarks>
public class XPoolManager : XSingletonCSharp<XPoolManager>
{
    private XPoolManager()
    {
    }

    private GameObject _allRoot;
    private Dictionary<string, XGameObjectPoolWrapper> _gameObjectPoolDict;
    private Dictionary<Type, XCSharpPoolWrapperBase> _cSharpObjectPoolDict;


    public static bool PrettyShow = false;

    public GameObject GetGameObject(string poolName, bool isLimited = false)
    {
        if (_gameObjectPoolDict == null) //初始化管理器字典
        {
            _gameObjectPoolDict = new Dictionary<string, XGameObjectPoolWrapper>();
        }

        if (!_gameObjectPoolDict.ContainsKey(poolName)) //初始化对象池
        {
            XGameObjectPoolWrapper poolWrapper;
            if (PrettyShow)
            {
                if (_allRoot == null)
                {
                    _allRoot = new GameObject("AllPoolRoot");
                }

                poolWrapper = new XGameObjectPoolWrapper(poolName, _allRoot);
            }
            else
            {
                poolWrapper = new XGameObjectPoolWrapper(poolName);
            }

            if (!poolWrapper.IsValid)
            {
                Debug.LogError("池子初始化失败");
                return null;
            }

            _gameObjectPoolDict.Add(poolName, poolWrapper);
        }


        var result = _gameObjectPoolDict[poolName].Get(isLimited);


        if (result == null)
        {
            Debug.Log($"返回失败{poolName}");
            return null;
        }

        return result;
    }

    public void ReturnGameObject(GameObject obj)
    {
        if (obj == null || _gameObjectPoolDict == null)
        {
            Debug.LogError("无法归还空对象，或者没有存过对象");
            return;
        }

        XPoolGameObjectData poolGameObjectData = obj.GetComponent<XPoolGameObjectData>();
        if (poolGameObjectData == null)
        {
            Debug.LogError("无法找到对象上的PoolData脚本");
            return;
        }

        _gameObjectPoolDict.TryGetValue(poolGameObjectData.BelongPoolName, out XGameObjectPoolWrapper poolData);
        if (poolData == null)
        {
            Debug.LogError("无法找到对象上的PoolData池子");
            return;
        }

        poolData.Release(obj);
    }

    public T GetCsharpObject<T>() where T : class, IXPoolObject, new()
    {
        if (_cSharpObjectPoolDict == null)
        {
            _cSharpObjectPoolDict = new Dictionary<Type, XCSharpPoolWrapperBase>();
        }

        Type type = typeof(T);

        if (!_cSharpObjectPoolDict.ContainsKey(type))
        {
            XCSharpPoolWrapperBase csharpPoolWrapper = new XCSharpPoolWrapper<T>();
            _cSharpObjectPoolDict.Add(type, csharpPoolWrapper);
        }

        var result = (_cSharpObjectPoolDict[type] as XCSharpPoolWrapper<T>)?.Pool.Get();
        return result;
    }

    public void ReturnCsharpObject<T>(T obj) where T : class, IXPoolObject, new()
    {
        if (obj == null || _cSharpObjectPoolDict == null)
        {
            Debug.LogError("无法归还空对象，或者没有存过对象");
            return;
        }

        Type type = typeof(T);


        if (!_cSharpObjectPoolDict.ContainsKey(type))
        {
            Debug.LogError("无法找到该类型池子");
            return;
        }


        (_cSharpObjectPoolDict[type] as XCSharpPoolWrapper<T>)?.Pool.Release(obj);
    }


    public void Clear()
    {
        if (_gameObjectPoolDict != null)
        {
            foreach (var poolData in _gameObjectPoolDict.Values)
            {
                poolData.ClearPool();
            }

            if (_allRoot != null)
            {
                Object.Destroy(_allRoot);
                _allRoot = null;
            }

            _gameObjectPoolDict.Clear();
            _gameObjectPoolDict = null;
        }


        if (_cSharpObjectPoolDict != null)
        {
            foreach (var pool in _cSharpObjectPoolDict.Values)
            {
                pool.ClearPool();
            }

            _cSharpObjectPoolDict.Clear();
            _cSharpObjectPoolDict = null;
        }
    }

    // public int DebugCsharpPool<T>() where T : class, IXPoolObject, new()
    // {
    //     if (_cSharpObjectPoolDict == null)
    //     {
    //         return 0;
    //     }
    //
    //     var i = (_cSharpObjectPoolDict[typeof(T)] as CSharpPoolWrapper<T>);
    //     if (i == null)
    //     {
    //         return 0;
    //     }
    //
    //     var result = i.Pool.CountActive;
    //     return result;
    // }
}


public interface IXPoolObject
{
    //重置数据的方法
    void ResetInfo();


    void Invalid();
}