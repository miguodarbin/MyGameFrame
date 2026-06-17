using UnityEngine;
using UnityEngine.EventSystems;

public class Example_MVC_CountPanelController : XUIPanelController<Example_MVC_CountPanelView, Example_MVC_CountModel>
{
    
    //刷新UI方法
    protected override void RefreshView(Example_MVC_CountModel model)
    {
        PanelView.RefreshUI(model);
    }


    //监听 UI交互——————————————————————————
    protected override void SubscribeInteractionChanges()
    {
        PanelView.CloseButton.onClick.AddListener(OnCloseButtonClicked);
        PanelView.ResetButton.onClick.AddListener(OnResetButtonClicked);
        PanelView.AddButton.onClick.AddListener(OnAddButtonClicked);
        PanelView.SubButton.onClick.AddListener(OnSubButtonClicked);
        PanelView.OnBeginDrag.callback.AddListener(OnDragAreaBeginDrag);
        PanelView.OnEndDrag.callback.AddListener(OnDragAreaEndDrag);
    }

    protected override void UnSubscribeInteractionEvents()
    {
        PanelView.CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        PanelView.ResetButton.onClick.RemoveListener(OnResetButtonClicked);
        PanelView.AddButton.onClick.RemoveListener(OnAddButtonClicked);
        PanelView.SubButton.onClick.RemoveListener(OnSubButtonClicked);
        PanelView.OnBeginDrag.callback.RemoveListener(OnDragAreaBeginDrag);
        PanelView.OnEndDrag.callback.RemoveListener(OnDragAreaEndDrag);
    }
    //————————————————————————————————


    //订阅 Model——————————————————————————
    protected override void SubscribeModelValueChanges()
    {
        PanelModel.onCountChanged += RefreshView;
    }

    protected override void UnSubscribeModelValueChanges()
    {
        PanelModel.onCountChanged -= RefreshView;
    }
    //————————————————————————————————


    //交互回调——————————————————————————

    public void OnCloseButtonClicked()
    {
        XUIManager.Instance.HidePanel<Example_MVC_CountPanelView>();
    }

    public void OnResetButtonClicked()
    {
        PanelModel.ResetCount();
    }

    public void OnAddButtonClicked()
    {
        PanelModel.AddCount();
    }

    public void OnSubButtonClicked()
    {
        PanelModel.SubCount();
    }

    public void OnDragAreaBeginDrag(BaseEventData eventData)
    {
        Debug.Log("OnDragAreaBeginDrag");
    }

    public void OnDragAreaEndDrag(BaseEventData eventData)
    {
        Debug.Log("OnDragAreaEndDrag");
    }

    //————————————————————————————————
}