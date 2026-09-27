using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Configuration;
using AChen.Decks;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;

public sealed class DeckEditWindowProperties : IWindowProperties
{
    public Guid Id { get; }
    public DeckEditWindowProperties(Guid id) { Id = id; }
}

public class DeckEditWindow : AWindowController<DeckEditWindowProperties>
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnBack;
    [SerializeField] Button m_BtnSave;
    [SerializeField] GameObject m_GoDetail;
    [SerializeField] TextMeshProUGUI m_TxtDetailName;
    [SerializeField] TextMeshProUGUI m_TxtDetailStats;
    [SerializeField] TextMeshProUGUI m_TxtRarity;
    [SerializeField] TextMeshProUGUI m_TxtDetailType;
    [SerializeField] TextMeshProUGUI m_TxtDetailDesc;
    [SerializeField] Button m_BtnPlus;
    [SerializeField] Button m_BtnMinus;
    [SerializeField] TextMeshProUGUI m_TxtName;
    [SerializeField] Button m_BtnRename;
    [SerializeField] TextMeshProUGUI m_TxtMainCount;
    [SerializeField] GameObject m_GoEmpty;
    [SerializeField] TextMeshProUGUI m_TxtExtraCount;
    [SerializeField] TextMeshProUGUI m_TxtStatus;
    [SerializeField] TMP_InputField m_InpSearch;
    [SerializeField] TextMeshProUGUI m_TxtResults;
    // --tag_end: 自动生成--

    [SerializeField] GridListController m_Pool;
    [SerializeField] ScrollRect m_PoolScroll;
    [SerializeField] ScrollRect m_MainScroll;
    [SerializeField] ScrollRect m_ExtraScroll;
    [SerializeField] DeckCardCell m_DeckCellPrefab;
    [SerializeField] DeckCardView m_DetailCard;
    [SerializeField] Button m_BtnDetailCard;
    [SerializeField] DeckCardView m_DragCard;
    [SerializeField] RectTransform m_DragRoot;
    [SerializeField] RectTransform m_CanvasRect;
    [SerializeField] CanvasGroup m_Controls;
    [SerializeField] Image m_Attribute;
    [SerializeField] Sprite[] m_Attributes;
    [SerializeField] Image m_DetailHeader;
    [SerializeField] Graphic[] m_DropHighlights;
    [SerializeField] GameObject m_PoolEmpty;
    DeckEditorState m_state;
    List<DeckCardData> m_pool = new();
    DeckCardData m_selected, m_dragged;
    bool m_ready, m_busy, m_dragging;
    Action m_afterUnsaved;
    readonly List<DeckCardCell> m_mainCells = new(), m_extraCells = new();
    readonly List<CardMove> m_cardMoves = new();
    sealed class CardMove
    {
        public DeckCardData Data;
        public int CopyIndex;
        public DeckCardView View;
        public MotionHandle Handle;
    }
    protected override void AddListeners()
    {
        m_BtnBack.onClick.AddListener(Back);
        m_BtnSave.onClick.AddListener(Save);
        m_BtnRename.onClick.AddListener(Rename);
        m_BtnPlus.onClick.AddListener(AddSelected);
        m_BtnMinus.onClick.AddListener(RemoveSelected);
        m_BtnDetailCard.onClick.AddListener(InspectSelected);
        m_InpSearch.onValueChanged.AddListener(Search);
    }
    protected override void RemoveListeners()
    {
        m_BtnBack.onClick.RemoveListener(Back);
        m_BtnSave.onClick.RemoveListener(Save);
        m_BtnRename.onClick.RemoveListener(Rename);
        m_BtnPlus.onClick.RemoveListener(AddSelected);
        m_BtnMinus.onClick.RemoveListener(RemoveSelected);
        m_BtnDetailCard.onClick.RemoveListener(InspectSelected);
        m_InpSearch.onValueChanged.RemoveListener(Search);
    }
    protected override void OnOpen()
    {
        m_ready = false; m_selected = null; m_busy = false;
        m_afterUnsaved = null;
        m_InpSearch.SetTextWithoutNotify(string.Empty);
        m_GoDetail.SetActive(false);
        EventCenter.AddListener(GameEvent.PlayerOwnedCardsChanged, InventoryChanged);
        LocalizationService.LanguageChanged += LanguageChanged;
        Load().Forget();
    }
    protected override void OnResume()
    {
        if (m_ready) Refresh();
        // The frame restores this window after the popup's closing transition finishes.
        var continuation = m_afterUnsaved;
        m_afterUnsaved = null;
        continuation?.Invoke();
    }
    protected override void OnHide() { StopCardMoves(); EndCardDrag(); }
    protected override void OnClose()
    {
        StopCardMoves(); EndCardDrag(); m_ready = false;
        EventCenter.RemoveListener(GameEvent.PlayerOwnedCardsChanged, InventoryChanged);
        LocalizationService.LanguageChanged -= LanguageChanged;
    }
    void LanguageChanged() { if (m_ready) Refresh(); }
    void InventoryChanged(PlayerData _) { if (m_ready) Refresh(); }
    async UniTask Load()
    {
        Busy(true);
        bool loaded = await RunGuardedAsync(async ct =>
        {
            m_state = new DeckEditorState(await PlayerSession.Instance.GetDeckAsync(Properties.Id, ct));
            m_ready = true; Refresh();
        }, "读取卡组", "err.deck.load");
        if (IsOpened) { Busy(false); if (!loaded) UI_Close(); }
    }
    void Busy(bool busy)
    {
        m_busy = busy; m_Controls.interactable = !busy;
        if (m_ready) RefreshButtons();
    }
    DeckCardEntry[] Inventory() => PlayerSession.Instance.CurrentPlayer.OwnedCards
        .Select(x => new DeckCardEntry(x.CardId, x.Rarity, x.Count)).ToArray();
    void Search(string _) { if (m_ready) RefreshPool(); }
    void Refresh()
    {
        var deck = m_state.Draft.ToData();
        m_TxtName.text = deck.Name + (m_state.IsDirty ? " *" : string.Empty);
        m_TxtMainCount.text = deck.MainDeck.Sum(x => x.Count) + " / 60";
        m_TxtExtraCount.text = deck.ExtraDeck.Sum(x => x.Count) + " / 15";
        m_TxtStatus.text = LocalizationService.GetText(deck.MainDeck.Sum(x => x.Count) < 40 ? "ui.deck.incomplete" : "ui.deck.ready");
        m_GoEmpty.SetActive(deck.MainDeck.Count == 0);
        Populate(m_MainScroll.content, deck.MainDeck, m_mainCells);
        Populate(m_ExtraScroll.content, deck.ExtraDeck, m_extraCells);
        RefreshPool(); RefreshButtons();
    }
    void Populate(RectTransform content, IReadOnlyList<DeckCardEntry> entries, List<DeckCardCell> cells)
    {
        // Exclude scheduled-for-destruction children from this frame's grid layout.
        foreach (Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        cells.Clear();
        foreach (var entry in entries.OrderBy(x => x.CardId, StringComparer.Ordinal).ThenBy(x => x.Rarity))
        {
            string pool = LocalGameConfiguration.Data.AllCards.Single(x => x.CardId == entry.CardId).SourcePool;
            var data = new DeckCardData(entry.CardId, pool, entry.Rarity, 0, true, this);
            for (int i = 0; i < entry.Count; i++)
            {
                var cell = Instantiate(m_DeckCellPrefab, content);
                cell.Bind(data, i, false, _ => SelectCard(data));
                cell.View.SetArtVisible(!m_cardMoves.Any(x => x.Data.CardId == entry.CardId && x.Data.Rarity == entry.Rarity && x.CopyIndex == i));
                cells.Add(cell);
            }
        }
    }
    void RefreshPool()
    {
        var inventory = Inventory();
        m_pool = new List<DeckCardData>();
        foreach (var card in LocalGameConfiguration.Data.AllCards.OrderBy(x => x.CardId, StringComparer.Ordinal))
        {
            if (!CardNameSearch.Matches(card.CardId, m_InpSearch.text)) continue;
            var versions = inventory.Where(x => x.CardId == card.CardId && x.Count > 0).OrderBy(x => x.Rarity).ToArray();
            if (versions.Length == 0) m_pool.Add(new DeckCardData(card.CardId, card.SourcePool, 0, 0, false, this));
            foreach (var version in versions) m_pool.Add(new DeckCardData(card.CardId, card.SourcePool, version.Rarity, version.Count, false, this));
        }
        m_TxtResults.text = m_pool.Count.ToString();
        m_PoolEmpty.SetActive(m_pool.Count == 0);
        int selected = m_selected == null ? -1 : m_pool.FindIndex(x => x.CardId == m_selected.CardId && x.Rarity == m_selected.Rarity);
        BindPool(selected).Forget();
    }
    async UniTask BindPool(int selected)
    {
        try { await m_Pool.InitList(AddressKeys.Prefab.DeckCardRow, m_pool, i => SelectCard(m_pool[i]), selected, ScreenToken); }
        catch (OperationCanceledException) { }
    }
    public void BindCard(DeckCardView view, DeckCardData data) => BindCardAsync(view, data).Forget();
    async UniTask BindCardAsync(DeckCardView view, DeckCardData data)
    {
        try { await view.BindAsync(data.SourcePool, data.CardId, data.Rarity, ScreenToken); }
        catch (OperationCanceledException) { }
    }
    public void SelectCard(DeckCardData data)
    {
        m_selected = data; m_GoDetail.SetActive(true);
        CardCatalog.TryGet(data.CardId, out var row);
        m_TxtDetailName.text = LocalizationService.GetText("card." + data.CardId + ".name");
        m_TxtDetailDesc.text = LocalizationService.GetText("card." + data.CardId + ".desc");
        m_TxtDetailType.text = CardCatalog.FormatTypeLine(row);
        m_TxtRarity.text = LocalizationService.GetText("ui.rarity." + data.Rarity);
        m_TxtDetailStats.text = row.Kind == (int)CardKind.Monster
            ? "★ " + row.Level + "\nATK " + CardCatalog.FormatStat(row.Atk) + "\nDEF " + CardCatalog.FormatStat(row.Def) : string.Empty;
        m_Attribute.enabled = row.Attribute > 0;
        if (row.Attribute > 0) m_Attribute.sprite = m_Attributes[row.Attribute - 1];
        m_DetailHeader.color = row.Kind == (int)CardKind.Spell ? new Color(.1f,.45f,.4f)
            : row.Kind == (int)CardKind.Trap ? new Color(.45f,.18f,.38f) : new Color(.5f,.4f,.18f);
        BindCard(m_DetailCard, data); RefreshButtons();
    }
    void InspectSelected() => RequestOpenWindow(AddressKeys.Prefab.CardDetailOverlay, new CardDetailWindowProperty(
        new[] { new CardDetailEntry(m_selected.CardId, m_selected.SourcePool, m_DetailCard.Texture) }, 0));
    void RefreshButtons()
    {
        m_BtnSave.interactable = !m_busy;
        if (m_selected == null) { m_BtnPlus.interactable = m_BtnMinus.interactable = false; return; }
        int owned = Inventory().Where(x => x.CardId == m_selected.CardId && x.Rarity == m_selected.Rarity).Sum(x => x.Count);
        int used = m_state.Count(m_selected.CardId, m_selected.Rarity);
        var deck = m_state.Draft.ToData();
        int totalCopies = deck.MainDeck.Concat(deck.ExtraDeck).Where(x => x.CardId == m_selected.CardId).Sum(x => x.Count);
        LocalGameConfiguration.DeckRules.TryGetSection(m_selected.CardId, out var section);
        int sectionCount = (section == DeckSection.Main ? deck.MainDeck : deck.ExtraDeck).Sum(x => x.Count);
        m_BtnPlus.interactable = !m_busy && used < owned && totalCopies < LocalGameConfiguration.DeckRules.GetMaxCopies(m_selected.CardId)
            && sectionCount < (section == DeckSection.Main ? 60 : 15);
        m_BtnMinus.interactable = !m_busy && used > 0;
    }
    void AddSelected() => Change(m_selected, 1);
    void RemoveSelected() => Change(m_selected, -1);
    bool Change(DeckCardData data, int delta, bool keepCardMoves = false)
    {
        if (m_busy) return false;
        if (delta < 0 && m_state.Count(data.CardId, data.Rarity) == 0) return false;
        var result = m_state.TryChange(data.CardId, data.Rarity, delta, LocalGameConfiguration.DeckRules, Inventory());
        if (!result.IsValid) { ShowIssue(result); return false; }
        if (!keepCardMoves) StopCardMoves();
        Refresh();
        return true;
    }
    public void AddCardFromClick(DeckCardData data, Texture texture, Vector3 worldCenter)
    {
        var start = m_CanvasRect.InverseTransformPoint(worldCenter);
        if (!Change(data, 1, true)) return;
        var move = new CardMove { Data = data, CopyIndex = m_state.Count(data.CardId, data.Rarity) - 1,
            View = Instantiate(m_DragCard, m_CanvasRect) };
        var scroll = ScrollFor(new DeckCardData(data.CardId, data.SourcePool, data.Rarity, 0, true, this));
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scroll.content);
        var landingCell = LandingCard(move);
        var landing = landingCell.View;
        RevealCard(scroll, (RectTransform)landingCell.transform);
        landing.SetArtVisible(false);
        var rect = move.View.ArtRect;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = landing.ArtRect.rect.size;
        rect.localPosition = start;
        rect.SetAsLastSibling();
        move.View.SetTexture(texture, data.Rarity);
        m_cardMoves.Add(move);
        move.Handle = LMotion.Create(0f, 1f, .35f)
            .WithEase(Ease.OutCubic)
            .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
            .WithOnComplete(() =>
            {
                LandingCard(move).View.SetArtVisible(true);
                m_cardMoves.Remove(move);
                Destroy(move.View.gameObject);
            })
            .Bind(progress =>
            {
                var target = LandingCard(move).View.ArtRect;
                var end = m_CanvasRect.InverseTransformPoint(target.TransformPoint(target.rect.center));
                rect.localPosition = Vector3.LerpUnclamped(start, end, progress);
            })
            .AddTo(move.View);
    }
    DeckCardCell LandingCard(CardMove move)
    {
        LocalGameConfiguration.DeckRules.TryGetSection(move.Data.CardId, out var section);
        return (section == DeckSection.Main ? m_mainCells : m_extraCells)
            .Where(x => x.Data.CardId == move.Data.CardId && x.Data.Rarity == move.Data.Rarity).ElementAt(move.CopyIndex);
    }
    static void RevealCard(ScrollRect scroll, RectTransform card)
    {
        scroll.StopMovement();
        var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.viewport, card);
        var position = scroll.content.anchoredPosition;
        if (bounds.min.y < scroll.viewport.rect.yMin) position.y += scroll.viewport.rect.yMin - bounds.min.y;
        if (bounds.max.y > scroll.viewport.rect.yMax) position.y -= bounds.max.y - scroll.viewport.rect.yMax;
        position.y = Mathf.Clamp(position.y, 0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height));
        scroll.content.anchoredPosition = position;
    }
    void StopCardMoves()
    {
        foreach (var move in m_cardMoves) { move.Handle.TryCancel(); Destroy(move.View.gameObject); }
        m_cardMoves.Clear();
        foreach (var cell in m_mainCells) cell.View.SetArtVisible(true);
        foreach (var cell in m_extraCells) cell.View.SetArtVisible(true);
    }
    void ShowIssue(DeckValidationResult result) => ShowMessage("ui.deck.issue." + result.Issues[0].Code);
    void Rename() => RequestOpenWindow(AddressKeys.Prefab.DeckNameWindow, new DeckNameWindowProperties(m_state.Draft.Name, true,
        (name, ct) => { m_state.Draft.Name = name; return UniTask.CompletedTask; }, Refresh));
    void Save() => SaveAsync(false).Forget();
    async UniTask SaveAsync(bool close)
    {
        if (m_busy) return;
        var validation = PlayerSession.Instance.ValidateDeck(m_state.Draft.ToData(), DeckValidationMode.Draft);
        if (!validation.IsValid) { ShowIssue(validation); return; }
        Busy(true);
        var ct = ScreenToken;
        bool saved = false;
        try
        {
            var result = await PlayerSession.Instance.SaveDeckAsync(m_state.Draft, ct);
            ct.ThrowIfCancellationRequested(); m_state.AcceptSaved(result); saved = true;
        }
        catch (OperationCanceledException) { }
        catch (BackendApiException ex)
        {
            if (!ct.IsCancellationRequested)
            {
                if (ex.Code == "DECK_DATA_CHANGED") Confirm("ui.deck.conflict", () => Load().Forget());
                else ShowMessage(ex.UserMessage);
            }
        }
        catch (DeckValidationException ex) { if (!ct.IsCancellationRequested) ShowIssue(ex.Result); }
        catch (Exception ex)
        { if (!ct.IsCancellationRequested) { ALog.LogError("保存卡组失败: " + ex.Message, ALogCategories.UI); ShowMessage("err.deck.save"); } }
        finally { if (!ct.IsCancellationRequested) Busy(false); }
        if (!saved) return;
        if (close) UI_Close();
        else { Refresh(); ShowMessage("ui.deck.saved"); }
    }
    void Back()
    {
        if (m_busy) return;
        if (!m_state.IsDirty) { UI_Close(); return; }
        RequestOpenWindow(AddressKeys.Prefab.DeckUnsavedWindow, new DeckUnsavedWindowProperties(
            () => m_afterUnsaved = () => SaveAsync(true).Forget(),
            () => m_afterUnsaved = UI_Close));
    }
    public ScrollRect ScrollFor(DeckCardData data)
    {
        if (!data.InDeck) return m_PoolScroll;
        LocalGameConfiguration.DeckRules.TryGetSection(data.CardId, out var section);
        return section == DeckSection.Main ? m_MainScroll : m_ExtraScroll;
    }
    public void BeginCardDrag(DeckCardData data, Texture texture, Vector2 pointer)
    {
        if (m_busy) return;
        StopCardMoves();
        m_dragged = data; m_dragging = true;
        m_DragRoot.gameObject.SetActive(true); m_DragCard.SetTexture(texture, data.Rarity);
        m_DropHighlights[data.InDeck ? 1 : 0].enabled = true;
        MoveCardDrag(pointer);
    }
    public void MoveCardDrag(Vector2 pointer)
    {
        if (!m_dragging) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, pointer, m_UIFrame.UICamera, out var position);
        m_DragRoot.localPosition = new Vector3(position.x, position.y, 0);
    }
    public void EndCardDrag()
    {
        m_dragging = false; m_DragRoot.gameObject.SetActive(false);
        foreach (var highlight in m_DropHighlights) highlight.enabled = false;
    }
    public void DropCard(bool intoDeck)
    {
        if (!m_dragging || intoDeck == m_dragged.InDeck) return;
        var data = m_dragged; EndCardDrag(); Change(data, intoDeck ? 1 : -1);
    }
}
