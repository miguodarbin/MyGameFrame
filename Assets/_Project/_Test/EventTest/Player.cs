using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
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
        Debug.Log($"玩家击杀{monster.name}");
    }
}