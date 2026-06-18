using DG.Tweening;
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
        PanelView.OnDraging.callback.AddListener(OnDragingArea);
        PanelView.OnEndDrag.callback.AddListener(OnDragAreaEndDrag);
    }

    protected override void UnSubscribeInteractionEvents()
    {
        PanelView.CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        PanelView.ResetButton.onClick.RemoveListener(OnResetButtonClicked);
        PanelView.AddButton.onClick.RemoveListener(OnAddButtonClicked);
        PanelView.SubButton.onClick.RemoveListener(OnSubButtonClicked);
        PanelView.OnDraging.callback.RemoveListener(OnDragingArea);
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


    //拖拽窗口逻辑——————————————————————————

    private Vector2 _totalOffset = Vector2.zero;
    private Vector2 _threshold = new Vector2(0.2f, 0.2f);

    private Tween _tween;


    public void OnDragingArea(BaseEventData eventData)
    {
        var data = eventData as PointerEventData;
        _totalOffset += data.delta;
        bool draging = Vector2.Distance(_totalOffset, _threshold) > 0.2f;
        RectTransform windowRect = PanelView.Window.transform as RectTransform;
        if (draging)
        {
            windowRect.anchoredPosition += data.delta;
        }
    }

    public void OnDragAreaEndDrag(BaseEventData eventData)
    {
        //先得到根物体，是mask和window的根
        var rootRect = PanelView.Root.transform as RectTransform;
        var maskRect = PanelView.Mask.transform as RectTransform;
        var windowRect = PanelView.Window.transform as RectTransform;

        //得到maskRect和windowRect在root坐标系下的宽高
        var maskSize = GetRectSizeInParent(maskRect, rootRect);
        var maskWidthInRoot = maskSize.x;
        var maskHeightInRoot = maskSize.y;
        var windowSize = GetRectSizeInParent(windowRect, rootRect);
        var windowWidthInRoot = windowSize.x;
        var windowHeightInRoot = windowSize.y;


        //再得到windowRect的anchoredPosition，也就是Root坐标系下，windowRect的Pivot距离参考锚点的偏移量
        var windowAnchoredPos = windowRect.anchoredPosition;

        //先定义一下anchoredPosition的X规则，mask的最左边+一半的window < anchoredPosition.x < mask的最右边-一半的window
        var minX = (-maskWidthInRoot / 2) + (windowWidthInRoot / 2);
        var maxX = (maskWidthInRoot / 2) - (windowWidthInRoot / 2);
        //在定义一下anchoredPosition的Y规则,mask的最下面+一半的window < anchoredPosition.y < mask的最上边-一半的window
        var minY = (-maskHeightInRoot / 2) + (windowHeightInRoot / 2);
        var maxY = (maskHeightInRoot / 2) - (windowHeightInRoot / 2);

        //夹紧
        windowAnchoredPos.x = Mathf.Clamp(windowAnchoredPos.x, minX, maxX);
        windowAnchoredPos.y = Mathf.Clamp(windowAnchoredPos.y, minY, maxY);

        //重新赋值
        _tween = windowRect.DOAnchorPos(windowAnchoredPos, 0.2f).SetEase(Ease.OutBack);
    }

    protected override void OnPanelControllerHide()
    {
        _tween?.Kill();
    }

    private Vector2 GetRectSizeInParent(RectTransform sonRect, RectTransform parentRect)
    {
        Vector3[] corners = new Vector3[4];
        sonRect.GetWorldCorners(corners);
        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] = parentRect.InverseTransformPoint(corners[i]);
            corners[i].z = 0;
        }

        var height = Vector2.Distance(corners[0], corners[1]);
        var width = Vector2.Distance(corners[1], corners[2]);
        return new Vector2(width, height);
    }


    //————————————————————————————————
}