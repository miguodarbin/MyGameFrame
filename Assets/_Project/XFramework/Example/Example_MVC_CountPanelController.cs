
public class Example_MVC_CountPanelController : XUIPanelController<Example_MVC_CountPanelView, Example_MVC_CountModel>
{
    protected override void SubscribeInteractionChanges()
    {
        PanelView.CloseButton.onClick.AddListener(OnCloseButtonClicked);
        PanelView.ResetButton.onClick.AddListener(OnResetButtonClicked);
        PanelView.AddButton.onClick.AddListener(OnAddButtonClicked);
        PanelView.SubButton.onClick.AddListener(OnSubButtonClicked);
    }

    protected override void UnSubscribeInteractionEvents()
    {
        PanelView.CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        PanelView.ResetButton.onClick.RemoveListener(OnResetButtonClicked);
        PanelView.AddButton.onClick.RemoveListener(OnAddButtonClicked);
        PanelView.SubButton.onClick.RemoveListener(OnSubButtonClicked);
    }

    protected override void SubscribeModelValueChanges()
    {
        PanelModel.onCountChanged += RefreshView;
    }

    protected override void UnSubscribeModelValueChanges()
    {
        PanelModel.onCountChanged -= RefreshView;
    }

    protected override void RefreshView(Example_MVC_CountModel model)
    {
        PanelView.RefreshUI(model);
    }


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
}