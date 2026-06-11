using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class XEventCenter : XSingletonCSharp<XEventCenter>
{
    private XEventCenter()
    {
    }

    //事件字典，持有所有事件名字和事件
    private Dictionary<string, UnityAction> _eventDict = new Dictionary<string, UnityAction>();

    //监听者用这个方法监听想要监听的事件，如果想要监听的事件在字典里没有，就创建该事件
    public void AddListener(string eventName, UnityAction action)
    {
        if (!_eventDict.ContainsKey(eventName))
        {
            _eventDict.Add(eventName, action);
        }
        else
        {
            _eventDict[eventName] += action;
        }
    }

    //监听者取消监听某个事件
    public void RemoveListener(string eventName, UnityAction action)
    {
        if (!_eventDict.ContainsKey(eventName))
        {
            Debug.LogError(eventName + "事件不存在");
            return;
        }

        _eventDict[eventName] -= action;

        
        if (_eventDict[eventName] == null)
        {
            _eventDict.Remove(eventName);
            return;
        }
    }

    //被监听者希望告诉监听者所用的函数
    public void EventTrigger(string eventName)
    {
        if (!_eventDict.ContainsKey(eventName))
        {
            return;
        }

        _eventDict[eventName]?.Invoke();
    }

    //清理一个事件的所有监听者
    public void Clear(string eventName)
    {
        if (!_eventDict.ContainsKey(eventName))
        {
            Debug.LogError("找不到对应的事件");
            return;
        }

        _eventDict.Remove(eventName);
    }

    //清理字典中的全部事件
    public void ClearAll()
    {
        _eventDict.Clear();
    }
}