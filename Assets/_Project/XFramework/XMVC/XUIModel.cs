using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 【 Model 基类】子类需要自己定义数据。基类负责保存和维护界面所需的数据状态，并在数据发生变化时通知外部。Model 不持有 UI 控件，不操作 GameObject，不负责界面刷新。
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c> NotifyModelChanged</c>：子类调用这个来通知数据变化</description>
/// </item>
/// <item>
/// <description><c> OnModelChanged</c>：Controller订阅这个数据变化的事件</description>
/// </item>
/// </list>
/// </remarks>
public class XUIModel
{
    //Model 数据发生变化时通知外部。一般由 Controller 监听，然后刷新 View。
    public event UnityAction OnModelChanged;

    private bool _isInit = false;

    //初始化 Model，只调用一次。
    public void Init()
    {
        if (_isInit)
        {
            return;
        }

        _isInit = true;
        OnInit();
    }

    //子类重写初始化数据
    protected virtual void OnInit()
    {
    }


    /// 通知数据变化。子类改完数据后调用它
    protected void NotifyModelChanged()
    {
        OnModelChanged?.Invoke();
    }

    //释放 Model
    public virtual void Dispose()
    {
        OnModelChanged = null;
        _isInit = false;
    }
}