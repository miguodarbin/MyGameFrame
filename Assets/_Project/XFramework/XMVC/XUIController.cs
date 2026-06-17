using UnityEngine;

/// <summary>
/// 【Controller基类】
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
/// <item>
/// <description>类的泛型占位符T指的是这个Controller是哪个Panel的Controller，子类必须实现这个T，表明自己是哪个Panel的Controller</description>
/// </item>
/// </list>
/// </remarks>
public abstract class XUIController<TPanelView, TModel> : MonoBehaviour
    where TPanelView : XUIPanelView
    where TModel : XUIModel, new()
{
    protected TPanelView PanelView { get; private set; }

    private TModel _model;

    protected TModel PanelModel
    {
        get
        {
            if (_model == null)
            {
                _model = new TModel();
            }

            return _model;
        }
    }
}