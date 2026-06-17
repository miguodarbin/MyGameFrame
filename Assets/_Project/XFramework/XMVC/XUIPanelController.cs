using UnityEngine;

/// <summary>
/// UI Controller 基类
/// 职责：
/// 1. 持有 View 和 Model
/// 2. 管理 Controller 生命周期
/// 3. 统一绑定 / 解绑 View 事件和 Model 事件
/// 4. 不直接写具体业务逻辑
/// </summary>
public abstract class XUIPanelController<TView, TModel> : MonoBehaviour
    where TView : XUIPanelView
    where TModel : new()
{
    //作为中间人，理应持有Model和View
    protected TView PanelView { get; private set; }
    protected TModel PanelModel { get; private set; }

    private bool _isInit = false; //应为初始化要放到Enable和 Disable 里面，所以要用这个标记一下，别反复初始化


    //初始化 Model 和 View的逻辑
    private void Init()
    {
        if (_isInit)
        {
            return;
        }

        PanelView = GetComponent<TView>();
        if (PanelView == null)
        {
            Debug.LogError($"{gameObject.name} 上没有挂载 {typeof(TView).Name}");
            return;
        }

        PanelModel = new TModel();

        _isInit = true;
    }

    //初始化时机
    protected  void Awake()
    {
        Init();
    }

    //订阅事件
    protected void OnEnable()
    {
        if (!_isInit)
        {
            return;
        }

        SubscribeInteractionChanges();
        SubscribeModelValueChanges();
        RefreshView(PanelModel);
    }

    //解绑事件
    protected void OnDisable()
    {
        if (!_isInit)
        {
            return;
        }

        UnSubscribeInteractionEvents();
        UnSubscribeModelValueChanges();
    }


    //作为控制层，负责监听 UI交互事件，并把交互事件期望发生的数据逻辑交给Model，面板逻辑交给UIManager，其他逻辑交给自己   
    protected abstract void SubscribeInteractionChanges();

    protected abstract void UnSubscribeInteractionEvents();


    protected abstract void SubscribeModelValueChanges();

    protected abstract void UnSubscribeModelValueChanges();


    //Controller必须提供刷新UI方法，用于Controller开启的时候刷新 UI
    protected abstract void RefreshView(TModel model);
}