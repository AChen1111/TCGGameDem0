using System;
using System.Collections.Generic;
using AChen.Activities;
using AChen.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ActivityRewardItem : MonoBehaviour
{
    [SerializeField] LocalizedText m_Label;
    [SerializeField] LocalizedText m_Amount;
    [SerializeField] LocalizedText m_Cost;
    [SerializeField] LocalizedText m_Action;
    [SerializeField] Button m_Button;
    [SerializeField] Image m_Icon;
    [SerializeField] Sprite m_GoldIcon;
    [SerializeField] Sprite m_CardIcon;
    [SerializeField] GameObject m_Claimed;
    [SerializeField] TMP_Text m_Description;
    Action m_click;
    ActivitySnapshot m_state;
    ActivityEntryDefinition m_entry;
    ActivityEntryState m_progress;
    void Awake() => m_Button.onClick.AddListener(() => m_click());
    public void Bind(ActivitySnapshot state, ActivityEntryDefinition entry, ActivityEntryState progress, ActivityManager manager, Action action)
    {
        m_click = action;
        m_state = state; m_entry = entry; m_progress = progress;
        if (state.Definition.Type == ActivityType.SignIn) m_Label.SetKey("ui.activities.sign_in.day", new Dictionary<string, object> { ["day"] = entry.DayIndex });
        else if (state.Definition.Type == ActivityType.Milestone) m_Label.SetKey("ui.activities.milestone", new Dictionary<string, object> { ["count"] = entry.Threshold });
        else m_Label.SetKey(entry.NameKey);
        var descriptions = new List<string>();
        foreach (var reward in entry.Rewards)
            descriptions.Add(LocalizationService.GetText(reward.RewardType == ActivityRewardType.Gold ? "ui.activities.reward_gold" : "ui.activities.reward_card",
                new Dictionary<string, object> { ["amount"] = reward.Amount, ["id"] = reward.RewardId, ["variant"] = reward.CardVariant }));
        m_Amount.SetKey("ui.activities.reward_summary", new Dictionary<string, object> { ["rewards"] = string.Join("\n", descriptions) });
        m_Icon.sprite = entry.Rewards[0].RewardType == ActivityRewardType.Gold ? m_GoldIcon : m_CardIcon;
        m_Description.text = entry.Description;
        m_Cost.gameObject.SetActive(entry.CostGold > 0);
        m_Cost.SetKey("ui.activities.cost", new Dictionary<string, object> { ["amount"] = entry.CostGold });
        UpdateAvailability(manager);
    }
    public void UpdateAvailability(ActivityManager manager)
    {
        var entry = m_entry; var progress = m_progress;
        bool canClaim = manager.CanClaim(m_state, entry.Id);
        m_Button.interactable = canClaim;
        m_Claimed.SetActive(progress.Status == "claimed");
        m_Action.SetKey(manager.HasPendingRequest(m_state.Definition.Id, entry.Id) ? "ui.activities.retry" : progress.Status == "claimed" ? (entry.PeriodKind == ActivityPeriodKind.Daily ? "ui.activities.claimed_today" : "ui.activities.claimed") :
            canClaim ? (entry.CostGold > 0 ? "ui.activities.exchange" : "ui.activities.claim") : "ui.activities.locked");
    }
}
