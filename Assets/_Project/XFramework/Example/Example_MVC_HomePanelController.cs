using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example_MVC_HomePanelController : XUIPanelController<Example_MVC_HomePanelView, Example_MVC_HomePanelModel>
{
    protected override void SubscribeInteractionChanges()
    {
        PanelView.CountPanelButton.onClick.AddListener(OnCountPanelButtonClick);
    }

    protected override void UnSubscribeInteractionEvents()
    {
        PanelView.CountPanelButton.onClick.RemoveListener(OnCountPanelButtonClick);
    }

    protected override void SubscribeModelValueChanges()
    {
    }

    protected override void UnSubscribeModelValueChanges()
    {
    }

    protected override void RefreshView(Example_MVC_HomePanelModel model)
    {
    }


    public void OnCountPanelButtonClick()
    {
        XUIManager.Instance.ShowPanel<Example_MVC_CountPanelView>(XCustomUILayer.E_Top);
    }
}