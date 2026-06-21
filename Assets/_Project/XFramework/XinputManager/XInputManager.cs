using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum XHotKeyType
{
    Horizontal,
    Vertical
}

public struct XHotKeyInputInfo
{
    public float inputValue;
    public XHotKeyType hotKeyType;
}

/// <summary>
/// 外部用的话必须要先注册输入事件信息给InputManager，不能直接用EventCenter直接去调事件，因为inputmanager调用的事件是字典里的Key，而非直接是EventType
/// </summary>
public class XInputManager : XSingletonCSharp<XInputManager>
{
    public bool enableInputEvents = false; //全局输入检测开关，关掉则禁用所有输入事件

    public bool enableKeyInputEvents = true;
    public bool enableMouseInputEvents = true;
    public bool enableHotKey = false; //热键开关，默认不检测


    private XInputManager()
    {
        XMonoManager.Instance.OnUpdateAddListener(LoopCheckEvents);
    }

    private Dictionary<XEventType, XInputInfo>
        _inputEventsDict = new Dictionary<XEventType, XInputInfo>(); //字典管理了整个游戏中全部的输入事件的键位信息，外部将不再单独在某个脚本中用input相关API，而是注册输入事件到这个字典里


    //================================================== 对外接口 =============================================================
    public void AddOrChangeInputEvent(XEventType eventType, XInputInfo inputInfo) //提供给外部的注册输入事件，或者改变输入事件键位的方法
    {
        if (!_inputEventsDict.ContainsKey(eventType)) //如果字典中没有这个事件，说明是来注册输入事件的
        {
            _inputEventsDict.Add(eventType, inputInfo);
        }
        else //如果字典中有这个输入事件，说明是来改键位信息的
        {
            _inputEventsDict[eventType] = inputInfo;
        }
    }

    public void RemoveInputEvent(XEventType eventType) //提供给外部的移除输入事件
    {
        _inputEventsDict.Remove(eventType);
    }

    public string GetInputEventsKeyString(XEventType eventType) //获得某个输入事件的具体字符串类型的值
    {
        if (!_inputEventsDict.ContainsKey(eventType))
        {
            return string.Empty;
        }

        var inputEventInfo = _inputEventsDict[eventType];
        if (_inputEventsDict[eventType].keyBoardOrMouse == XInputInfo.KeyBoardOrMouse.Keyboard)
        {
            return inputEventInfo.keyCode.ToString();
        }
        else
        {
            return inputEventInfo.mouseID.ToString();
        }
    }

    public void CheckTheNextInputKey(UnityAction<XInputInfo> callback) //获得下一帧开始输入的信息，只检测一次
    {
        if (EventSystem.current != null) //检测开始，强制空选，防止空格之类UI系统自带的输入
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        XMonoManager.Instance.StartCoroutine(ReallyCheckTheNextInputKey(callback)); //XInputManager的生命周期按道理来说是永久的，不过确实可以提供一个停止检测的方法？？？？
    }


    //================================================== 核心循环 =============================================================
    private void LoopCheckEvents() //每一帧都会调用这个方法，循环检测输入事件的输入有没有被按下，如果按下了就触发输入事件
    {
        if (!enableInputEvents)
        {
            return;
        }

        foreach (var inputEventPair in _inputEventsDict)
        {
            var inputInfo = inputEventPair.Value;

            if (enableKeyInputEvents && inputInfo.keyBoardOrMouse == XInputInfo.KeyBoardOrMouse.Keyboard) //如果是键盘事件的话
            {
                CheckKeyboardInputEvent(inputInfo.keyState, inputInfo.keyCode, inputEventPair.Key);
            }
            else if (enableMouseInputEvents && inputInfo.keyBoardOrMouse == XInputInfo.KeyBoardOrMouse.Mouse) //如果是鼠标事件的话
            {
                CheckMouseInputEvent(inputInfo.keyState, inputInfo.mouseID, inputEventPair.Key);
            }
        }

        if (enableHotKey)
        {
            JudgeHotKey("Horizontal", XHotKeyType.Horizontal);
            JudgeHotKey("Vertical", XHotKeyType.Vertical);
        }
    }


