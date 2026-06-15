using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 限制场上最多对象的对象池总管理器
/// 复用对象可能保留上一次使用状态，外部每次 GetObj 后要自己重置状态
/// 
/// 使用方式：
/// GetObj(poolName) 取对象
/// ReturnObj(obj) 还对象
/// 
/// 注意事项：
/// 1. Prefab 放到 Resources/PoolPrefabs/ 下。
/// 2. Prefab 名必须和 poolName 一致。
/// 3. Prefab 上必须挂 PoolPrefabMaxCount，用来设置最大同时使用数量。
/// 4. 外部通过 GetObj(poolName) 取对象，通过 ReturnObj(obj) 还对象。
/// 5. 不要改对象 name，因为归还时会用 obj.name 找对应池子。
/// </summary>
public class LimitedReusePoolData
{
    private Stack<GameObject> _pool;
    private GameObject _poolRoot;
    private List<GameObject> _usingObj;
    private int _maxUsingCount;

    public int MaxUsingCount
    {
        get { return _maxUsingCount; }
    }

    public LimitedReusePoolData(GameObject father, string poolName, int maxUsingCount)
    {
        _pool = new Stack<GameObject>();
        _usingObj = new List<GameObject>();
        _maxUsingCount = maxUsingCount;

        if (LimitedReusePoolManager.PrettyShow)
        {
            _poolRoot = new GameObject(poolName + "Root");
            _poolRoot.transform.SetParent(father.transform);
        }
    }

    public int Count
    {
        get { return _pool.Count; }
    }

    public GameObject Pop()
    {
        var obj = _pool.Pop();

        if (LimitedReusePoolManager.PrettyShow)
        {
            obj.transform.SetParent(null);
        }


        return obj;
    }

    public void Push(GameObject obj)
    {
        if (LimitedReusePoolManager.PrettyShow)
        {
            obj.transform.SetParent(_poolRoot.transform);
        }

        obj.SetActive(false);
        _pool.Push(obj);
    }

    public void AddUsingObj(GameObject obj)
    {
        _usingObj.Add(obj);
    }

    public bool RemoveUsingObj(GameObject obj)
    {
        return _usingObj.Remove(obj);
    }

    public GameObject GetOldestObj()
    {
        if (_usingObj.Count == 0)
        {
            Debug.LogError("没有使用中的对象了");
            return null;
        }

        var obj = _usingObj[0];

        RemoveUsingObj(obj);
        return obj;
    }

    public int UsingCount
    {
        get { return _usingObj.Count; }
    }
}

public abstract class LimitedReusePoolObjectBase
{
}

public class LimitedReusePoolObject<T> : LimitedReusePoolObjectBase where T : class
{
    public Queue<T> poolObjects = new Queue<T>();
}



public class LimitedReusePoolManager : XSingletonCSharp<LimitedReusePoolManager>
{
    private LimitedReusePoolManager()
    {
    }

    public static bool PrettyShow = false;

    private GameObject _allPoolRoot;

    private Dictionary<string, LimitedReusePoolData> _poolsDic;
    private Dictionary<string, LimitedReusePoolObjectBase> _poolsObjDic;


