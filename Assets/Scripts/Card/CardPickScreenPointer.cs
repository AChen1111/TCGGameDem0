using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public interface ICardPickPointer
{
    bool TryGetPointerRay(out Ray ray);
}

/// <summary>把 RawImage 上的屏幕点映射成抽卡相机射线. 渲到 RT 时不能用 ScreenPointToRay.</summary>
public sealed class CardPickScreenPointer : ICardPickPointer
{
    readonly RawImage _rawImage;
    readonly Camera _uiCamera;
    readonly Camera _pickCamera;
    readonly List<RaycastResult> _hits = new List<RaycastResult>(8);
    PointerEventData _eventData;

    public CardPickScreenPointer(RawImage rawImage, Camera uiCamera, Camera pickCamera)
    {
        _rawImage = rawImage;
        _uiCamera = uiCamera;
        _pickCamera = pickCamera;
    }

    public bool TryGetPointerRay(out Ray ray)
    {
        ray = default;
        if (_rawImage == null || _pickCamera == null || Mouse.current == null)
        {
            return false;
        }

        Vector2 screen = Mouse.current.position.ReadValue();
        if (HitsBlockingControl(screen))
        {
            return false;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rawImage.rectTransform, screen, _uiCamera, out Vector2 local))
        {
            return false;
        }

        Rect rect = _rawImage.rectTransform.rect;
        if (!rect.Contains(local))
        {
            return false;
        }

        float u = (local.x - rect.xMin) / rect.width;
        float v = (local.y - rect.yMin) / rect.height;
        ray = _pickCamera.ViewportPointToRay(new Vector3(u, v, 0f));
        return true;
    }

    bool HitsBlockingControl(Vector2 screen)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            return false;
        }

        if (_eventData == null)
        {
            _eventData = new PointerEventData(eventSystem);
        }

        _eventData.position = screen;
        _hits.Clear();
        eventSystem.RaycastAll(_eventData, _hits);
        for (int i = 0; i < _hits.Count; i++)
        {
            GameObject hit = _hits[i].gameObject;
            if (hit == _rawImage.gameObject || hit.transform.IsChildOf(_rawImage.transform))
            {
                return false;
            }

            return true;
        }

        return false;
    }
}