    //================================================== 辅助方法 =============================================================
    private IEnumerator ReallyCheckTheNextInputKey(UnityAction<XInputInfo> callback)
    {
        bool oldEnableInputEvents = enableInputEvents; //缓存一下开关状态，反正检测的时候一定是要关着的
        enableInputEvents = false;
        yield return null; //下一帧开始检测
        XInputInfo inputInfo = null;

        var keyCodesArray = Enum.GetValues(typeof(KeyCode)); //先得到全部KeyCode数组
        KeyCode[] keyCodes = (KeyCode[])keyCodesArray;


        while (inputInfo == null) //开始循环检测输入，
        {
            //1.先检测鼠标，数据量比较少，万一检测到了鼠标，剩下一大堆键盘的就不用找了
            for (int i = 0; i < 3; i++)
            {
                if (Input.GetMouseButtonDown(i))
                {
                    inputInfo = new XInputInfo(XInputInfo.KeyBoardOrMouse.Mouse, XInputInfo.KeyState.Down, i);
                    break; //如果检测到了，break打破for，走完while里的剩下两个判断，也不会执行while里别的了，直接执行回调
                }
            }

            if (inputInfo == null) //如果检测到鼠标输入了，那键盘输入就不用找了
            {
                //2.再检测键盘
                foreach (var keyCode in keyCodes)
                {
                    if (Input.GetKeyDown(keyCode))
                    {
                        inputInfo = new XInputInfo(XInputInfo.KeyBoardOrMouse.Keyboard, XInputInfo.KeyState.Down, keyCode);
                        break; //如果检测到了，break打破foreach，走完while里的剩下一个判断，也不会执行while里别的了，直接执行回调，直接执行回调
                    }
                }
            }

            if (inputInfo == null) //如果这一帧什么都没检测到，那就下一帧再来跑检测
            {
                yield return null;
            }
        }

        callback?.Invoke(inputInfo);
        enableInputEvents = oldEnableInputEvents;

        if (EventSystem.current != null) //检测结束，强制空选，防止空格之类UI系统自带的输入
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }


    private void CheckKeyboardInputEvent(XInputInfo.KeyState keyState, KeyCode keyCode, XEventType eventType) //判断键盘输入事件的这次输入是否有被按下
    {
        if (keyState == XInputInfo.KeyState.Down)
        {
            if (Input.GetKeyDown(keyCode))
            {
                XEventCenter.Instance.EventTrigger(eventType);
            }
        }
        else if (keyState == XInputInfo.KeyState.Pressed)
        {
            if (Input.GetKey(keyCode))
            {
                XEventCenter.Instance.EventTrigger(eventType);
            }
        }
        else if (keyState == XInputInfo.KeyState.Up)
        {
            if (Input.GetKeyUp(keyCode))
            {
                XEventCenter.Instance.EventTrigger(eventType);
            }
        }
    }

    private void CheckMouseInputEvent(XInputInfo.KeyState keyState, int mouseID, XEventType eventType) //判断鼠标输入事件的这次输入是否有被按下
    {
        if (keyState == XInputInfo.KeyState.Down)
        {
            if (Input.GetMouseButtonDown(mouseID))
            {
                XEventCenter.Instance.EventTrigger(eventType);
            }
        }
        else if (keyState == XInputInfo.KeyState.Pressed)
        {
            if (Input.GetMouseButton(mouseID))
            {
                XEventCenter.Instance.EventTrigger(eventType);
            }
        }
        else if (keyState == XInputInfo.KeyState.Up)
        {
            if (Input.GetMouseButtonUp(mouseID))
            {
                XEventCenter.Instance.EventTrigger(eventType);
            }
        }
    }

    private void JudgeHotKey(string hotKeyName, XHotKeyType hotKeyType) //判断水平垂直输入
    {
        float value = Input.GetAxis(hotKeyName);

        // 没有明显输入时，不触发热键事件
        if (Mathf.Abs(value) < 0.01f)
        {
            return;
        }

        var hotKeyInputInfo = new XHotKeyInputInfo();
        hotKeyInputInfo.hotKeyType = hotKeyType;
        hotKeyInputInfo.inputValue = Input.GetAxis(hotKeyName);
        XEventCenter.Instance.EventTrigger<XHotKeyInputInfo>(XEventType.E_HotKey, hotKeyInputInfo);
    }
}