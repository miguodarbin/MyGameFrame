using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMain : MonoBehaviour
{
    private void OnEnable()
    {
        XInputManager.Instance.enableInput = true;
        XEventCenter.Instance.AddEventListener<XKeyInputInfo>(XEventType.E_KeyEvent, OnSpacePressed);
    }

    private void OnDisable()
    {
        XEventCenter.Instance.RemoveEventListener<XKeyInputInfo>(XEventType.E_KeyEvent, OnSpacePressed);
    }

    private void OnSpacePressed(XKeyInputInfo info)
    {
        if (info._keyCode != KeyCode.Space)
        {
            return;
        }

        if (info.inputState != XInputState.Press)
        {
            return;
        }

        Debug.Log(info._keyCode + "按下了");
    }
}