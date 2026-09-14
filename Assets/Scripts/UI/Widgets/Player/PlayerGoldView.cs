using AChen.Events;
using AChen.Player;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;

/// <summary>
/// 显示玩家金币，跟随玩家数据自动刷新。
/// </summary>
public class PlayerGoldView : MonoBehaviour
{
    const float TweenDuration = 0.45f;

    [SerializeField] TextMeshProUGUI m_GoldText;

    MotionHandle m_Tween;
    long? m_Displayed;
    bool m_HasDisplayed;

    void OnEnable()
    {
        EventCenter.AddListener(GameEvent.PlayerGoldChanged, OnGoldChanged);
        // 首次展示直接跳值, 避免从 0 滚到余额
        Apply(PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer?.Gold : null, false);
    }

    void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.PlayerGoldChanged, OnGoldChanged);
        m_Tween.TryCancel();
        m_HasDisplayed = false;
        m_Displayed = null;
    }

    void OnGoldChanged(long? value) => Apply(value, m_HasDisplayed);

    void Apply(long? value, bool animate)
    {
        if (m_GoldText == null)
        {
            return;
        }

        if (!value.HasValue)
        {
            m_Tween.TryCancel();
            m_HasDisplayed = false;
            m_Displayed = null;
            SetText(string.Empty);
            return;
        }

        long target = value.Value;
        if (!animate || !m_Displayed.HasValue || m_Displayed.Value == target)
        {
            m_Tween.TryCancel();
            SetDisplayed(target);
            return;
        }

        long from = m_Displayed.Value;
        m_Tween.TryCancel();
        m_Tween = LMotion.Create((float)from, (float)target, TweenDuration)
            .WithEase(Ease.OutCubic)
            .WithOnComplete(() => SetDisplayed(target))
            .Bind(current => SetDisplayed((long)Mathf.Round(current)))
            .AddTo(this);
    }

    void SetDisplayed(long gold)
    {
        m_Displayed = gold;
        m_HasDisplayed = true;
        SetText(gold.ToString());
    }

    void SetText(string gold)
    {
        if (m_GoldText.text != gold)
        {
            m_GoldText.Localized().SetKey("ui.common.gold_amount", new System.Collections.Generic.Dictionary<string, object> { ["gold"] = gold });
        }
    }
}
