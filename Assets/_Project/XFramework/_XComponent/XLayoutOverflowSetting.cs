using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(HorizontalLayoutGroup))]
public class XLayoutOverflowSetting : MonoBehaviour
{
    [Header("Fixed Point")]
    [SerializeField] private TextAnchor fixedAnchor = TextAnchor.MiddleRight;

    [Header("Overflow Check")]
    [SerializeField] private float epsilon = 0.5f;

    private RectTransform _rect;
    private HorizontalLayoutGroup _layoutGroup;

    private bool _isOverflowMode;

    private void Awake()
    {
        CacheComponents();
    }

    private void OnEnable()
    {
        CacheComponents();
        MarkLayoutDirty();
    }

    private void OnDisable()
    {
        CacheComponents();

        if (_layoutGroup != null)
            _layoutGroup.enabled = true;

        _isOverflowMode = false;
    }

    private void OnValidate()
    {
        CacheComponents();

        if (epsilon < 0)
            epsilon = 0;

        // 重点：
        // OnValidate 里不能直接改 RectTransform。
        // 所以这里只标记，延迟到安全时机再 ApplyLayout。
        MarkLayoutDirty();
    }

    private void LateUpdate()
    {
        ApplyLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        MarkLayoutDirty();
    }

    private void MarkLayoutDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall -= DelayedApplyLayoutInEditor;
            EditorApplication.delayCall += DelayedApplyLayoutInEditor;
        }
#endif
    }

#if UNITY_EDITOR
    private void DelayedApplyLayoutInEditor()
    {
        if (this == null)
            return;

        if (!isActiveAndEnabled)
            return;

        CacheComponents();
        ApplyLayout();
    }
#endif

    private void CacheComponents()
    {
        if (_rect == null)
            _rect = transform as RectTransform;

        if (_layoutGroup == null)
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
    }

    private void ApplyLayout()
    {
        if (_rect == null || _layoutGroup == null)
            return;

        if (!isActiveAndEnabled)
            return;

        _layoutGroup.childAlignment = fixedAnchor;

        float needWidth = GetChildrenTotalWidth();
        float currentWidth = _rect.rect.width;

        if (currentWidth + epsilon >= needWidth)
        {
            NormalLayoutMode();
        }
        else
        {
            OverflowLayoutMode();
        }
    }

    private void NormalLayoutMode()
    {
        if (_layoutGroup.enabled == false)
        {
            _layoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
        }

        _isOverflowMode = false;
    }

    private void OverflowLayoutMode()
    {
        if (_isOverflowMode == false)
        {
            _layoutGroup.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);

            _layoutGroup.enabled = false;
            _isOverflowMode = true;
        }

        if (_layoutGroup.enabled)
            _layoutGroup.enabled = false;

        float contentWidth = GetChildrenContentWidth();
        Vector2 anchorPoint = GetAnchorPoint(fixedAnchor);

        float cursorX = GetStartX(contentWidth);

        for (int i = 0; i < _rect.childCount; i++)
        {
            RectTransform child = _rect.GetChild(i) as RectTransform;

            if (ShouldIgnoreChild(child))
                continue;

            float width = GetChildWidth(child);
            float height = child.rect.height;

            child.anchorMin = anchorPoint;
            child.anchorMax = anchorPoint;

            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            child.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            float anchoredX = cursorX + child.pivot.x * width;
            float anchoredY = GetAnchoredY(child, height);

            child.anchoredPosition = new Vector2(anchoredX, anchoredY);

            cursorX += width + _layoutGroup.spacing;
        }
    }

    private float GetChildrenTotalWidth()
    {
        return _layoutGroup.padding.left
               + GetChildrenContentWidth()
               + _layoutGroup.padding.right;
    }

    private float GetChildrenContentWidth()
    {
        float totalWidth = 0f;
        int validChildCount = 0;

        for (int i = 0; i < _rect.childCount; i++)
        {
            RectTransform child = _rect.GetChild(i) as RectTransform;

            if (ShouldIgnoreChild(child))
                continue;

            validChildCount++;
            totalWidth += GetChildWidth(child);
        }

        if (validChildCount > 1)
            totalWidth += _layoutGroup.spacing * (validChildCount - 1);

        return totalWidth;
    }

    private float GetChildWidth(RectTransform child)
    {
        float width = LayoutUtility.GetMinWidth(child);

        if (width <= 0)
            width = LayoutUtility.GetPreferredWidth(child);

        if (width <= 0)
            width = child.rect.width;

        return width;
    }

    private bool ShouldIgnoreChild(RectTransform child)
    {
        if (child == null)
            return true;

        if (child.gameObject.activeSelf == false)
            return true;

        LayoutElement layoutElement = child.GetComponent<LayoutElement>();

        if (layoutElement != null && layoutElement.ignoreLayout)
            return true;

        return false;
    }

    private float GetStartX(float contentWidth)
    {
        switch (fixedAnchor)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                return _layoutGroup.padding.left;

            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                return -contentWidth * 0.5f
                       + (_layoutGroup.padding.left - _layoutGroup.padding.right) * 0.5f;

            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                return -_layoutGroup.padding.right - contentWidth;
        }

        return 0f;
    }

    private float GetAnchoredY(RectTransform child, float height)
    {
        switch (fixedAnchor)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.UpperCenter:
            case TextAnchor.UpperRight:
                return -_layoutGroup.padding.top - (1f - child.pivot.y) * height;

            case TextAnchor.MiddleLeft:
            case TextAnchor.MiddleCenter:
            case TextAnchor.MiddleRight:
                return (_layoutGroup.padding.bottom - _layoutGroup.padding.top) * 0.5f
                       + (child.pivot.y - 0.5f) * height;

            case TextAnchor.LowerLeft:
            case TextAnchor.LowerCenter:
            case TextAnchor.LowerRight:
                return _layoutGroup.padding.bottom + child.pivot.y * height;
        }

        return child.anchoredPosition.y;
    }

    private Vector2 GetAnchorPoint(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.UpperLeft:
                return new Vector2(0f, 1f);

            case TextAnchor.UpperCenter:
                return new Vector2(0.5f, 1f);

            case TextAnchor.UpperRight:
                return new Vector2(1f, 1f);

            case TextAnchor.MiddleLeft:
                return new Vector2(0f, 0.5f);

            case TextAnchor.MiddleCenter:
                return new Vector2(0.5f, 0.5f);

            case TextAnchor.MiddleRight:
                return new Vector2(1f, 0.5f);

            case TextAnchor.LowerLeft:
                return new Vector2(0f, 0f);

            case TextAnchor.LowerCenter:
                return new Vector2(0.5f, 0f);

            case TextAnchor.LowerRight:
                return new Vector2(1f, 0f);
        }

        return new Vector2(0.5f, 0.5f);
    }
}