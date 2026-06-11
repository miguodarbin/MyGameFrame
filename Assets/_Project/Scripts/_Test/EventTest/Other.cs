using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Other : MonoBehaviour
{
    private void OnEnable()
    {
        XEventCenter.Instance.AddEventListener<Monster>(XEventType.E_Example, OnMonsterDead);
    }

    private void OnDisable()
    {
        XEventCenter.Instance.RemoveEventListener<Monster>(XEventType.E_Example, OnMonsterDead);
    }

    private void OnMonsterDead(Monster monster)
    {
        Debug.Log($"其他处理{monster.name}");
    }
}
