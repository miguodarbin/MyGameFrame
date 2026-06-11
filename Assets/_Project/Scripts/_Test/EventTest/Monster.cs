using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Dead();
        }
    }

    private void Dead()
    {
        Debug.Log("Dead");
        XEventCenter.Instance.EventTrigger("OnMonsterDead");
    }
}
