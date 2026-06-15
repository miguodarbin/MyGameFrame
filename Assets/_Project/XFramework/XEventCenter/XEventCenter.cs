using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#region 思路

//我想做什么？我想让观察者每次执行回调的时候，都能拿到被观察者送出来的数据
//所以说回调方法的签名里就要有一个想要的参数类型
//所以添加回调的 AddListener就应该是泛型方法，调用AddListener的时候才指定UnityAction这个委托里面到底装的什么参数列表的函数
//所以说事件的字典Value也应该是 UnityAction<T>，要不然无法添加AddListener这个泛型方法给到的委托
//但问题来了，如果字典Value也应该是 UnityAction<T>，就强迫着XEventCenter是个泛型类了，但这不对了，如果XEventCenter是个泛型类，就不符合事件中心的单例特性了
//所以要像个桥接字典的Value和AddListener之间的“中间态”
//那我就不让这个字典直接存UnityAction<T>了，我搞一个Base类，让UnityAction<T>继承Base类，然字典的Value存Base，然后用Base转成UnityAction，等等，UnityAction继承Base类？
//虽然UnityAction属于引用类型的delegate，但是delegate不能给他写继承，
//那还有别的办法能实现把UnityAction<T> action变成某个通用的东西放给Dictionary的Value吗？刚才考虑的继承一个父类是不行的
//那我用一个泛型类装这个泛型委托呢？把泛型委托当做泛型类的字段，然后点出这个泛型委托
//但这只是把泛型委托包装成了泛型类，诶？对啊，都变成了泛型类，那就可以用刚才不行的基类转子类了
//也就是这样的，核心泛型委托作为字段，包上一层泛型类，然后再包上一层通用Base类，然后这个Base类给字典，但是我这么推理只是为了消除报错，具体监听者和被监听者怎么流转数据我还是有点懵
//先试试吧
//emmm,似乎有点理解？被观察者和观察者要提前商量好，才不会出错，具体来说就是监听者通过AddListener<T>封装了一个指定类型的XUnityActionWrapperBase基类存到字典里了，
//调用EventTrigger<T>的时候必须把它准确的把当时AddListener<T>封装了一个指定类型不差的填进去，有点像XUnityActionWrapperBase是个加密包，T就是密码，然后EventTrigger<T>和AddListener<T>必须是同样的密码才能正常工作
//然后字典就相当于存的是key是string，value是加密包的一个字点了

#endregion

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
/// 监听者和被监听者如果想传递数据包，可以用泛型版本的public方法
/// </summary>
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