using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class XUnityActionWrapperBase
{
}

public class XUnityActionWrapper<T> : XUnityActionWrapperBase
{
    public UnityAction<T> unityAction;
}

public class XUnityActionWrapper : XUnityActionWrapperBase
{
    public UnityAction unityAction;
}


/// <summary>
/// 全局事件中心
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c>AddEventListener&lt;T&gt;(eventName, action)</c>：监听带参数事件 </description>
/// </item>
/// <item>
/// <description><c>RemoveEventListener&lt;T&gt;(eventName, action)</c>：取消监听带参数事件 </description>
/// </item>
/// <item>
/// <description><c>AddEventListener(eventName, action)</c>：监听无参数事件 </description>
/// </item>
/// <item>
/// <description><c>RemoveEventListener(eventName, action)</c>：取消监听无参数事件 </description>
/// </item>
/// <item>
/// <description><c>EventTrigger&lt;T&gt;(eventName, param)</c>：触发带参数事件 </description>
/// </item>
/// <item>
/// <description><c>EventTrigger(eventName)</c>：触发无参数事件 </description>
/// </item>
/// <item>
/// <description><c>ClearEventListener(eventName)</c>：清理某一个事件的全部监听者 </description>
/// </item>
/// <item>
/// <description><c>ClearAllEvent()</c>：清理全部事件 </description>
/// </item>
/// </list>
/// </remarks>
/// <remarks>
/// 外部须知：
/// <list type="number">
///  <item>
/// 同一个事件名第一次注册时，会决定这个事件的参数类型 ! 后续 Add / Remove / Trigger 必须使用相同的参数类型 ！
/// </item>
///  <item>
/// 所有事件名都必须先注册到 public enum XEventType 中
/// </item>
/// </list>
/// </remarks>
public class XEventCenter : XSingletonCSharp<XEventCenter>
{
    private XEventCenter()
    {
    }


    //事件字典，持有所有事件名字和事件
    private Dictionary<XEventType, XUnityActionWrapperBase> _eventDict;


    public void AddEventListener<T>(XEventType eventName, UnityAction<T> action)
    {
        if (_eventDict == null)
        {
            _eventDict = new Dictionary<XEventType, XUnityActionWrapperBase>();
        }

        if (!_eventDict.ContainsKey(eventName))
        {
            //第一次来监听的时候，监听者实例懒注册一个带具体参数类型事件，并把这个事件封装成了 XUnityActionWrapperBase给到字典
            //之后每次用这个字典的时候，都必须要和第一次注册时传的具体参数类型一致，否则就会拿不到正确的事件
            var unityActionWrapper = new XUnityActionWrapper<T>();
            unityActionWrapper.unityAction = action;
            _eventDict.Add(eventName, unityActionWrapper);
        }
        else
        {
            var unityActionWrapper = (_eventDict[eventName] as XUnityActionWrapper<T>);
            if (unityActionWrapper == null)
            {
                Debug.LogError($"这次的{eventName}事件监听者要的参数，必须和注册事件时第一个监听者要的参数一致！同样的事件只能给到同类型的参数给到各个监听者！");
                return;
            }

            unityActionWrapper.unityAction += action;
        }
    }

    public void AddEventListener(XEventType eventName, UnityAction action)
    {
        if (_eventDict == null)
        {
            _eventDict = new Dictionary<XEventType, XUnityActionWrapperBase>();
        }

        if (!_eventDict.ContainsKey(eventName))
        {
            var unityActionWrapper = new XUnityActionWrapper();
            unityActionWrapper.unityAction += action;
            _eventDict.Add(eventName, unityActionWrapper);
        }
        else
        {
            var unityActionWrapper = _eventDict[eventName] as XUnityActionWrapper;
            if (unityActionWrapper == null)
            {
                Debug.LogError("未知错误");
                return;
            }

            unityActionWrapper.unityAction += action;
        }
    }


    //监听者取消监听某个事件
    public void RemoveEventListener<T>(XEventType eventName, UnityAction<T> action)
    {
        if (_eventDict == null || !_eventDict.ContainsKey(eventName))
        {
            Debug.LogError(eventName + "事件不存在");
            return;
        }

        //虽然逻辑上，某个监听者想取消监听某个事件，不用再指定所需要的参数类型了，但是为了能拿到正确的UnityAction，还是要传，没有这个参数类型，就打不开UnityAction的外壳
        var unityActionWrapper = (_eventDict[eventName] as XUnityActionWrapper<T>);
        if (unityActionWrapper == null)
        {
            Debug.LogError($"这次的{eventName}事件监听者要的参数，必须和注册事件时第一个监听者要的参数一致！同样的事件只能给到同类型的参数给到各个监听者！");
            return;
        }

        unityActionWrapper.unityAction -= action;


        if (unityActionWrapper.unityAction == null)
        {
            _eventDict.Remove(eventName);
        }
    }

    public void RemoveEventListener(XEventType eventName, UnityAction action)
    {
        if (_eventDict == null || !_eventDict.ContainsKey(eventName))
        {
            Debug.LogError(eventName + "事件不存在");
            return;
        }

        var unityActionWrapper = _eventDict[eventName] as XUnityActionWrapper;
        if (unityActionWrapper == null)
        {
            Debug.LogError("未知错误");
            return;
        }

        unityActionWrapper.unityAction -= action;

        if (unityActionWrapper.unityAction == null)
        {
            _eventDict.Remove(eventName);
        }
    }


    //被监听者希望告诉监听者所用的函数
    public void EventTrigger<T>(XEventType eventName, T param)
    {
        if (_eventDict == null || !_eventDict.ContainsKey(eventName))
        {
            return;
        }

        var unityActionWrapper = (_eventDict[eventName] as XUnityActionWrapper<T>);
        if (unityActionWrapper == null)
        {
            Debug.LogError($"{typeof(T).Name}不是{eventName}这个事件监听者所要的参数！！");
            return;
        }

        unityActionWrapper.unityAction?.Invoke(param);
    }

    public void EventTrigger(XEventType eventName)
    {
        if (_eventDict == null || !_eventDict.ContainsKey(eventName))
        {
            return;
        }

        var unityActionWrapper = (_eventDict[eventName] as XUnityActionWrapper);
        if (unityActionWrapper == null)
        {
            Debug.LogError("未知错误");
            return;
        }

        unityActionWrapper.unityAction?.Invoke();
    }

    //清理一个事件的所有监听者
    public void ClearEventListener(XEventType eventName)
    {
        if (_eventDict == null || !_eventDict.ContainsKey(eventName))
        {
            return;
        }

        _eventDict.Remove(eventName);
    }

    //清理字典中的全部事件
    public void ClearAllEvent()
    {
        if (_eventDict == null)
        {
            return;
        }

        _eventDict.Clear();
    }
}