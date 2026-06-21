using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 公共 Mono 生命周期入口
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c>XMonoManager.Instance.OnUpdateAddListener</c>：注册方法给MonoManager生命周期循环 </description>
/// </item>
/// <item>
/// <description><c>XMonoManager.Instance.OnUpdateRemoveListener(action)</c>：注销方法给MonoManager生命周期循环 </description>
/// </item>
/// </list>
/// </remarks>
/// <remarks>
/// 外部须知：
/// <list type="number">
///  <item>
/// 订阅和退订必须传同一个方法引用； Lambda 不适合退订
/// </item>
///  <item>
/// 也可以用这个类代替其他Mono去执行他们的生命周期函数
/// </item>
///  <item>
/// 也提供代理开启和关闭协程
/// </item>
/// </list>
/// </remarks>
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