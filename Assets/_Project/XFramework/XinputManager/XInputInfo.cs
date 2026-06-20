using UnityEngine;

/// <summary>
/// 这是一个输入事件 所对应的输入信息，包含了要触发本次输入事件所需要的输入信息，只有满足了输入信息，这个输入事件才会被触发
/// </summary>
public class XInputInfo
{
    public enum KeyBoardOrMouse //定义一个类，专门来存这个事件到底是需要键盘输入还是鼠标输入
    {
        Keyboard,
        Mouse
    }

    public enum KeyState //定义一个类，专门来存这个事件到底是需要按下、按中、还是抬起触发
    {
        Down,
        Pressed,
        Up
    }

    public KeyBoardOrMouse keyBoardOrMouse; //这个字段主要是方便外部做判断的，看看这个事件的需要的输入信息是键盘的还是鼠标的
    public KeyState keyState; //这个字段是外部需要用来判断，这个事件是需要按下、按中、还是抬起触发
    public KeyCode keyCode; //具体赋值，是键盘输入的话对这个字段赋值
    public int mouseID; //具体赋值，是鼠标的话对这个字段赋值

    public XInputInfo(KeyBoardOrMouse keyBoardOrMouse, KeyState keyState, KeyCode keyCode) //键盘事件用这个构造函数
    {
        this.keyBoardOrMouse = keyBoardOrMouse;
        this.keyState = keyState;
        this.keyCode = keyCode;
        this.mouseID = -999;
    }

    public XInputInfo(KeyBoardOrMouse keyBoardOrMouse, KeyState keyState, int mouseID) //鼠标事件用这个构造函数
    {
        this.keyBoardOrMouse = keyBoardOrMouse;
        this.keyState = keyState;
        this.mouseID = mouseID;
        this.keyCode = KeyCode.None;
    }
}