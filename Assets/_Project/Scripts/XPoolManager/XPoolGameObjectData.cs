using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池对象可以得到这个脚本，然后执行删除逻辑
/// </summary>
public class XPoolGameObjectData : MonoBehaviour
{
    public int LimitedUsingCount = 0;
    public string BelongPoolName { get; private set; }

    public void Init(string _poolName)
    {
        BelongPoolName = _poolName;
    }

    public void ReturnMe()
    {
        XPoolManager.Instance.ReturnGameObject(this.gameObject);
    }
}