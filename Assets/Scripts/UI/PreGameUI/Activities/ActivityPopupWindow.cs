using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Activities;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class ActivityPopupProperties : IWindowProperties
{
    public readonly string ActivityId;
    public readonly long PolicyVersion;
    public readonly string PeriodKey;
    public readonly long SessionVersion;
    public readonly ActivityPopupScheduler Scheduler;
    public ActivityPopupProperties(string id, long policy, string period, long session, ActivityPopupScheduler scheduler)
    { ActivityId = id; PolicyVersion = policy; PeriodKey = period; SessionVersion = session; Scheduler = scheduler; }
}
public class ActivityPopupWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnClose;
    // --tag_end: 自动生成--

    [SerializeField] ActivityDetailView m_Detail;
    ActivityPopupProperties Context => (ActivityPopupProperties)Properties;
    ActivityManager Manager => PlayerSession.Instance.Activities;
    string m_navigation;
    bool m_counted;
    protected override void AddListeners() => m_BtnClose.onClick.AddListener(UI_Close);
    protected override void RemoveListeners() => m_BtnClose.onClick.RemoveListener(UI_Close);
    protected override void OnOpen()
    {
        m_navigation = null; m_counted = false;
        Manager.Changed += Render;
        Render(); Tick().Forget();
    }
    protected override void OnResume() => Render();
    void Render()
    {
        if (!Manager.TryGet(Context.ActivityId, out var state)) { m_Detail.gameObject.SetActive(false); if (IsVisible) UI_Close(); return; }
        m_Detail.Bind(state, Manager, Claim, id => { m_navigation = id; UI_Close(); }, ScreenToken);
    }
    protected override async UniTask PlayEnterTransition(CancellationToken token)
    {
        if (!m_counted && !Context.Scheduler.IsValid(Context)) { UI_Close(); return; }
        await base.PlayEnterTransition(token);
        token.ThrowIfCancellationRequested();
        if (!m_counted && !Context.Scheduler.IsValid(Context)) { UI_Close(); return; }
        if (!m_counted) { m_counted = true; Context.Scheduler.Visible(Context); }
    }
    protected override void OnClose()
    {
        Manager.Changed -= Render;
        Context.Scheduler.Finished(m_navigation);
    }
    protected override void OnDestroy() { Manager.Changed -= Render; base.OnDestroy(); }
    void Claim(string entry)
    {
        var definition = Manager.Items.Single(x => x.Definition.Id == Context.ActivityId).Definition.Entries.Single(x => x.Id == entry);
        if (definition.CostGold > 0)
            Confirm("ui.activities.exchange_confirm", () => ClaimAsync(entry).Forget(), new Dictionary<string, object> { ["amount"] = definition.CostGold });
        else ClaimAsync(entry).Forget();
    }
    async UniTaskVoid ClaimAsync(string entry)
    {
        await RunGuardedAsync(async ct =>
        {
            var result = await Manager.ClaimAsync(Context.ActivityId, entry, ct);
            if (result.UrGained > 0) RequestOpenWindow(AddressKeys.Prefab.UrNoticeWindow, new UrNoticeProperties(new LocalizedMessage("ui.workshop.gift_overflow", new Dictionary<string, object> { ["amount"] = result.UrGained })));
        }, "领取活动奖励", "ui.activities.claim_failed");
        if (IsOpened) Render();
    }
    async UniTaskVoid Tick()
    {
        try { while (IsOpened) { await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true, cancellationToken: ScreenToken); if (m_Detail.gameObject.activeSelf) m_Detail.UpdateTime(Manager); } }
        catch (OperationCanceledException) { }
    }
}
