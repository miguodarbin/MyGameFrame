using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

/// <summary>
/// 【UI面板(View)基类】持有和管理当前面板下的 UI 控件，子类可以重写 Panel显示或关闭时的 显示相关逻辑 / 表现逻辑，不写业务逻辑
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c> GetUIControl&lt;T&gt;()</c>：获取面板下的某个UI控件，泛型参数是控件类型</description>
/// </item>
/// <item>
/// <description><c> OnPanelShow()</c>：面板被显示时调用：只做View自己的刷新、动画、状态重置</description>
/// </item>
/// <item>
///  <description><c> OnPanelHide()</c>：面板被隐藏时调用：只做View自己的清理、动画、状态重置</description>
/// </item>
/// <item>
/// <description>想被这个Panel管理的UI控件，必须改成唯一且非默认的GameObject名字</description>
/// </item>
/// <item>
/// <description>子类如果重写 Awake，必须先调用 base.Awake()，否则不会自动收集UI控件！！</description>
/// </item>
/// <item>
/// <description>子类需要自己去声明公开控件，以让Controller订阅交互事件！！</description>
/// </item>
/// </list>
/// </remarks>
public class XUIPanelView : MonoBehaviour
{
    //Panel下，每个UI控件所对应的【GameObject名字】都会记录到这个字典中，一个GameObject名字只会对应一个UI控件
    protected Dictionary<string, UIBehaviour> _uiDict = new Dictionary<string, UIBehaviour>();


    //Panel下，如果一个UI控件的的GameObject的名字是以下的默认名，则认为不需要管理，因此纳入管理的UI控件需要改名字
    private List<String> _defaultUINames = new List<string>()
    {
        "Image", "Text (TMP)", "RawImage", "Label", "Background", "Toggle", "Slider", "Fill", "Handle", "Scroll View", "Viewport", "Scrollbar Horizontal",
        "Scrollbar Vertical", "Dropdown", "Button", "Arrow", "Template", "Scrollbar", "InputField (TMP)", "Placeholder", "Text Area", "Text (Legacy)",
        "Button (Legacy)", "Dropdown (Legacy)", "InputField (Legacy)"
    };


    //Panel下，Awake的时候就去读取自己所有继承了UIBehaviour的子控件,而且也要调用子类写的初始化
    //如果一个同名GameObject有多个UI控件，按这里的先后顺序决定绑定优先级，决定这个GameObject的Name用哪个UI控件对应为它的Value
    protected virtual void Awake()
    {
        FindAllUIControls<Button>(); //按钮
        FindAllUIControls<Toggle>(); //复选框
        FindAllUIControls<Slider>(); //拖动条，进度条
        FindAllUIControls<ScrollRect>(); //滚动视图
        FindAllUIControls<InputField>(); //输入框
        FindAllUIControls<ToggleGroup>(); //单选框
        FindAllUIControls<TextMeshProUGUI>(); //文字
        FindAllUIControls<Image>(); //图片
        FindAllUIControls<TMP_InputField>(); //TMP输入框
        FindAllUIControls<TMP_Dropdown>(); //TMP下拉菜单
        FindAllUIControls<ContentSizeFitter>(); //内容大小适配器
        FindAllUIControls<AspectRatioFitter>(); //宽高比适配器
        FindAllUIControls<LayoutGroup>(); //布局组件
        FindAllUIControls<LayoutElement>(); //布局元素
        FindAllUIControls<RectMask2D>(); //遮罩
        FindAllUIControls<Mask>(); //遮罩
        FindAllUIControls<Scrollbar>(); //滚动条
        FindAllUIControls<Dropdown>(); //下拉菜单
        FindAllUIControls<Text>(); //过时的文字

        InitPanelView();
    }


    //通过UI类型，找到某一类的全部儿子 UI控件，儿子的儿子....也能找
    private void FindAllUIControls<T>() where T : UIBehaviour
    {
        var controls = GetComponentsInChildren<T>(true); //参数填true包含查找未激活的
        foreach (var control in controls)
        {
            string controlName = control.gameObject.name;
            //对于儿子名字中，是默认名字的不进行管理
            if (_defaultUINames.Contains(control.gameObject.name))
            {
                continue;
            }

            //对于儿子名字中，已经有过值的不进行重复添加
            if (_uiDict.ContainsKey(control.gameObject.name))
            {
                continue;
            }
            
            //如果字符串中有(Clone)那就删掉
            if (control.gameObject.name.EndsWith("(Clone)"))
            {
                control.gameObject.name = control.gameObject.name.Substring(0, name.Length - "(Clone)".Length);
            }

            //添加到字典进行管理
            _uiDict.Add(control.gameObject.name, control);
        }
    }

    //通过这两个方法在子类中也可以用控件名字直接得到字典中的控件
    protected T GetUIControl<T>(string controlName) where T : UIBehaviour
    {
        if (!_uiDict.ContainsKey(controlName))
        {
            Debug.LogError(controlName + "并不存在，请检查！");
            return null;
        }

        var result = _uiDict[controlName] as T;
        if (result == null)
        {
            Debug.LogError(controlName + "并不是类型：" + typeof(T).Name);
            return null;
        }

        return result;
    }


    // 面板被显示时调用：只做View自己的刷新、动画、状态重置，不做SetActive，XUIManager 统一负责SetActive
    public virtual void OnPanelViewShow()
    {
    }

    //面板被隐藏时调用：只做View自己的清理、动画、状态重置，不做SetActive，XUIManager 统一负责SetActive
    public virtual void OnPanelViewHide()
    {
    }


    //提供一个方法给子类作为初始化逻辑用
    protected virtual void InitPanelView()
    {
    }
}