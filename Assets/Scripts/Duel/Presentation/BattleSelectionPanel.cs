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
    public BattleSelectionProperties(BattleSceneController scene) { Scene = scene; }
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
    readonly List<string> m_selected = new List<string>();
    readonly List<BattleSelectionItem> m_items = new List<BattleSelectionItem>();
    DuelSelectionView m_choice = DuelSelectionView.Empty;
    DuelSelectionOption[] m_filtered = Array.Empty<DuelSelectionOption>();
    int m_index;
    float m_width;
    const int PageSize = 6;
    protected override float TransitionDuration => 0;
    protected override void OnResume() { Unbind(); OnOpen(); }
    protected override void OnOpen()
    {
        m_confirm.onClick.AddListener(Confirm); m_cancel.onClick.AddListener(Cancel);
        m_reset.onClick.AddListener(() => Properties.Scene.Source.Submit(new ResetDuel()));
        m_previous.onClick.AddListener(() => { m_index--; Draw(); });
        m_next.onClick.AddListener(() => { m_index++; Draw(); });
        m_search.onValueChanged.AddListener(Filter);
        Properties.Scene.Source.Changed += Changed;
        Refresh();
    }
    void Changed(DuelViewChange change)
    {
        if (change.Kind != DuelChangeKind.Timer && Properties.Scene.Source.Current.Choice.Active) Refresh();
        if (change.Kind == DuelChangeKind.Rejected) m_title.text = m_choice.Prompt + "\n" + change.Message;
    }
    void Refresh()
    {
        var choice = Properties.Scene.Source.Current.Choice;
        if (choice.Id == m_choice.Id) return;
        m_choice = choice; m_selected.Clear(); m_index = 0;
        m_title.text = choice.Prompt; m_cancel.interactable = choice.CanCancel;
        m_search.gameObject.SetActive(choice.Searchable); m_search.SetTextWithoutNotify("");
        m_content.anchoredPosition=new Vector2(0,choice.Searchable?-80:-50); Filter("");
    }
    void Filter(string query)
    {
        m_filtered = m_choice.Options.Where(o => o.Label.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0).ToArray();
        m_index = 0; Draw();
    }
    void Draw()
    {
        m_width=m_content.rect.width;
        foreach (var item in m_items) Destroy(item.gameObject);
        m_items.Clear();
        var page = m_filtered.Skip(m_index * PageSize).Take(PageSize).ToArray();
        for (int i = 0; i < page.Length; i++)
        {
            var option = page[i]; var item = Instantiate(m_itemPrefab, m_content);
            float scale=Mathf.Min(.9f,m_content.rect.width/(175f*Mathf.Max(1,page.Length)));float cell=175f*scale;
            item.transform.localScale=Vector3.one*scale;
            ((RectTransform)item.transform).anchoredPosition=new Vector2((m_content.rect.width-page.Length*cell)*.5f+i*cell,0);
            item.Bind(Properties.Scene, option, () => Select(option)); m_items.Add(item);
            item.SetSelected(m_selected.IndexOf(option.Key) + 1);
        }
        bool pages=m_filtered.Length>PageSize;m_previous.gameObject.SetActive(pages);m_next.gameObject.SetActive(pages);m_page.gameObject.SetActive(pages);
        m_previous.interactable = m_index > 0;
        m_next.interactable = (m_index + 1) * PageSize < m_filtered.Length;
        m_page.text = (m_index + 1) + " / " + Math.Max(1, (m_filtered.Length + PageSize - 1) / PageSize);
        Count();
    }
    void Select(DuelSelectionOption option)
    {
        if (m_selected.Contains(option.Key)) m_selected.Remove(option.Key);
        else
        {
            if (m_choice.Max == 1) m_selected.Clear();
            if (m_selected.Count >= m_choice.Max) return;
            m_selected.Add(option.Key);
        }
        if (m_choice.PreviewAttack) Properties.Scene.Source.Submit(new PreviewDuelTarget(m_selected.Count == 0 ? -1 : option.CardId));
        if (option.DefinitionId.Length > 0) Properties.Scene.InspectChoice(option.DefinitionId);
        for (int i = 0; i < m_items.Count; i++)
            m_items[i].SetSelected(m_selected.IndexOf(m_filtered[m_index * PageSize + i].Key) + 1);
        Count();
    }
    void Count()
    {
        m_confirm.interactable = m_selected.Count >= m_choice.Min && m_selected.Count <= m_choice.Max;
        m_confirmLabel.color=m_confirm.interactable?new Color(.8f,1,0):new Color(.35f,.43f,.06f);
        m_count.gameObject.SetActive(false);
        m_title.text=m_choice.Prompt+" "+m_selected.Count+"/"+m_choice.Max+(m_choice.Ordered?"（按点击顺序）":"");
        m_count.text = "已选择 " + m_selected.Count + " / " + (m_choice.Min == m_choice.Max ? m_choice.Min.ToString() : m_choice.Min + "–" + m_choice.Max)
            + (m_choice.Ordered ? " · 按点击顺序排列" : "");
    }
    void Confirm() => Properties.Scene.Source.Submit(new ConfirmDuelSelection(m_choice.Id, m_selected));
    void LateUpdate() { if(m_choice.Active && !Mathf.Approximately(m_width,m_content.rect.width))Draw(); }
    void Cancel() => Properties.Scene.CancelAction();
    protected override void OnHide() => Unbind();
    protected override void OnClose() => Unbind();
    void Unbind()
    {
        Properties.Scene.Source.Changed -= Changed;
        m_confirm.onClick.RemoveAllListeners(); m_cancel.onClick.RemoveAllListeners();
        m_previous.onClick.RemoveAllListeners(); m_next.onClick.RemoveAllListeners(); m_search.onValueChanged.RemoveAllListeners();
        m_reset.onClick.RemoveAllListeners();
        foreach (var item in m_items) Destroy(item.gameObject); m_items.Clear(); m_choice = DuelSelectionView.Empty;
    }
}
