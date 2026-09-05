using LitMotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>挂在 Button 上,按下时播放短促缩放.</summary>
[RequireComponent(typeof(Button))]
public class ButtonClickTween : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] bool m_Skip;
    [SerializeField] float m_Duration = 0.12f;
    [SerializeField] float m_PunchScale = 1.04f;
    [SerializeField] Ease m_Ease = Ease.OutCubic;

    Button m_Button;
    Vector3 m_OriginScale = Vector3.one;
    bool m_CapturedOrigin;
    MotionHandle m_Handle;

    void Awake()
    {
        m_Button = GetComponent<Button>();
    }

    [Button("重置特效")]
    void OnEnable()
    {
        if (!m_CapturedOrigin)
        {
            m_OriginScale = transform.localScale;
            m_CapturedOrigin = true;
        }
        else
        {
            transform.localScale = m_OriginScale;
        }
    }

    void OnDisable()
    {
        m_Handle.TryCancel();
        transform.localScale = m_OriginScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_Skip) return;
        if (m_Button != null && !m_Button.interactable) return;

        m_Handle.TryCancel();
        transform.localScale = m_OriginScale;
        m_Handle = UITween.DoPunchScale(transform, m_PunchScale, m_Duration, m_Ease).AddTo(this);
    }
}
