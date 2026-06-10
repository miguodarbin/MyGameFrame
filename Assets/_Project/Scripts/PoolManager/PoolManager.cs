using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  1.这是纯 C# 管理器，不继承 Mono，不挂载到场景对象上，负责用C#的逻辑管理Unity对象
///  2.这是所有的对象池 管理器
///  3. 池子名和池子对象名必须一一致
///  4. Prefab 必须放到 Resources/PoolPrefabs/下
/// </summary>
///

#region 【Hierarchy优化思路】

//I.所有对象池的Root，在第一次使用池子的时候就该初始化了
//II.对象在用的时候，完全交出去了，不归自己管，要和池子这边没有任何父子关系
//III.但是对象被还回来了，就要归池子这边管了，就要绑定父子关系
//IIII.这里如果想在Hierarchy里也实现pool也有父对象，只能给pool再封装一层，加上父对象，并且可以把出入池规则也给到pool去处理

#endregion

#region 【最大上限功能思路】

// 1.Pool ：存对象，弹对象，记录使用中的数量，记录自己的父对象,出池的规则。存对象入池规则。
// 2.使用中的对象列表：记录使用中的对象，有Pool就有使用中列表
// 3.PoolManager: 怎么拿对象
// 4.边界情况：

//拿对象：
// Pool为空，Stack为零，Stack不为零
// 使用中列表未超限，使用中列表超限

// 1.Pool为空，                       不用判断使用中列表，                        =》创建池子，创建对象，对象移入使用中列表，返回新建的对象给外部
// 2.使用中列表未超限，Stack为零,       有池子，使用中列表没有达到上限，Stack为0      =》创建对象，对象移入使用中列表，返回新建的对象给外部
// 3.使用中列表未超限，Stack不为零,     有池子，使用中列表没有达到上限，Stack不为0    =》不创建对象。从Stack中弹出一个对象，移入使用中，返回Stack的对象给外部。
// 4.使用中列表超限，                  有池子，使用中列表达到上限，Stack为0         =》不创建对象。从使用中列表最老移出使用中，移入使用中，返回最老的对象给外部。

//还对象：
//有拿才有还，拿了就说明Pool不为空，并且还之前，必须要存入过，否则会超出索引，仔细检查了上面那对象的情况，都有移入使用中，那只要还的时候移出使用中即可

#endregion

public class Pool
{
    private Stack<GameObject> _pool;
    private GameObject _poolRoot;
    private List<GameObject> _usingObj;

    public Pool(GameObject father, string poolName)
    {
        _pool = new Stack<GameObject>();
        _usingObj = new List<GameObject>();
        if (PoolManager.PrettyShow)
        {
            _poolRoot = new GameObject(poolName);
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

        if (PoolManager.PrettyShow)
        {
            obj.transform.SetParent(null);
        }


        return obj;
    }

    public void Push(GameObject obj)
    {
        if (PoolManager.PrettyShow)
        {
            obj.transform.SetParent(_poolRoot.transform);
        }

        //入池逻辑
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
            Debug.LogError("没有使用中的对象里");
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

public class PoolManager : SingletonCSharp<PoolManager>
{
    private PoolManager()
    {
    }

    public static bool PrettyShow = true;
    private GameObject _allPoolRoot;
    private Dictionary<string, Pool> _poolsDic;


    /// <summary>
    /// 从池子捞出一个对象
    /// </summary>
    /// <param name="poolName">对象池的名字，对象池的名字和对象池一致</param>
    /// <returns></returns>
    public GameObject GetObj(string poolName, int maxUsingPoolCount)
    {
        if (maxUsingPoolCount <= 0)
        {
            Debug.LogError("池子最大不能小于1");
            return null;
        }

        //懒加载字段
        if (_poolsDic == null)
        {
            _poolsDic = new Dictionary<string, Pool>();
        }

        if (_allPoolRoot == null && PrettyShow)
        {
            _allPoolRoot = new GameObject("PoolRoot");
        }


        //拿对象规则
        GameObject resultObj = null;
        if (!_poolsDic.ContainsKey(poolName)) //没有这个池子的话
        {
            var prefab = Resources.Load<GameObject>("PoolPrefabs/" + poolName);
            if (prefab == null)
            {
                Debug.LogError("无法找到资源");
                return null;
            }

            Pool pool = new Pool(_allPoolRoot, poolName);
            _poolsDic.Add(poolName, pool);

            var obj = Object.Instantiate(prefab);

            obj.name = poolName;
            pool.AddUsingObj(obj);
            resultObj = obj;
        }
        else if (_poolsDic[poolName].UsingCount < maxUsingPoolCount && _poolsDic[poolName].Count == 0) //有池子，但没有达到上限，stack为0了
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
        else if (_poolsDic[poolName].UsingCount < maxUsingPoolCount && _poolsDic[poolName].Count > 0) //有池子，但没有达到上限，stack也有东西
        {
            var obj = _poolsDic[poolName].Pop();
            _poolsDic[poolName].AddUsingObj(obj);
            resultObj = obj;
        }
        else if (_poolsDic[poolName].UsingCount >= maxUsingPoolCount) //有池子，但达到上限了，不管stack里有没有东西，都拿使用中最老的来顶
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

        //出池逻辑
        resultObj.SetActive(true);
        return resultObj;
    }

    /// <summary>
    /// 把对象还给对象池，注意，用对象池的对象的时候不要改名字
    /// </summary>
    /// <param name="obj">还回来的对象名字，会根据这个对象名字找对象池</param>
    public void ReturnObj(GameObject obj)
    {
        if (obj == null || _poolsDic == null || !_poolsDic.ContainsKey(obj.name) || _poolsDic[obj.name] == null)
        {
            Debug.LogError("传入为空、不存在池子、池子中没有你还的对象");
            return;
        }


        if (_poolsDic[obj.name].RemoveUsingObj(obj)) //万一外部把一个归还的在调用归还方法
        {
            _poolsDic[obj.name].Push(obj);
        }
        else
        {
            Debug.LogError("重复归还");
            return;
        }

        //入池逻辑放给Pool最后收尾了
    }

    //4.清空所有小池子，切断引用，因为管理器和小池子对象的生命周期不一致：
    public void ClearAllPools()
    {
        _poolsDic?.Clear();
        _allPoolRoot = null;
    }
}