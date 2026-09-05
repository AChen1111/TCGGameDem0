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
    int m_originSiblingIndex;
    LayoutGroup m_parentLayout;
    bool m_raised;

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
        SetRaised(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_selectable != null && !m_selectable.interactable)
        {
            return;
        }

        SetRaised(true);
        AnimateTo(m_originScale * m_scaleMultiplier);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetRaised(false);
        AnimateTo(m_originScale);
    }

    void SetRaised(bool raised)
    {
        if (m_raised == raised) return;
        m_raised = raised;
        if (raised)
        {
            m_originSiblingIndex = transform.GetSiblingIndex();
            m_parentLayout = transform.parent != null
                ? transform.parent.GetComponent<LayoutGroup>()
                : null;
            // 先停布局再改层级,否则 HorizontalLayoutGroup 会按新顺序把格子挤走
            if (m_parentLayout != null)
            {
                m_parentLayout.enabled = false;
            }

            transform.SetAsLastSibling();
            return;
        }

        transform.SetSiblingIndex(m_originSiblingIndex);
        if (m_parentLayout != null)
        {
            m_parentLayout.enabled = true;
            m_parentLayout = null;
        }
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
