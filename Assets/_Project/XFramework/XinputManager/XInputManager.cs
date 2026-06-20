using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 定义输入的状态，是Down按下，还是Press按中，还是Up抬起
/// </summary>
public enum XInputState
{
    Down,
    Press,
    Up
}

public enum XHotKeyType
{
    Horizontal,
    Vertical
}


/// <summary>
/// 定义一个键盘输入的数据包，每次轮询到有键盘输入都会给监听者发这个数据包，告诉本次键盘输入是哪个键被点了，是按下还是按中还是抬起
/// </summary>
public struct XKeyInputInfo
{
    public XInputState inputState;
    public KeyCode _keyCode;
}

/// <summary>
/// 定义一个鼠标输入的数据包，每次轮询到有鼠标输入都会给监听者发这个数据包，告诉本次鼠标输入是哪个键被点了，是按下还是按中还是抬起
/// </summary>
public struct XMouseInputInfo
{
    public XInputState inputState;
    public int mouseKey; //0是左键，1是右键，2是滚轮键
}

/// <summary>
/// 定义一个热键输入的数据包，每次轮询到有热键输入都会给监听者发这个数据包，告诉本次热键输入是哪个热键，返回的值是多少
/// </summary>
public struct XHotKeyInputInfo
{
    public float inputValue;
    public XHotKeyType hotKeyType;
}


public class XInputManager : XSingletonCSharp<XInputManager>
{
    //其实这个输入管理器的原理就是通过这个脚本去轮询是否有输入，然后触发输入事件
    //这只是一个纯C#对象，需要被Unity相关的调一下，才会被初始化，进而去公共mono注册事件
    //所以说可以用一个开关，就是别人用之前，必须要改一下这里的开关，如果是第一次改，不仅逻辑上是打开输入检测了，而且也就顺便初始化这个对象了，不是第一次改，说明已经初始化了，也没事儿，这个输入检测反正有个开关也很符合逻辑
    //然后这个脚本似乎没有自己接入unity生命周期的方法，所以需要借助公共Mono模块，而且还要定义一些事件，而且这边的定义事件是全局事件，可以放到XEventType里

    public bool enableInput = false; //全局开关

    public bool enableKeyboard = true; //只控制键盘轮询检测的开关
    public bool enableMouse = true; //只控制鼠标轮询检测的开关
    public bool enableHotKey = false; //只控制热键轮训检查的开关


    private XInputManager()
    {
        //主要是这个XinputManager的单例对象生命周期只会比XMonoManager短，所以可以不用考虑在XMonoManager注销事件
        XMonoManager.Instance.OnUpdateAddListener(LoopCheckInputs);
    }

    private void LoopCheckInputs() //这个类主要逻辑
    {
        if (!enableInput) //全局开关 
        {
            return;
        }

        if (enableKeyboard) //=====================键盘检测相关=============================
        {
            JudgeKeyCode(KeyCode.Escape);
            JudgeKeyCode(KeyCode.Space);
        }

        if (enableMouse) //=====================鼠标检测相关=============================
        {
            JudgeMouse(0);
            JudgeMouse(1);
        }

        if (enableHotKey) //=====================热键检测相关=============================
        {
            JudgeHotKey("Horizontal", XHotKeyType.Horizontal);
            JudgeHotKey("Vertical", XHotKeyType.Vertical);
        }
    }

    //===================== 辅助方法 =====================
    private void JudgeKeyCode(KeyCode keyCode)
    {
        if (Input.GetKeyDown(keyCode))
        {
            var keyInputInfo = new XKeyInputInfo();
            keyInputInfo._keyCode = keyCode;
            keyInputInfo.inputState = XInputState.Down;
            XEventCenter.Instance.EventTrigger(XEventType.E_KeyEvent, keyInputInfo);
        }

        if (Input.GetKey(keyCode))
        {
            var keyInputInfo = new XKeyInputInfo();
            keyInputInfo._keyCode = keyCode;
            keyInputInfo.inputState = XInputState.Press;
            XEventCenter.Instance.EventTrigger(XEventType.E_KeyEvent, keyInputInfo);
        }

        if (Input.GetKeyUp(keyCode))
        {
            var keyInputInfo = new XKeyInputInfo();
            keyInputInfo._keyCode = keyCode;
            keyInputInfo.inputState = XInputState.Up;
            XEventCenter.Instance.EventTrigger(XEventType.E_KeyEvent, keyInputInfo);
        }
    }

    private void JudgeMouse(int mouseKey)
    {
        if (Input.GetMouseButtonDown(mouseKey))
        {
            var mouseInfo = new XMouseInputInfo();
            mouseInfo.mouseKey = mouseKey;
            mouseInfo.inputState = XInputState.Down;
            XEventCenter.Instance.EventTrigger(XEventType.E_MouseEvent, mouseInfo);
        }

        if (Input.GetMouseButton(mouseKey))
        {
            var mouseInfo = new XMouseInputInfo();
            mouseInfo.mouseKey = mouseKey;
            mouseInfo.inputState = XInputState.Press;
            XEventCenter.Instance.EventTrigger(XEventType.E_MouseEvent, mouseInfo);
        }

        if (Input.GetMouseButtonUp(mouseKey))
        {
            var mouseInfo = new XMouseInputInfo();
            mouseInfo.mouseKey = mouseKey;
            mouseInfo.inputState = XInputState.Up;
            XEventCenter.Instance.EventTrigger(XEventType.E_MouseEvent, mouseInfo);
        }
    }

    private void JudgeHotKey(string hotKeyName, XHotKeyType hotKeyType)
    {
        var hotKeyInputInfo = new XHotKeyInputInfo();
        hotKeyInputInfo.hotKeyType = hotKeyType;
        hotKeyInputInfo.inputValue = Input.GetAxis(hotKeyName);
        XEventCenter.Instance.EventTrigger<XHotKeyInputInfo>(XEventType.E_HotKey, hotKeyInputInfo);
    }
}