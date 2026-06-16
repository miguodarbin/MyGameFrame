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

public class XPanelLoadInfoBase
{
}

public class XPanelLoadInfo<T> : XPanelLoadInfoBase where T : XUIPanel
{
    public T panel = null;
    public UnityAction<T> callback = null;
    public bool isNeedHide = false;
}


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
    private Dictionary<Type, XPanelLoadInfoBase> _uiPanels = new Dictionary<Type, XPanelLoadInfoBase>();


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
            var panelLoadInfo = _uiPanels[panelType] as XPanelLoadInfo<T>;
            if (panelLoadInfo == null)
            {
                Debug.LogError("传入的类型和第一次异步加载时的类型不一致");
                return;
            }

            var panel = panelLoadInfo.panel;

            if (panel != null) //有值的话，那就正常处理这次请求，把位置设置好，把回调也掉了
            {
                //如果隐藏了没从字典移出，那之后Show请求都会带着脏的isNeedHide，所以不管咋样，这里重置isNeedHide总没错
                panelLoadInfo.isNeedHide = false;
                panel.transform.SetParent(layerTransform, false);
                panel.ShowMe();
                callback?.Invoke(panel);
            }
            else //没值的话，就把这次异步请求的回调拿下，等异步加载完调
            {
                panelLoadInfo.isNeedHide = false;
                panelLoadInfo.callback += callback;
            }
        }
        else //如果字典里没有这个panel类型的话，那就需要去加载
        {
            //先创建加载信息，并把第一次创建异步加载的这次的回调加入总回调中
            var panelLoadInfo = new XPanelLoadInfo<T>();
            panelLoadInfo.callback += callback;
            _uiPanels.Add(panelType, panelLoadInfo);

            //然后开始异步加载
            XABUnifiedManager.Instance.LoadAsset<GameObject>("ui_prefab", panelType.Name, (result) =>
            {
                //看看有没有把资源加载成功
                if (result == null)
                {
                    Debug.LogError($"UI预设体 {panelType.Name} 加载失败");
                    _uiPanels.Remove(panelType);
                    return;
                }
                
                //等panel加载完毕，就要出处理 隐藏有隐藏标记的panel，而且还要把这次异步加载请求的回调也删掉，要不然后面继续执行回调就不符合逻辑
                if (panelLoadInfo.isNeedHide)
                {
                    panelLoadInfo.callback = null;
                    _uiPanels.Remove(panelType);
                    return;
                }

                //加载完之后先实例化panel，顺便把位置和层级设置好
                var panelObj = Object.Instantiate(result, layerTransform, false);
                var panel = panelObj.GetComponent<T>();
                if (panel == null)
                {
                    Debug.LogError($"面板预设体 {panelType.Name} 上没有挂载 {panelType.Name} 脚本");
                    Object.Destroy(panelObj);
                    _uiPanels.Remove(panelType);
                    return;
                }

                //加载完毕，就记录到字典的value里面
                panelLoadInfo.panel = panel;


                //然后把面板显示出来
                panel.ShowMe();

                //然后收一下尾，触发回调、置空

                panelLoadInfo.callback?.Invoke(panel);
                panelLoadInfo.callback = null;
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

        var panelLoadInfo = _uiPanels[panelType] as XPanelLoadInfo<T>;
        if (panelLoadInfo == null)
        {
            Debug.LogError("传入的类型和第一次异步加载时的类型不一致");
            return;
        }

        if (panelLoadInfo.panel == null) //有记录了，但还没值,等加载完再处理Hide逻辑，这里就只把加载信息里的Hide标记打开，到时候加载完的回调判断一下，如果有Hide标记，就帮我这里完成Hide逻辑
        {
            panelLoadInfo.isNeedHide = true;
            panelLoadInfo.callback = null;
        }
        else //有记录了，也有值了
        {
            panelLoadInfo.panel.HideMe();
            Object.Destroy(panelLoadInfo.panel.gameObject);
            _uiPanels.Remove(panelType);
        }
    }


    //获得面板,因为获得的时候可能会异步加载，但是外部拿面板可能也不知道在不在加载中，那就统一都用回调通知外部，所以外部需要拿一个回调函数来收通知
    public void GetPanel<T>(UnityAction<T> callback) where T : XUIPanel
    {
        var panelType = typeof(T);
        if (!_uiPanels.ContainsKey(panelType))
        {
            return;
        }

        var panelLoadInfo = _uiPanels[panelType] as XPanelLoadInfo<T>;
        if (panelLoadInfo == null)
        {
            Debug.LogError("传入的类型和第一次异步加载时的类型不一致");
            return;
        }

        if (panelLoadInfo.panel == null)
        {
            if (panelLoadInfo.isNeedHide)
            {
                return;
            }
            panelLoadInfo.callback += callback; //还没加载完，就让这次请求的回调交给加载完一起调用吧，反正加载完一起调用的那个总回调也是提供这个panel面板对象
        }
        else
        {
            if (panelLoadInfo.isNeedHide)
            {
                return;
            }

            callback?.Invoke(panelLoadInfo.panel);
        }
    }
}