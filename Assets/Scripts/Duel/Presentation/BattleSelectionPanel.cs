using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using AChen.Duel.Presentation;
using UnityEngine.UI;

public sealed class BattleSelectionProperties : IPanelProperties
{
    public BattleSceneController Scene { get; }
    public ZoneRef? BrowseZone { get; }
    public IReadOnlyList<ZonePreviewCard> Cards { get; }
    public BattleSelectionProperties(BattleSceneController scene) { Scene = scene; }
    public BattleSelectionProperties(BattleSceneController scene, ZoneRef zone, IReadOnlyList<ZonePreviewCard> cards)
    { Scene = scene; BrowseZone = zone; Cards = cards; }
}
public sealed class BattleSelectionPanel : APanelController<BattleSelectionProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] TextMeshProUGUI m_TxtTitle;
    [SerializeField] TextMeshProUGUI m_TxtCount;
    [SerializeField] TextMeshProUGUI m_TxtPage;
    [SerializeField] Button m_BtnPrevious;
    [SerializeField] TextMeshProUGUI m_TxtPrevious;
    [SerializeField] Button m_BtnNext;
    [SerializeField] TextMeshProUGUI m_TxtNext;
    [SerializeField] Button m_BtnCancel;
    [SerializeField] TextMeshProUGUI m_TxtCancel;
    [SerializeField] Button m_BtnConfirm;
    [SerializeField] TextMeshProUGUI m_TxtConfirm;
    [SerializeField] Button m_BtnReset;
    [SerializeField] TextMeshProUGUI m_TxtReset;
    // --tag_end: 自动生成--
    [SerializeField] TextMeshProUGUI m_title, m_count, m_page, m_confirmLabel;
    [SerializeField] UnityEngine.UI.Button m_confirm, m_cancel, m_previous, m_next, m_reset;
    [SerializeField] TMP_InputField m_search;
    [SerializeField] BattleSelectionItem m_itemPrefab;
    [SerializeField] RectTransform m_content;
    [SerializeField] UnityEngine.UI.ScrollRect m_scroll;
    readonly List<string> m_selected = new List<string>();
    readonly List<BattleSelectionItem> m_items = new List<BattleSelectionItem>();
    DuelSelectionView m_choice = DuelSelectionView.Empty;
    DuelSelectionOption[] m_filtered = Array.Empty<DuelSelectionOption>();
    bool Browsing => Properties.BrowseZone.HasValue;
    protected override float TransitionDuration => 0;
    protected override void OnResume() { Unbind(); OnOpen(); }
    protected override void OnOpen()
    {
        m_confirm.onClick.AddListener(Confirm); m_cancel.onClick.AddListener(Cancel);
        m_previous.gameObject.SetActive(false); m_next.gameObject.SetActive(false);
        m_page.gameObject.SetActive(false); m_reset.gameObject.SetActive(false); m_count.gameObject.SetActive(false);
        m_confirm.gameObject.SetActive(!Browsing);
        m_cancel.gameObject.SetActive(true);
        m_TxtCancel.text = Browsing ? "关闭" : "取消";
        m_search.onValueChanged.AddListener(Filter);
        Properties.Scene.Source.Changed += Changed;
        if (Browsing)
        {
            var zone = Properties.BrowseZone.Value;
            m_title.text = (zone.Player == 0 ? "我方 · " : "对方 · ") + BattleLabels.Zone(zone.Kind) + " · " + Properties.Cards.Sum(c => c.Count);
            m_cancel.interactable = true; m_search.gameObject.SetActive(false);
            m_filtered = Properties.Cards.SelectMany(c => Enumerable.Range(0, c.Count).Select(i => new DuelSelectionOption(c.CardId + ":" + i, c.Known ? "" : "隐藏卡牌", c.Known ? c.DefinitionId : "", c.CardId))).ToArray();
            Draw();
        }
        else Refresh();
    }
    void Changed(DuelViewChange change)
    {
        if (Browsing) return;
        if (change.Kind != DuelChangeKind.Timer && Properties.Scene.Source.Current.Choice.Active) Refresh();
        if (change.Kind == DuelChangeKind.Rejected) m_title.text = m_choice.Prompt + "\n" + change.Message;
    }
    void Refresh()
    {
        var choice = Properties.Scene.Source.Current.Choice;
        if (choice.Id == m_choice.Id) return;
        m_choice = choice; m_selected.Clear();
        m_title.text = choice.Prompt; m_cancel.interactable = choice.CanCancel;
        m_search.gameObject.SetActive(choice.Searchable); m_search.SetTextWithoutNotify("");
        ((RectTransform)m_scroll.transform).anchoredPosition = new Vector2(0, choice.Searchable ? -80 : -50);
        Filter("");
    }
    void Filter(string query)
    {
        m_filtered = m_choice.Options.Where(o => o.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToArray(); Draw();
    }
    void Draw()
    {
        foreach (var item in m_items) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
        m_items.Clear();
        foreach (var option in m_filtered)
        {
            var item = Instantiate(m_itemPrefab, m_content);
            item.transform.localScale = Vector3.one;
            item.Bind(Properties.Scene, option, () => Select(option, item)); m_items.Add(item);
            item.SetSelected(m_selected.IndexOf(option.Key) + 1);
        }
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(m_content);
        m_scroll.horizontalNormalizedPosition = 0;
        if (!Browsing) Count();
    }
    void Select(DuelSelectionOption option, BattleSelectionItem item)
    {
        if (Browsing)
        {
            if (option.DefinitionId.Length == 0) return;
            Properties.Scene.InspectChoice(option.DefinitionId);
            if (option.CardId != 0 && Properties.Scene.Source.Mode != DuelSessionMode.Replay)
                Properties.Scene.ShowCardActions(option.CardId, item.ScreenAnchor(Properties.Scene.BattleUICamera));
            return;
        }
        if (m_selected.Contains(option.Key)) m_selected.Remove(option.Key);
        else
        {
            if (m_choice.Max == 1) m_selected.Clear();
            if (m_selected.Count >= m_choice.Max) return;
            m_selected.Add(option.Key);
        }
        if (m_choice.PreviewAttack) Properties.Scene.Source.Submit(new PreviewDuelTarget(m_selected.Count == 0 ? -1 : option.CardId));
        if (option.DefinitionId.Length > 0) Properties.Scene.InspectChoice(option.DefinitionId);
        for (int i = 0; i < m_items.Count; i++) m_items[i].SetSelected(m_selected.IndexOf(m_filtered[i].Key) + 1);
        Count();
    }
    void Count()
    {
        m_confirm.interactable = m_selected.Count >= m_choice.Min && m_selected.Count <= m_choice.Max;
        m_confirmLabel.color = m_confirm.interactable ? new Color(.8f, 1, 0) : new Color(.35f, .43f, .06f);
        m_title.text = m_choice.Prompt + " " + m_selected.Count + "/" + m_choice.Max + (m_choice.Ordered ? "（按点击顺序）" : "");
    }
    void Confirm() => Properties.Scene.Source.Submit(new ConfirmDuelSelection(m_choice.Id, m_selected));
    void Cancel() { if (Browsing) Properties.Scene.CloseZone(); else Properties.Scene.CancelAction(); }
    protected override void OnHide() => Unbind();
    protected override void OnClose() => Unbind();
    void Unbind()
    {
        Properties.Scene.Source.Changed -= Changed;
        m_confirm.onClick.RemoveAllListeners(); m_cancel.onClick.RemoveAllListeners(); m_search.onValueChanged.RemoveAllListeners();
        foreach (var item in m_items) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
        m_items.Clear(); m_choice = DuelSelectionView.Empty;
    }
}
