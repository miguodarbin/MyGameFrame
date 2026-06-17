using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 【 Model 基类】 Model 不持有 UI 控件，不操作 GameObject，不负责界面刷新。
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
/// /// <item>
/// <description><c> 子类需要自己定义数据</c>：</description>
/// </item>
/// /// <item>
/// <description><c> 子类需要自己定义数据处理规则</c>：</description>
/// </item>
/// /// <item>
/// <description><c> 子类必须实现初始化</c>：</description>
/// </item>
/// </list>
/// </remarks>
public abstract class XUIModel
{
    //Model 数据发生变化时通知外部。一般由 Controller 监听，然后刷新 View。
    public event UnityAction OnModelChanged;

    //子类重写初始化数据
    protected abstract void OnInit();
    

    /// 通知数据变化。子类改完数据后调用它
    protected void NotifyModelChanged()
    {
        OnModelChanged?.Invoke();
    }
}