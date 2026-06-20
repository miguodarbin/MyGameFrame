/// <summary>
/// 所有的事件名 都通过枚举来定义
/// </summary>
public enum XEventType
{
    E_Confirm,//确认事件，inputManager如果在检测输入中检测到了按键可以触发这个事件，就会触发这个事件，对应的，但凡在这个Confirm事件中添加过事件的监听者们的方法也都会被调用
    E_Cancel
}