using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    private void Awake()
    {
        InitInput();
    }

    private void InitInput()
    {
        var releaseFireBallKeyInfo = new XInputInfo(XInputInfo.KeyBoardOrMouse.Keyboard, XInputInfo.KeyState.Down, KeyCode.Space);
        XInputManager.Instance.AddOrChangeInputEvent(XEventType.E_Confirm, releaseFireBallKeyInfo);
    }

    private void OnEnable()
    {
        XInputManager.Instance.enableInput = true;
        XEventCenter.Instance.AddEventListener(XEventType.E_Confirm, ReleaseFireball);
    }

    private void OnDisable()
    {
        XEventCenter.Instance.RemoveEventListener(XEventType.E_Confirm, ReleaseFireball);
    }

    private void ReleaseFireball()
    {
        Debug.Log("ReleaseFireball!!!");
    }
}