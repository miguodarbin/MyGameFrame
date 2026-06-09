using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  1.这是纯 C# 管理器，不继承 Mono，不挂载到场景对象上，负责用C#的逻辑管理Unity对象
///  2.这是所有的对象池 管理器
///  3. 池子名和池子对象名必须一一致
///  4. Prefab 必须放到 Resources/PoolPrefabs/下
/// </summary>
public class PoolManager : SingletonCSharp<PoolManager>
{
    private PoolManager()
    {
    }

    private GameObject _allPoolRoot;

    #region 【Hierarchy优化思路】

    //I.所有对象池的Root，在第一次使用池子的时候就该初始化了
    //II.对象在用的时候，完全交出去了，不归自己管，要和池子这边没有任何父子关系
    //III.但是对象被还回来了，就要归池子这边关了，就要绑定父子关系

    #endregion


    //1.管理并持有所有小池子，可以根据 String 名字， 找到 Stack<T> 小池子
    Dictionary<string, Stack<GameObject>> _poolsDic = new Dictionary<string, Stack<GameObject>>();

    //2.暴露 公开的取对象接口,要在哪个名字的池子里取对象:
    public GameObject GetObj(string poolName)
    {
        if (_allPoolRoot == null)
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

        //c. 还有 出池逻辑 = 默认处理 + 外部可继续初始化
        obj.transform.SetParent(null);
        obj.SetActive(true);
        return obj;
    }

    //3.暴露 公开的还对象接口,要把哪个对象还给哪个池子
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
            Stack<GameObject> stack = new Stack<GameObject>();
            stack.Push(obj);
            _poolsDic.Add(obj.name, stack);
        }

        //c.入池逻辑 = PoolManager 最终统一收口
        obj.transform.SetParent(_allPoolRoot.transform);
        obj.SetActive(false);
    }

    //4.清空所有小池子，切断引用，因为管理器和小池子对象的生命周期不一致：
    public void ClearAllPools()
    {
        _poolsDic.Clear();
        _allPoolRoot = null;
    }
}