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

public class Pool
{
    private Stack<GameObject> _pool;
    private GameObject _poolRoot;

    public Pool(GameObject father, string poolName)
    {
        _pool = new Stack<GameObject>();

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

        //c. 还有 出池逻辑 = 默认处理 + 外部可继续初始化
        if (PoolManager.PrettyShow)
        {
            obj.transform.SetParent(null);
        }

        obj.SetActive(true);
        return obj;
    }

    public void Push(GameObject obj)
    {
        _pool.Push(obj);
        //c.入池逻辑 = PoolManager 最终统一收口
        if (PoolManager.PrettyShow)
        {
            obj.transform.SetParent(_poolRoot.transform);
        }

        obj.SetActive(false);
    }
}

public class PoolManager : SingletonCSharp<PoolManager>
{
    private PoolManager()
    {
    }

    private GameObject _allPoolRoot;

    public static bool PrettyShow = true;


    //1.管理并持有所有小池子，可以根据 String 名字， 找到 Stack<T> 小池子
    Dictionary<string, Pool> _poolsDic = new Dictionary<string, Pool>();

    //2.暴露 公开的取对象接口,要在哪个名字的池子里取对象:
    /// <summary>
    /// 从池子捞出一个对象
    /// </summary>
    /// <param name="poolName">对象池的名字，对象池的名字和对象池一致</param>
    /// <returns></returns>
    public GameObject GetObj(string poolName)
    {
        if (_allPoolRoot == null && PrettyShow)
        {
            _allPoolRoot = new GameObject("AllPoolRoot");
        }

        GameObject obj = null;
        //a. 有这个名字的小池子，并且这个名字的小池子里有对象，那就直接把小池子里的对象给出去，
        if (_poolsDic.ContainsKey(poolName) && _poolsDic[poolName].Count > 0)
        {
            obj = _poolsDic[poolName].Pop();
        }
        //b. 没小池子，那就只创建对象并给出去（重点：小池子的创建是还对象的逻辑）
        else
        {
            var prefab = Resources.Load<GameObject>("PoolPrefabs/" + poolName);
            obj = Object.Instantiate(prefab);
            obj.name = poolName;
        }

        return obj;
    }

    //3.暴露 公开的还对象接口,要把哪个对象还给哪个池子
    /// <summary>
    /// 把对象还给对象池，注意，用对象池的对象的时候不要改名字
    /// </summary>
    /// <param name="obj">还回来的对象名字，会根据这个对象名字找对象池</param>
    public void ReturnObj(GameObject obj)
    {
        //a.有小池子，就直接还给小池子
        if (_poolsDic.ContainsKey(obj.name))
        {
            _poolsDic[obj.name].Push(obj);
        }
        //没有小池子，就先创建一个小池子，再存给小池子,最后注册给管理器
        else
        {
            Pool pool = new Pool(_allPoolRoot, obj.name);
            pool.Push(obj);
            _poolsDic.Add(obj.name, pool);
        }
    }

    //4.清空所有小池子，切断引用，因为管理器和小池子对象的生命周期不一致：
    public void ClearAllPools()
    {
        _poolsDic.Clear();
        _allPoolRoot = null;
    }
}