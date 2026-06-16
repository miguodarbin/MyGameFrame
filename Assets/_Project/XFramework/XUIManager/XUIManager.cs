using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

//注意主摄像机的就不要渲染UI了
//每个项目都需要去调一下Canvas Scaler组件里的参考分辨率和 Screen Match Mode
//所有面板的脚本名字必须要和面板GameObject的名字一致！
//panel是组件，不是GameObject！
public enum XCustomUILayer
{
    E_Bottom,
    E_Middle,
    E_Top,
    E_System
}

public class XUIManager : XSingletonCSharp<XUIManager>
{
    //缓存的关键组件，会自动创建
    private EventSystem _eventSystem;
    private Camera _uiCamera;
    private Canvas _canvas;


    //缓存的Canvas里的自定义层级，会自动获取
    private Transform bottomLayer;
    private Transform middleLayer;
    private Transform topLayer;
    private Transform systemLayer;


    //在第一次访问到UIManager类的时候，就初始化的时候就创建出UI相关的GameObject
    private XUIManager()
    {
        var eventSystemPrefab = XResourcesManager.Instance.LoadAsset<GameObject>("UIPrefabs/EventSystem");
        _eventSystem = Object.Instantiate(eventSystemPrefab).GetComponent<EventSystem>();
        Object.DontDestroyOnLoad(_eventSystem.gameObject);

        var uiCameraPrefab = XResourcesManager.Instance.LoadAsset<GameObject>("UIPrefabs/UICamera");
        _uiCamera = Object.Instantiate(uiCameraPrefab).GetComponent<Camera>();
        Object.DontDestroyOnLoad(_uiCamera.gameObject);

        var canvasPrefab = XResourcesManager.Instance.LoadAsset<GameObject>("UIPrefabs/Canvas");
        _canvas = Object.Instantiate(canvasPrefab).GetComponent<Canvas>();
        _canvas.worldCamera = _uiCamera;
        Object.DontDestroyOnLoad(_canvas.gameObject);

        bottomLayer = _canvas.transform.Find("Bottom");
        middleLayer = _canvas.transform.Find("Middle");
        topLayer = _canvas.transform.Find("Top");
        systemLayer = _canvas.transform.Find("System");

        if (bottomLayer == null || middleLayer == null || topLayer == null || systemLayer == null)
        {
            Debug.LogError("Canvas 层级节点缺失，请检查 Bottom / Middle / Top / System 是否存在且名字正确");
        }
    }


    //给外部去获得自定义UI层
    public Transform GetCustomUILayer(XCustomUILayer uiLayer)
    {
        switch (uiLayer)
        {
            case XCustomUILayer.E_Bottom:
                return bottomLayer;

            case XCustomUILayer.E_Middle:
                return middleLayer;

            case XCustomUILayer.E_Top:
                return topLayer;

            case XCustomUILayer.E_System:
                return systemLayer;
            default:
                return null;
        }
    }


    //管理全部Panel的字典容器,注意！所有面板的脚本名字必须要和面板GameObject的名字一致！
    private Dictionary<Type, XUIPanel> _uiPanels = new Dictionary<Type, XUIPanel>();

    //显示面板 注意！所有面板预设体都放在Assets\Editor\ArtRes\ui_prefab里！
    //callback负责加载完panel之后，把panel给到外部
    public void ShowPanel<T>(XCustomUILayer layer, UnityAction<T> callback = null, bool isSync = false) where T : XUIPanel
    {
        var panelType = typeof(T);

        var layerTransform = GetCustomUILayer(layer);
        if (layerTransform == null)
        {
            layerTransform = systemLayer;
            Debug.LogError("传入Layer有误请检查!");
        }

        if (_uiPanels.ContainsKey(panelType)) //字典中有这个panel类型的话
        {
            var panel = _uiPanels[panelType] as T;
            //这里应该不可能转换失败，应为都是按T的type存的
            panel?.transform.SetParent(layerTransform, false);
            panel?.ShowMe();
            callback?.Invoke(panel);
        }
        else //如果字典里没有这个panel类型的话，那就需要去加载
        {
            XABUnifiedManager.Instance.LoadAsset<GameObject>("ui_prefab", panelType.Name, (result) =>
            {
                var panelObj = Object.Instantiate(result, layerTransform, false);
                var panel = panelObj.GetComponent<T>();
                if (panel == null)
                {
                    Debug.LogError($"面板预设体 {panelType.Name} 上没有挂载 {panelType.Name} 脚本");
                    Object.Destroy(panelObj);
                    return;
                }

                panel.ShowMe();
                _uiPanels.Add(panelType, panel);
                callback?.Invoke(panel);
            }, isSync);
        }
    }


    //隐藏并删除面板
    public void HidePanel<T>() where T : XUIPanel
    {
        var panelType = typeof(T);
        if (!_uiPanels.ContainsKey(panelType))
        {
            Debug.LogError("还没有显示过该面板,无法隐藏");
            return;
        }

        var panelBase = _uiPanels[panelType];
        panelBase.HideMe();
        Object.Destroy(panelBase.gameObject);
        _uiPanels.Remove(panelType);
    }


    //获得面板
    public T GetPanel<T>() where T : XUIPanel
    {
        var panelType = typeof(T);
        if (!_uiPanels.ContainsKey(panelType))
        {
            Debug.LogError("还没有显示过该面板，无法获得");
            return null;
        }

        return _uiPanels[panelType] as T;
    }
}