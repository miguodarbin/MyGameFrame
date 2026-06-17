/// <summary>
/// 【Controller基类】负责连接 View 和 Model。绑定 / 解绑 View 抛出的 UI 事件，处理用户操作后的业务流程，并驱动 View 根据 Model 数据刷新显示。
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c> BindEvents</c>：绑定UI事件</description>
/// </item>
/// <item>
/// <description><c>  UnbindEvents</c>：解绑UI事件</description>
/// </item>
/// </list>
/// </remarks>
public class XUIController<T> where T : XUIPanelView
{
    protected T Panel { get; private set; }

    private bool _isInit = false;

    /// 初始化 Controller，绑定 View，只调用一次
    public void Init(T view)
    {
        if (_isInit)
        {
            return;
        }

        Panel = view;
        _isInit = true;

        OnInit();
        BindEvents();
    }

    // Controller 初始化时调用。子类可以在这里初始化数据、缓存组件引用
    protected virtual void OnInit()
    {
    }

    //绑定 View 抛出的 UI 事件。比如按钮点击、Toggle变化、Slider变化。
    protected virtual void BindEvents()
    {
    }

    //解绑 View 抛出的 UI 事件。Dispose 时会自动调用。
    protected virtual void UnbindEvents()
    {
    }

    //面板显示后调用。适合刷新数据、请求当前状态、驱动 View 更新显示。
    public virtual void OnControllerShow()
    {
    }

    //面板隐藏前调用。适合暂停刷新、停止计时器、清理临时状态。
    public virtual void OnControllerHide()
    {
    }

    //Controller 被销毁前调用。释放 / 清理这个对象占用的东西。定要在这里解绑事件，避免 View 和 Controller 相互引用导致脏回调
    public virtual void Dispose()
    {
        if (!_isInit)
        {
            return;
        }

        UnbindEvents();

        Panel = null;
        _isInit = false;
    }
}