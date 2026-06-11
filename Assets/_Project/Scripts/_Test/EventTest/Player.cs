using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Player : MonoBehaviour
{
    private void OnEnable()
    {
        XEventCenter.Instance.AddListener<Monster>("OnMonsterDead", OnMonsterDead);
    }

    private void OnDisable()
    {
        XEventCenter.Instance.RemoveListener<Monster>("OnMonsterDead", OnMonsterDead);
    }

    private void OnMonsterDead(Monster monster)
    {
        Debug.Log($"玩家击杀{monster.name}");
    }
}