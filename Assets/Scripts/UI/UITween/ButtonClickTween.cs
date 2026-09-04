using LitMotion;
using UnityEngine;
using UnityEngine.UI;

/// <summary>点击时播放短促缩放.界面打开和列表取格子时自动挂上.</summary>
[RequireComponent(typeof(Button))]
public class ButtonClickTween : MonoBehaviour
{
    [SerializeField] bool m_Skip;
    [SerializeField] float m_Duration = 0.12f;
    [SerializeField] float m_PunchScale = 1.04f;
    [SerializeField] Ease m_Ease = Ease.InOutCubic;

    Button m_Button;
    Vector3 m_OriginScale = Vector3.one;
    bool m_CapturedOrigin;
    MotionHandle m_Handle;

    public static void EnsureOn(Transform root)
    {
        if (root == null) return;
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            EnsureOn(buttons[i]);
        }
    }

    public static void EnsureOn(Button button)
    {
        if (button == null) return;
        if (button.GetComponent<ButtonClickTween>() == null)
        {
            button.gameObject.AddComponent<ButtonClickTween>();
        }
    }

    void Awake()
    {
        m_Button = GetComponent<Button>();
    }

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

        if (m_Button != null)
        {
            m_Button.onClick.AddListener(OnClicked);
        }
    }

    void OnDisable()
    {
        if (m_Button != null)
        {
            m_Button.onClick.RemoveListener(OnClicked);
        }

        m_Handle.TryCancel();
        transform.localScale = m_OriginScale;
    }

    void OnClicked()
    {
        if (m_Skip) return;
        if (m_Button != null && !m_Button.interactable) return;

        m_Handle.TryCancel();
        transform.localScale = m_OriginScale;
        m_Handle = UITween.DoPunchScale(transform, m_PunchScale, m_Duration, m_Ease).AddTo(this);
    }
}
