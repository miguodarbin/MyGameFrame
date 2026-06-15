using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池对象可以得到这个脚本，然后执行删除逻辑,或者限制池子里最多使用多少个
/// </summary>
public class XPoolGameObjectData : MonoBehaviour
{
    // 0表示不限制，小于0非法，大于 0 表示该池对象最多同时在场景中存在的数量
    //只有 GetGameObject(poolName, true) 时，这个限制才会生效
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