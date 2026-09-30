using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Activities;
using AChen.Configuration;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ActivityDetailView : MonoBehaviour
{
    [SerializeField] LocalizedText m_Title;
    [SerializeField] LocalizedText m_Time;
    [SerializeField] LocalizedText m_Reset;
    [SerializeField] LocalizedText m_State;
    [SerializeField] LocalizedText m_Progress;
    [SerializeField] TMP_Text m_Description;
    [SerializeField] TMP_Text m_Notice;
    [SerializeField] Image m_Banner;
    [SerializeField] Sprite[] m_TypeBanners;
    [SerializeField] RectTransform m_Content;
    [SerializeField] ActivityRewardItem m_RewardPrefab;
    [SerializeField] Button m_GoTo;
    readonly List<ActivityRewardItem> m_rewards = new List<ActivityRewardItem>();
    Action<string> m_navigation;
    ActivitySnapshot m_state;
    int m_bindVersion;
    void Awake() => m_GoTo.onClick.AddListener(() => m_navigation(m_state.Definition.Notice.ActionTarget));
    public void Bind(ActivitySnapshot state, ActivityManager manager, Action<string> claim, Action<string> navigate, CancellationToken token)
    {
        gameObject.SetActive(true); m_state = state; m_navigation = navigate;
        m_Title.SetKey(state.Definition.NameKey);
        m_Description.text = state.Definition.DescriptionKey.Length > 0 ? LocalizationService.GetText(state.Definition.DescriptionKey) : state.Definition.Description;
        m_Notice.gameObject.SetActive(state.Definition.Type == ActivityType.Notice);
        m_Notice.text = state.Definition.Notice.Content;
        m_GoTo.gameObject.SetActive(state.Definition.Type == ActivityType.Notice && state.Definition.Notice.ActionKind == "activity");
        m_GoTo.interactable = manager.Running(state) && state.Eligible && !manager.IsStale;
        m_State.SetKey(manager.IsStale ? "ui.activities.sync_failed" : !manager.Running(state) ? "ui.activities." + state.Status : !state.Eligible ? "ui.activities.locked" : "ui.activities.running");
        m_Progress.gameObject.SetActive(state.Definition.Type is ActivityType.SignIn or ActivityType.Milestone);
        m_Progress.SetKey(state.Definition.Type == ActivityType.SignIn ? "ui.activities.sign_in.progress" : "ui.activities.draw.progress",
            new Dictionary<string, object> { ["count"] = state.PlayerState.Progress, ["days"] = state.Definition.RequiredDays });
        foreach (var reward in m_rewards) Destroy(reward.gameObject);
        m_rewards.Clear();
        foreach (var entry in state.Definition.Entries.OrderBy(x => x.SortOrder).ThenBy(x => x.Id))
        {
            var item = Instantiate(m_RewardPrefab, m_Content); m_rewards.Add(item);
            item.Bind(state, entry, state.PlayerState.EntryStates.Single(x => x.EntryId == entry.Id), manager, () => claim(entry.Id));
        }
        m_Banner.sprite = m_TypeBanners[(int)state.Definition.Type];
        string resource = state.Definition.Type == ActivityType.Notice && state.Definition.Notice.ImageResourceKey.Length > 0 ? state.Definition.Notice.ImageResourceKey : state.Definition.BannerResourceKey;
        int version = ++m_bindVersion;
        if (resource.Length > 0) LoadBanner(resource, version, token).Forget();
        UpdateTime(manager);
    }
    async UniTaskVoid LoadBanner(string key, int version, CancellationToken token)
    {
        try
        {
            var sprite = await AddressableLoader.Instance.LoadSprite(key).AttachExternalCancellation(token);
            if (version == m_bindVersion && !token.IsCancellationRequested) m_Banner.sprite = sprite;
        }
        catch (OperationCanceledException) { }
    }
    public void UpdateTime(ActivityManager manager)
    {
        var d = m_state.Definition;
        m_Reset.gameObject.SetActive(d.Entries.Any(x => x.PeriodKind == ActivityPeriodKind.Daily));
        m_Reset.SetKey("ui.activities.next_reset", new Dictionary<string, object> { ["time"] = manager.Snapshot.NextResetAt.ToOffset(TimeSpan.FromHours(8)).ToString("MM-dd HH:mm") });
        string status = d.ScheduleMode == ActivityScheduleMode.Timed && manager.Clock.Now >= d.EndsAt ? "ended" : m_state.Status;
        var condition = m_state.LockedCondition;
        string dependency = manager.TryGet(condition.ActivityId, out var required) ? LocalizationService.GetText(required.Definition.NameKey) : condition.ActivityId;
        m_State.SetKey(manager.IsStale ? "ui.activities.sync_failed" : !manager.Running(m_state) ? "ui.activities." + status : !m_state.Eligible ? "ui.activities.condition." + m_state.LockedReasonCode : "ui.activities.running",
            new Dictionary<string, object> { ["count"] = condition.Value, ["activity"] = dependency, ["time"] = condition.Time?.ToOffset(TimeSpan.FromHours(8)).ToString("yyyy-MM-dd HH:mm") });
        foreach (var reward in m_rewards) reward.UpdateAvailability(manager);
        m_GoTo.interactable = manager.Running(m_state) && m_state.Eligible && !manager.IsStale;
        if (d.ScheduleMode == ActivityScheduleMode.Permanent) m_Time.SetKey("ui.activities.permanent");
        else
        {
            var left = (d.StatusTime(manager.Clock.Now) - manager.Clock.Now);
            m_Time.SetKey(manager.Clock.Now < d.StartsAt ? "ui.activities.starts_in" : "ui.activities.ends_in",
                new Dictionary<string, object> { ["time"] = string.Format("{0}d {1:00}:{2:00}:{3:00}", Math.Max(0, (int)left.TotalDays), Math.Max(0,left.Hours), Math.Max(0,left.Minutes), Math.Max(0,left.Seconds)) });
        }
    }
}
static class ActivityTimeExtensions
{
    public static DateTimeOffset StatusTime(this ActivityDefinition definition, DateTimeOffset now) => now < definition.StartsAt ? definition.StartsAt.Value : definition.EndsAt.Value;
}
