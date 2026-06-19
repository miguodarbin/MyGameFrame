using System.Collections.Generic;
using UnityEngine;

public class Example_MVC_ControlSelectedSize : MonoBehaviour
{
    [Header("选中参考点")]
    public RectTransform perfectPoint;

    [Header("缩放参数")]
    public float normalScale = 1f;        // 离得远时的大小
    public float selectedScale = 1.2f;    // 靠近 perfectPoint 时的最大大小
    public float influenceDistance = 500f; // 多远范围内开始受影响
    public float scaleSpeed = 12f;        // 缩放平滑速度

    [Header("是否只判断横向距离")]
    public bool onlyCheckHorizontalDistance = true;

    private List<RectTransform> _itemList;
    private RectTransform _coordinateRoot;

    private void OnEnable()
    {
        XEventCenter.Instance.AddEventListener<List<RectTransform>>(
            XEventType.E_GetAllEntryItems,
            InitList
        );
    }

    private void OnDisable()
    {
        XEventCenter.Instance.RemoveEventListener<List<RectTransform>>(
            XEventType.E_GetAllEntryItems,
            InitList
        );
    }

    private void InitList(List<RectTransform> list)
    {
        _itemList = list;

        if (perfectPoint != null)
        {
            // 用 perfectPoint 的父物体作为统一坐标系
            _coordinateRoot = perfectPoint.parent as RectTransform;
        }

        // 刚拿到列表时，立刻刷新一次，避免第一帧状态不对
        RefreshItemScale(true);
    }

    private void Update()
    {
        RefreshItemScale(false);
    }

    private void RefreshItemScale(bool immediately)
    {
        if (_itemList == null || _itemList.Count == 0)
            return;

        if (perfectPoint == null || _coordinateRoot == null)
            return;

        Vector2 perfectPointPos = GetRectCenterInRoot(perfectPoint);

        foreach (RectTransform item in _itemList)
        {
            if (item == null)
                continue;

            Vector2 itemPos = GetRectCenterInRoot(item);

            float distance;

            if (onlyCheckHorizontalDistance)
            {
                // 横向滑动列表一般只看 X 距离
                distance = Mathf.Abs(itemPos.x - perfectPointPos.x);
            }
            else
            {
                // 如果是二维距离，就用 Vector2.Distance
                distance = Vector2.Distance(itemPos, perfectPointPos);
            }

            // 距离越近，weight 越接近 1
            // 距离越远，weight 越接近 0
            float weight = 1f - distance / influenceDistance;
            weight = Mathf.Clamp01(weight);

            // 平滑一下变化，不然缩放变化会有点硬
            weight = Mathf.SmoothStep(0f, 1f, weight);

            float targetScale = Mathf.Lerp(normalScale, selectedScale, weight);
            Vector3 targetScaleVec = Vector3.one * targetScale;

            if (immediately)
            {
                item.localScale = targetScaleVec;
            }
            else
            {
                item.localScale = Vector3.Lerp(
                    item.localScale,
                    targetScaleVec,
                    Time.deltaTime * scaleSpeed
                );
            }
        }
    }

    /// <summary>
    /// 得到某个 RectTransform 的视觉中心点，并转换到统一坐标系下
    /// </summary>
    private Vector2 GetRectCenterInRoot(RectTransform rect)
    {
        Vector3 worldCenter = rect.TransformPoint(rect.rect.center);
        Vector3 localCenter = _coordinateRoot.InverseTransformPoint(worldCenter);

        return new Vector2(localCenter.x, localCenter.y);
    }
}