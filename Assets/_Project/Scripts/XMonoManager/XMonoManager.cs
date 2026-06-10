using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共 Mono 模块管理器
/// </summary>
public class XMonoManager : XSingletonAutoMono<XMonoManager>
{
    private event UnityAction _onUpdate;
    private event UnityAction _onFixedUpdate;
    private event UnityAction _onLateUpdate;
    
    private void Update()
    {
        _onUpdate?.Invoke();
    }

    private void FixedUpdate()
    {
        _onFixedUpdate?.Invoke();
    }

    private void LateUpdate()
    {
        _onLateUpdate?.Invoke();
    }
    
    //公开的订阅事件、退订时间接口⬇
    public void OnUpdateAddListener(UnityAction action)
    {
        _onUpdate += action;
    }

    public void OnFixedUpdateAddListener(UnityAction action)
    {
        _onFixedUpdate += action;
    }

    public void OnLateUpdateAddListener(UnityAction action)
    {
        _onLateUpdate += action;
    }

    public void OnUpdateRemoveListener(UnityAction action)
    {
        
        _onUpdate -= action;
    }

    public void OnFixedUpdateRemoveListener(UnityAction action)
    {
        _onFixedUpdate -= action;
    }

    public void OnLateUpdateRemoveListener(UnityAction action)
    {
        _onLateUpdate -= action;
    }
}