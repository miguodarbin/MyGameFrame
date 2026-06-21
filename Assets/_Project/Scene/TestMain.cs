using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestMain : MonoBehaviour
{
    public Button changeReleaseKeyButton;
    public TextMeshProUGUI releaseKeyText;

    private void Awake()
    {
        InitInput();
    }

    private void InitInput()
    {
        var releaseFireBallKeyInfo = new XInputInfo(XInputInfo.KeyBoardOrMouse.Keyboard, XInputInfo.KeyState.Down, KeyCode.Space);
        XInputManager.Instance.AddOrChangeInputEvent(XEventType.E_Confirm, releaseFireBallKeyInfo);
        RefreshKeyMapping();
    }

    private void RefreshKeyMapping()
    {
        releaseKeyText.text = XInputManager.Instance.GetInputEventsKeyString(XEventType.E_Confirm);
    }

    private void OnEnable()
    {
        XInputManager.Instance.enableInputEvents = true;
        XEventCenter.Instance.AddEventListener(XEventType.E_Confirm, ReleaseFireball);
        changeReleaseKeyButton.onClick.AddListener(OnChangeReleaseKeyButtonClicked);
    }

    private void OnDisable()
    {
        XEventCenter.Instance.RemoveEventListener(XEventType.E_Confirm, ReleaseFireball);
        changeReleaseKeyButton.onClick.RemoveListener(OnChangeReleaseKeyButtonClicked);
    }

    private void ReleaseFireball()
    {
        Debug.Log("ReleaseFireball!!!");
    }

    public void OnChangeReleaseKeyButtonClicked()
    {
        releaseKeyText.text = "Please Enter Your Key";
        XInputManager.Instance.CheckTheNextInputKey((inputInfo) =>
        {
            XInputManager.Instance.AddOrChangeInputEvent(XEventType.E_Confirm, inputInfo);
            RefreshKeyMapping();
        });
    }
}