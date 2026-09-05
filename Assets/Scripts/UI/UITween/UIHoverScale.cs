using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 指针进入时放大 UI,离开时恢复原始缩放。
/// </summary>
public sealed class UIHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Min(1f)] float m_scaleMultiplier = 1.08f;
    [SerializeField, Min(0.01f)] float m_duration = 0.12f;
    [SerializeField] Ease m_ease = Ease.OutCubic;
    [SerializeField] Selectable m_selectable;

    Vector3 m_originScale;
    MotionHandle m_handle;

    void Awake()
    {
        m_originScale = transform.localScale;
        if (m_selectable == null)
        {
            m_selectable = GetComponent<Selectable>();
        }
    }

    void OnDisable()
    {
        m_handle.TryCancel();
        transform.localScale = m_originScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_selectable != null && !m_selectable.interactable)
        {
            return;
        }

        AnimateTo(m_originScale * m_scaleMultiplier);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(m_originScale);
    }

    void AnimateTo(Vector3 targetScale)
    {
        m_handle.TryCancel();
        m_handle = UITween.DoScaleAnim(
                transform.localScale,
                targetScale,
                m_duration,
                transform,
                m_ease)
            .AddTo(this);
    }
}
