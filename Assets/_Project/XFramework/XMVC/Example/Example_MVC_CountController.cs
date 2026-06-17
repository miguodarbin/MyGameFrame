using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Example_MVC_CountController : MonoBehaviour
{
    //作为中间人，理应持有Model和View
    private Example_MVC_CountModel _countModel;
    private Example_MVC_CountPanelView _countPanelView;


    //初始化Model 和 View
    private void Awake()
    {
        _countModel = new Example_MVC_CountModel();
        _countPanelView = GetComponent<XUIPanelView>() as Example_MVC_CountPanelView;
        BindInteractiveEvents();
    }

    //订阅事件
    private void OnEnable()
    {
        _countModel.onCountChanged += OnModelValueChanged;
    }

    private void OnDisable()
    {
        _countModel.onCountChanged -= OnModelValueChanged;
    }

    private void Start()
    {
        _countPanelView.RefreshUI(_countModel);
    }

    //作为中间人，传递 Model的修改，以更新UI
    private void OnModelValueChanged(Example_MVC_CountModel model)
    {
        _countPanelView.RefreshUI(model);
    }

    //作为控制层，负责监听UI交互事件，并把交互事件期望发生的数据逻辑交给Model，面板逻辑交给UIManager，其他逻辑交给自己
    public void BindInteractiveEvents()
    {
        _countPanelView.CloseButton.onClick.AddListener(OnCloseButtonClicked);
        _countPanelView.ResetButton.onClick.AddListener(OnResetButtonClicked);
        _countPanelView.AddButton.onClick.AddListener(OnAddButtonClicked);
        _countPanelView.SubButton.onClick.AddListener(OnSubButtonClicked);
    }

    public void OnCloseButtonClicked()
    {
        XUIManager.Instance.HidePanel<Example_MVC_CountPanelView>();
    }

    public void OnResetButtonClicked()
    {
        _countModel.ResetCount();
    }

    public void OnAddButtonClicked()
    {
        _countModel.AddCount();
    }

    public void OnSubButtonClicked()
    {
        _countModel.SubCount();
    }
}