using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Other : MonoBehaviour
{
    private void OnEnable()
    {
        XEventCenter.Instance.AddListener("OnMonsterDead", OnMonsterDead);
    }

    private void OnDisable()
    {
        XEventCenter.Instance.RemoveListener("OnMonsterDead", OnMonsterDead);
    }

    private void OnMonsterDead()
    {
        Debug.Log("其他处理");
    }
}
