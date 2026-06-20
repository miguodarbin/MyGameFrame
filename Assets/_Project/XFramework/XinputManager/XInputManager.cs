using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 外部用的话必须要先注册输入事件信息给InputManager，不能直接用EventCenter直接去调事件，因为inputmanager调用的事件是字典里的Key，而非直接是EventType
/// </summary>
public class XInputManager : XSingletonCSharp<XInputManager>
{
    public bool enableInput = false; //全局输入检测开关，关掉则禁用所有输入事件
    //TODO:可能之后要做一下单独的开关？？

    private XInputManager()
    {
        XMonoManager.Instance.OnUpdateAddListener(LoopCheckEvents);
    }

    private Dictionary<XEventType, XInputInfo>
        _inputEventsDict = new Dictionary<XEventType, XInputInfo>(); //这个字典管理了整个游戏中全部的输入事件的键位信息，外部将不再单独在某个脚本中用input相关API，而是注册输入事件到这个字典里

    public void AddOrChangeInputEvent(XEventType eventType, XInputInfo InputInfo) //提供给外部的注册输入事件，或者改变输入事件键位的方法
    {
        if (!_inputEventsDict.ContainsKey(eventType)) //如果字典中没有这个事件，说明是来注册输入事件的
        {
            _inputEventsDict.Add(eventType, InputInfo);
        }
        else //如果字典中有这个输入事件，说明是来改键位信息的
        {
            _inputEventsDict[eventType] = InputInfo;
        }
    }

    public void RemoveInputEvent(XEventType eventType) //提供给外部的移除输入事件
    {
        _inputEventsDict.Remove(eventType);
    }

    //每一帧都会调用这个方法，循环检测输入事件的输入有没有被按下
    private void LoopCheckEvents()
    {
        if (!enableInput)
        {
            return;
        }

        foreach (var inputEventPair in _inputEventsDict)
        {
            var inputInfo = inputEventPair.Value;

            if (inputInfo.keyBoardOrMouse == XInputInfo.KeyBoardOrMouse.Keyboard) //如果是键盘事件的话
            {
                CheckKeyboardInputEvent(inputInfo.keyState, inputInfo.keyCode, inputEventPair.Key);
            }
            else //如果是鼠标事件的话
            {
                CheckMouseInputEvent(inputInfo.keyState, inputInfo.mouseID, inputEventPair.Key);
            }
        }
    }

    //判断键盘输入事件的这次输入是否有被按下
    private void CheckKeyboardInputEvent(XInputInfo.KeyState keyState, KeyCode keyCode, XEventType eventType)
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

    //判断鼠标输入事件的这次输入是否有被按下
    private void CheckMouseInputEvent(XInputInfo.KeyState keyState, int mouseID, XEventType eventType)
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
}