    /// <summary>
    /// 从对象池取一个对象。
    /// </summary>
    /// <param name="poolName">池子名，同时也是 Prefab 名和对象名。</param>
    /// <returns>返回可用对象；参数错误或资源不存在时返回 null。</returns>
    public GameObject GetObj(string poolName)
    {
        if (_poolsDic == null)
        {
            _poolsDic = new Dictionary<string, LimitedReusePoolData>();
        }

        if (_allPoolRoot == null && PrettyShow)
        {
            _allPoolRoot = new GameObject("PoolRoot");
        }

        GameObject resultObj = null;

        if (!_poolsDic.ContainsKey(poolName)) // 情况 1：第一次取这种对象。
        {
            var prefab = Resources.Load<GameObject>("PoolPrefabs/" + poolName);
            if (prefab == null)
            {
                Debug.LogError("无法找到资源");
                return null;
            }

            var obj = Object.Instantiate(prefab);
            obj.name = poolName;
            PoolPrefabMaxCount poolPrefabMaxCount;
            obj.TryGetComponent(out poolPrefabMaxCount);
            if (poolPrefabMaxCount == null)
            {
                Debug.LogError("Prefab上要挂PoolPrefabMaxCount设置最大使用的数量");
                return null;
            }

            int maxUsingPoolCount = poolPrefabMaxCount.MaxCount;
            if (maxUsingPoolCount <= 0)
            {
                Debug.LogError("池子最大不能小于1");
                return null;
            }

            LimitedReusePoolData limitedReusePoolData = new LimitedReusePoolData(_allPoolRoot, poolName, maxUsingPoolCount);
            _poolsDic.Add(poolName, limitedReusePoolData);

            limitedReusePoolData.AddUsingObj(obj);
            resultObj = obj;
        }
        else if (_poolsDic[poolName].UsingCount < _poolsDic[poolName].MaxUsingCount && _poolsDic[poolName].Count == 0) // 情况 2：未达上限，但没有可复用对象。
        {
            var prefab = Resources.Load<GameObject>("PoolPrefabs/" + poolName);
            if (prefab == null)
            {
                Debug.LogError("无法找到资源");
                return null;
            }

            var obj = Object.Instantiate(prefab);

            obj.name = poolName;

            _poolsDic[poolName].AddUsingObj(obj);
            resultObj = obj;
        }
        else if (_poolsDic[poolName].UsingCount < _poolsDic[poolName].MaxUsingCount && _poolsDic[poolName].Count > 0) // 情况 3：未达上限，并且有可复用对象。
        {
            var obj = _poolsDic[poolName].Pop();


            _poolsDic[poolName].AddUsingObj(obj);
            resultObj = obj;
        }
        else if (_poolsDic[poolName].UsingCount >= _poolsDic[poolName].MaxUsingCount) // 情况 4：已达到使用上限。
        {
            var obj = _poolsDic[poolName].GetOldestObj();

            _poolsDic[poolName].AddUsingObj(obj);
            resultObj = obj;
        }

        else
        {
            Debug.LogError("错误情况");
            return null;
        }

        // 无论对象来自新建、池中复用，还是上限复用，最后都激活。
        resultObj.SetActive(true);
        return resultObj;
    }

    //获取不继承自mono的数据类型
    public T GetObj<T>(string nameSpace = "") where T : class, IXPoolObject, new()
    {
        //池子的名字和是根据类的类型来的，就是他的类名
        string poolName = nameSpace + "." + typeof(T).Name;
        //有池子
        if (_poolsObjDic.ContainsKey(poolName))
        {
            LimitedReusePoolObject<T> poolObject = _poolsObjDic[poolName] as LimitedReusePoolObject<T>;
            //池子是否有可以复用的内容
            if (poolObject.poolObjects.Count > 0)
            {
                T obj = poolObject.poolObjects.Dequeue() as T;
                return obj;
            }
            else
            {
                T obj = new T();
                return obj;
            }
        }
        else //无池子
        {
            T obj = new T();
            return obj;
        }
    }


    /// <summary>
    /// 把对象归还给对象池。
    /// </summary>
    /// <param name="obj">要归还的对象，会根据 obj.name 找池子。</param>
    public void ReturnObj(GameObject obj)
    {
        if (obj == null || _poolsDic == null || !_poolsDic.ContainsKey(obj.name) || _poolsDic[obj.name] == null)
        {
            Debug.LogError("传入为空、不存在池子、池子中没有你还的对象");
            return;
        }

        if (_poolsDic[obj.name].RemoveUsingObj(obj)) // 万一外部把一个已经归还的对象再次调用归还方法。
        {
            _poolsDic[obj.name].Push(obj);
        }
        else
        {
            Debug.LogError("重复归还");
            return;
        }
        // 具体入池逻辑由 Pool.Push 处理。
    }


    public void ReturnObj<T>(T obj, string nameSpace = "") where T : class, IXPoolObject, new()
    {
        //有池子
        //池子的名字和是根据类的类型来的，就是他的类名
        string poolName = nameSpace + "." + typeof(T).Name;

        if (_poolsObjDic.ContainsKey(poolName))
        {
            LimitedReusePoolObject<T> poolObject = _poolsObjDic[poolName] as LimitedReusePoolObject<T>;
            obj.ResetInfo();
            poolObject.poolObjects.Enqueue(obj);
        }
        else //无池子
        {
            LimitedReusePoolObject<T> poolObject = new LimitedReusePoolObject<T>();
            _poolsObjDic.Add(poolName, poolObject);
            obj.ResetInfo();
            poolObject.poolObjects.Enqueue(obj);
        }
    }

    /// <summary>
    /// 清空所有对象池记录。
    /// </summary>
    public void ClearAllPools()
    {
        _poolsObjDic.Clear();
        _poolsDic?.Clear();
        _allPoolRoot = null;
    }
}