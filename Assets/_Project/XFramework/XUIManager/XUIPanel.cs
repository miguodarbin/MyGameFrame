using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;
using Toggle = UnityEngine.UI.Toggle;

public class XUIPanel : MonoBehaviour
{
    //我需要在这个基类里面就获得全部的子控件，放到一个字典里
    protected Dictionary<string, UIBehaviour> _uiDict = new Dictionary<string, UIBehaviour>();


    //Awake的时候就去读取自己所有子控件
    protected virtual void Awake()
    {
        GetAllControlsByUIType<Button>(); //按钮
        GetAllControlsByUIType<Toggle>(); //复选框
        GetAllControlsByUIType<Slider>(); //拖动条，进度条
        GetAllControlsByUIType<ScrollRect>(); //滚动视图
        GetAllControlsByUIType<InputField>(); //输入框
        GetAllControlsByUIType<ToggleGroup>(); //单选框
        GetAllControlsByUIType<TMP_Text>(); //文字
        GetAllControlsByUIType<Image>(); //图片
    }

    //通过UI类型，找到某一类的全部UI控件
    private void GetAllControlsByUIType<T>() where T : UIBehaviour
    {
        T[] controls = GetComponentsInChildren<T>();
        foreach (var control in controls)
        {
            if (_uiDict.ContainsKey(control.name))
            {
                Debug.LogError($"不允许ui控件重名，请检查{control.name}");
                return;
            }

            _uiDict.Add(control.name, control);
        }
    }
}