using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

/// <summary>
/// UI面板基类
/// </summary>
/// <remarks>
/// 对外接口：
/// <list type="number">
/// <item>
/// <description><c> OnPanelEnable()</c>：启动面板时逻辑</description>
/// </item>
/// <item>
///  <description><c> OnPanelDisable()</c>：关闭面板逻辑</description>
/// </item>
/// <item>
///  <description><c>子类GetUIControl&lt;T&gt;(string controlName) </c>：根据控件GameObject名字获取指定类型的UI控件</description>
/// </item>
///  <item>
///  <description><c>子类重写OnButtonClicked </c>：Button点击后的统一回调，子类重写后根据按钮名字分发逻辑。</description>
/// </item>
///  <item>
///  <description><c>子类重写OnToggleValueChanged(string toggleObjName, bool state) </c>：Toggle状态变化后的统一回调</description>
/// </item>
///  <item>
///  <description><c>子类重写 OnSliderValueChanged(string sliderObjName, float value)</c>： Slider数值变化后的统一回调</description>
/// </item>
/// <item>
/// <description>想被这个Panel管理的UI控件，必须改成唯一且非默认的GameObject名字</description>
/// </item>
/// <item>
/// <description>子类如果重写 Awake，必须先调用 base.Awake()，否则不会自动收集UI控件！！</description>
/// </item>
/// </list>
/// </remarks>

public class XUIPanel : MonoBehaviour
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


    //Panel下，Awake的时候就去读取自己所有继承了UIBehaviour的子控件
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

            //添加到字典进行管理
            _uiDict.Add(control.gameObject.name, control);

            //并为这个控件判断类型添加事件函数,只处理button、toggle、slider，其他UI如果有事件监听就交给外面了，否则有很多UI没有事件，在这里白白监听
            if (control is Button button)
            {
                button.onClick.AddListener(() => OnButtonClicked(controlName)); //这里用了闭包，捕获了这一次的controlName本身，以后调用这个OnButtonClicked的时候，用的controlName也是这次捕获到的
            }
            else if (control is Toggle toggle)
            {
                toggle.onValueChanged.AddListener((state) => OnToggleValueChanged(toggle.gameObject.name, state));
            }
            else if (control is Slider slider)
            {
                slider.onValueChanged.AddListener((value) => OnSliderValueChanged(slider.gameObject.name, value));
            }
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


    //启动面板时逻辑
    public virtual void OnPanelEnable()
    {
        this.gameObject.SetActive(true);
    }
    
    //关闭面板逻辑
    public virtual void OnPanelDisable()
    {
        this.gameObject.SetActive(false);
    }


    //提供三个的虚方法给外部重写添加监听，外部可以快速为Button、Toggle、Slider回调写逻辑
    protected virtual void OnButtonClicked(string buttonObjName)
    {
    }

    protected virtual void OnToggleValueChanged(string toggleObjName, bool state)
    {
    }

    protected virtual void OnSliderValueChanged(string sliderObjName, float value)
    {
    }
}