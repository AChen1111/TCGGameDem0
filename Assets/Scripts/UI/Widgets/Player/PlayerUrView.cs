using AChen.Events;
using AChen.Player;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

public sealed class PlayerUrView : MonoBehaviour
{
    const float TweenDuration = 0.45f;

    [SerializeField] TMP_Text m_Amount;

    MotionHandle m_Tween;
    long? m_Displayed;

    void OnEnable()
    {
        EventCenter.AddListener(GameEvent.PlayerUrChanged, OnChanged);
        Apply(PlayerSession.Instance.CurrentPlayer?.Ur, false);
    }
    void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.PlayerUrChanged, OnChanged);
        m_Tween.TryCancel();
        m_Displayed = null;
    }

    void OnChanged(long? amount) => Apply(amount, m_Displayed.HasValue);

    void Apply(long? amount, bool animate)
    {
        if (!amount.HasValue)
        {
            m_Tween.TryCancel();
            m_Displayed = null;
            m_Amount.text = string.Empty;
            return;
        }

        long target = amount.Value;
        if (!animate || m_Displayed.Value == target)
        {
            m_Tween.TryCancel();
            SetDisplayed(target);
            return;
        }

        long from = m_Displayed.Value;
        m_Tween.TryCancel();
        m_Tween = LMotion.Create(0f, 1f, TweenDuration)
            .WithEase(Ease.OutCubic)
            .WithOnComplete(() => SetDisplayed(target))
            .Bind(progress => SetDisplayed((long)System.Math.Round(from + (target - (double)from) * progress)))
            .AddTo(this);
    }

    void SetDisplayed(long amount)
    {
        m_Displayed = amount;
        m_Amount.text = amount.ToString("N0");
    }

    public void Show(long amount) => Apply(amount, false);
}
