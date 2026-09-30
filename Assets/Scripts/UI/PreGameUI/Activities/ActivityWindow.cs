using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Activities;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public sealed class ActivityWindowProperties : IWindowProperties
{
    public readonly string ActivityId;
    public ActivityWindowProperties(string id) { ActivityId = id; }
}
public class ActivityWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnClose;
    [SerializeField] Button m_BtnRefresh;
    // --tag_end: 自动生成--

    [SerializeField] RectTransform m_ListContent;
    [SerializeField] ActivityListItem m_ItemPrefab;
    [SerializeField] ActivityDetailView m_Detail;
    [SerializeField] LocalizedText m_Empty;
    readonly List<ActivityListItem> m_rows = new List<ActivityListItem>();
    string m_selected;
    ActivityManager Manager => PlayerSession.Instance.Activities;
    protected override void AddListeners() { m_BtnClose.onClick.AddListener(UI_Close); m_BtnRefresh.onClick.AddListener(Refresh); }
    protected override void RemoveListeners() { m_BtnClose.onClick.RemoveListener(UI_Close); m_BtnRefresh.onClick.RemoveListener(Refresh); }
    protected override void OnOpen()
    {
        if (Properties is ActivityWindowProperties properties) m_selected = properties.ActivityId;
        Manager.Changed += Render;
        Render(); Refresh(); Tick().Forget();
    }
    protected override void OnResume() => Render();
    protected override void OnClose() => Manager.Changed -= Render;
    protected override void OnDestroy() { Manager.Changed -= Render; base.OnDestroy(); }
    void Refresh() => RefreshAsync().Forget();
    async UniTaskVoid RefreshAsync()
    {
        await RunGuardedAsync(ct => Manager.RefreshAsync(false, ct), "刷新活动", "ui.activities.sync_failed");
        if (IsOpened) Render();
    }
    void Render()
    {
        var items = Manager.PageItems.ToList();
        foreach (var row in m_rows) Destroy(row.gameObject);
        m_rows.Clear();
        m_Empty.gameObject.SetActive(items.Count == 0);
        m_Empty.SetKey(Manager.IsLoading ? "ui.activities.loading" : Manager.IsStale ? "ui.activities.sync_failed" : "ui.activities.empty");
        m_Detail.gameObject.SetActive(items.Count > 0);
        if (items.Count == 0) return;
        if (!items.Any(x => x.Definition.Id == m_selected)) m_selected = (items.FirstOrDefault(x => x.PlayerState.EntryStates.Any(e => e.CanClaim)) ?? items[0]).Definition.Id;
        foreach (var item in items)
        {
            var row = Instantiate(m_ItemPrefab, m_ListContent); m_rows.Add(row);
            var id = item.Definition.Id;
            row.Bind(item, id == m_selected, () => { m_selected = id; Render(); });
        }
        var selected = items.Single(x => x.Definition.Id == m_selected);
        m_Detail.Bind(selected, Manager, Claim, id => { m_selected = id; Render(); }, ScreenToken);
    }
    void Claim(string entry)
    {
        var state = Manager.Items.Single(x => x.Definition.Id == m_selected);
        var definition = state.Definition.Entries.Single(x => x.Id == entry);
        string id = m_selected;
        if (definition.CostGold > 0)
            Confirm("ui.activities.exchange_confirm", () => ClaimAsync(id, entry).Forget(), new Dictionary<string, object> { ["amount"] = definition.CostGold });
        else ClaimAsync(id, entry).Forget();
    }
    async UniTaskVoid ClaimAsync(string id, string entry)
    {
        await RunGuardedAsync(async ct =>
        {
            var result = await Manager.ClaimAsync(id, entry, ct);
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
