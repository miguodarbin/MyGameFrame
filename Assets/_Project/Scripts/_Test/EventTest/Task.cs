using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Task : MonoBehaviour
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
        Debug.Log("任务处理");
    }
}
