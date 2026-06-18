using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 搜索do可以找到dotween改动画效果
/// </summary>
public class XScrollSnap : MonoBehaviour
{
    public RectTransform content;
    public RectTransform perfectPoint;
    public List<RectTransform> itemList = new List<RectTransform>();
    public int currentSnapIndex = 0;

    public float fastDragSpeed = 3000f;
    public float snapTolerance = 0.5f;

    private EventTrigger.Entry _onDragEntry;
    private EventTrigger.Entry _onEndDragEntry;

    private float _dragSpeed = 0;
    private float _dragAllSpeed = 0;
    private float _dragAvergeSpeed = 0;
    private int _count = 0;

    private float _dragTotalDeltaX = 0;

    private Tween _snapTween;
    private Coroutine _checkSnapCoroutine;

    private RectTransform ContentParentRect
    {
        get
        {
            return content.parent as RectTransform;
        }
    }

    private void Awake()
    {
        AddDragEvent();
    }

    private void Start()
    {
        RefreshLayout();
        GetAllItems();
        CheckAndSnapImmediately();
    }

    private void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();

        if (content != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        }
    }

    private void GetAllItems()
    {
        itemList.Clear();

        foreach (Transform child in content.transform)
        {
            if (child.name.Contains("item"))
            {
                RectTransform childRect = child as RectTransform;

                if (childRect != null)
                {
                    itemList.Add(childRect);
                }
            }
        }
    }

    private void AddDragEvent()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();

        if (eventTrigger == null)
        {
            eventTrigger = gameObject.AddComponent<EventTrigger>();
        }

        _onDragEntry = new EventTrigger.Entry();
        _onDragEntry.eventID = EventTriggerType.Drag;
        eventTrigger.triggers.Add(_onDragEntry);

        _onEndDragEntry = new EventTrigger.Entry();
        _onEndDragEntry.eventID = EventTriggerType.EndDrag;
        eventTrigger.triggers.Add(_onEndDragEntry);
    }

    private void OnDrag(BaseEventData eventData)
    {
        _snapTween?.Kill();

        PointerEventData data = eventData as PointerEventData;

        if (data == null)
            return;

        Vector2 anchoredPos = content.anchoredPosition;
        anchoredPos.x += data.delta.x;
        content.anchoredPosition = anchoredPos;

        _dragSpeed = Mathf.Abs(data.delta.x / Time.unscaledDeltaTime);
        _dragAllSpeed += _dragSpeed;
        _count++;

        _dragTotalDeltaX += data.delta.x;
    }

    private void OnEndDrag(BaseEventData eventData)
    {
        if (itemList.Count == 0)
        {
            ResetDragData();
            return;
        }

        _dragAvergeSpeed = _count > 0 ? _dragAllSpeed / _count : 0;

        int closestIndex = GetClosestItemIndex();

        if (closestIndex == -1)
        {
            ResetDragData();
            return;
        }

        int targetIndex = closestIndex;

        if (_dragAvergeSpeed > fastDragSpeed)
        {
            int nextIndex = currentSnapIndex;

            if (_dragTotalDeltaX < 0)
            {
                // 往左拖，理论上去下一张
                nextIndex++;
            }
            else if (_dragTotalDeltaX > 0)
            {
                // 往右拖，理论上去上一张
                nextIndex--;
            }

            nextIndex = Mathf.Clamp(nextIndex, 0, itemList.Count - 1);

            // 如果最近的不是理论上一张/下一张，就按最近的吸附
            if (closestIndex == nextIndex)
            {
                targetIndex = nextIndex;
            }
            else
            {
                targetIndex = closestIndex;
            }
        }

        currentSnapIndex = targetIndex;
        SnapToItem(currentSnapIndex, true);

        ResetDragData();
    }

    private int GetClosestItemIndex()
    {
        int closestIndex = -1;
        float closestDistance = 9999999f;

        Vector3 perfectWorldCenter = GetRectWorldCenter(perfectPoint);
        Vector3 perfectPosInParent = ContentParentRect.InverseTransformPoint(perfectWorldCenter);

        for (int i = 0; i < itemList.Count; i++)
        {
            Vector3 itemWorldCenter = GetRectWorldCenter(itemList[i]);
            Vector3 itemPosInParent = ContentParentRect.InverseTransformPoint(itemWorldCenter);

            float distance = Mathf.Abs(itemPosInParent.x - perfectPosInParent.x);

            if (distance < closestDistance)
            {
                closestIndex = i;
                closestDistance = distance;
            }
        }

        return closestIndex;
    }

    private Vector3 GetRectWorldCenter(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        // 四个角点平均，就是矩形视觉中心
        return (corners[0] + corners[1] + corners[2] + corners[3]) / 4f;
    }

    private bool IsItemSnappedToPerfect(int index)
    {
        if (index < 0 || index >= itemList.Count)
            return false;

        Vector3 itemWorldCenter = GetRectWorldCenter(itemList[index]);
        Vector3 perfectWorldCenter = GetRectWorldCenter(perfectPoint);

        Vector3 itemPosInParent = ContentParentRect.InverseTransformPoint(itemWorldCenter);
        Vector3 perfectPosInParent = ContentParentRect.InverseTransformPoint(perfectWorldCenter);

        float distanceX = Mathf.Abs(itemPosInParent.x - perfectPosInParent.x);

        return distanceX <= snapTolerance;
    }

    private void CheckAndSnapImmediately()
    {
        if (content == null || perfectPoint == null)
            return;

        RefreshLayout();

        if (itemList.Count == 0)
        {
            GetAllItems();
        }

        if (itemList.Count == 0)
            return;

        int closestIndex = GetClosestItemIndex();

        if (closestIndex == -1)
            return;

        currentSnapIndex = closestIndex;

        if (!IsItemSnappedToPerfect(closestIndex))
        {
            SnapToItem(closestIndex, false);
        }
    }

    private IEnumerator CheckAndSnapNextFrame()
    {
        yield return null;

        CheckAndSnapImmediately();

        _checkSnapCoroutine = null;
    }

    private void SnapToClosestItem(bool useTween)
    {
        int closestIndex = GetClosestItemIndex();

        if (closestIndex == -1)
            return;

        currentSnapIndex = closestIndex;
        SnapToItem(currentSnapIndex, useTween);
    }

    private void SnapToItem(int index, bool useTween)
    {
        if (index < 0 || index >= itemList.Count)
            return;

        Vector3 itemWorldCenter = GetRectWorldCenter(itemList[index]);
        Vector3 perfectWorldCenter = GetRectWorldCenter(perfectPoint);

        Vector3 itemPosInParent = ContentParentRect.InverseTransformPoint(itemWorldCenter);
        Vector3 perfectPosInParent = ContentParentRect.InverseTransformPoint(perfectWorldCenter);

        Vector3 toPerfectOffsetInParent = perfectPosInParent - itemPosInParent;

        Vector2 contentAnchoredPos = content.anchoredPosition;
        contentAnchoredPos.x += toPerfectOffsetInParent.x;

        if (useTween)
        {
            _snapTween?.Kill();
            _snapTween = content.DOAnchorPos(contentAnchoredPos, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            _snapTween?.Kill();
            content.anchoredPosition = contentAnchoredPos;
        }
    }

    private void ResetDragData()
    {
        _dragAllSpeed = 0;
        _dragSpeed = 0;
        _count = 0;
        _dragTotalDeltaX = 0;
    }

    // private void OnGUI()
    // {
    //     GUIStyle style = new GUIStyle();
    //     style.fontSize = 40;
    //
    //     GUI.Label(new Rect(10, 10, 100, 100), "_dragAvergeSpeed " + _dragAvergeSpeed, style);
    //     GUI.Label(new Rect(10, 110, 100, 100), "_dragSpeed " + _dragSpeed, style);
    //     GUI.Label(new Rect(10, 210, 100, 100), "_dragAllSpeed " + _dragAllSpeed, style);
    //     GUI.Label(new Rect(10, 310, 100, 100), "count " + _count, style);
    //     GUI.Label(new Rect(10, 410, 100, 100), "_dragTotalDeltaX " + _dragTotalDeltaX, style);
    //     GUI.Label(new Rect(10, 510, 100, 100), "currentSnapIndex " + currentSnapIndex, style);
    // }

    private void OnEnable()
    {
        _onDragEntry.callback.AddListener(OnDrag);
        _onEndDragEntry.callback.AddListener(OnEndDrag);

        if (_checkSnapCoroutine != null)
        {
            StopCoroutine(_checkSnapCoroutine);
        }

        _checkSnapCoroutine = StartCoroutine(CheckAndSnapNextFrame());
    }

    private void OnDisable()
    {
        _onDragEntry.callback.RemoveListener(OnDrag);
        _onEndDragEntry.callback.RemoveListener(OnEndDrag);

        if (_checkSnapCoroutine != null)
        {
            StopCoroutine(_checkSnapCoroutine);
            _checkSnapCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        _snapTween?.Kill();
    }
}