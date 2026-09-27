using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Configuration;
using AChen.Events;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardWorkshopPanel : MonoBehaviour
{
    [SerializeField] GridListController m_List;
    [SerializeField] CardWorkshopDetail m_Detail;
    [SerializeField] Button m_CraftTab;
    [SerializeField] Button m_DismantleTab;
    [SerializeField] TMP_Text m_CraftLabel;
    [SerializeField] TMP_Text m_DismantleLabel;
    [SerializeField] TMP_InputField m_Search;
    [SerializeField] Toggle m_OnlyCraftable;
    [SerializeField] TMP_Text m_Empty;
    List<WorkshopCardData> m_cards = new List<WorkshopCardData>();
    CancellationTokenSource m_lifetime, m_selection;
    Action<LocalizedMessage, Action> m_confirm;
    Action<LocalizedMessage> m_notice;
    Action<CardDetailEntry> m_inspect;
    readonly Dictionary<int, long> m_liveRewards = new Dictionary<int, long>();
    long m_liveCraft;
    bool m_dismantle, m_busy;
    int m_selected = -1;

    public void Bind(Action<LocalizedMessage, Action> confirm, Action<LocalizedMessage> notice, Action<CardDetailEntry> inspect)
    { m_confirm = confirm; m_notice = notice; m_inspect = inspect; }
    void Awake()
    {
        m_CraftTab.onClick.AddListener(() => Switch(false));
        m_DismantleTab.onClick.AddListener(() => Switch(true));
        m_Search.onValueChanged.AddListener(_ => Refresh());
        m_OnlyCraftable.onValueChanged.AddListener(_ => Refresh());
        m_Detail.Changed += RefreshDetail;
        m_Detail.Submitted += Confirm;
        m_Detail.Inspected += () => m_inspect(m_Detail.Inspection);
    }
    void OnEnable()
    {
        m_lifetime = new CancellationTokenSource();
        m_selection = CancellationTokenSource.CreateLinkedTokenSource(m_lifetime.Token);
        EventCenter.AddListener(GameEvent.PlayerOwnedCardsChanged, OnCardsChanged);
        EventCenter.AddListener(GameEvent.PlayerUrChanged, OnUrChanged);
        Refresh();
    }
    void OnDisable()
    {
        EventCenter.RemoveListener(GameEvent.PlayerOwnedCardsChanged, OnCardsChanged);
        EventCenter.RemoveListener(GameEvent.PlayerUrChanged, OnUrChanged);
        m_lifetime.Cancel(); m_lifetime.Dispose();
        m_selection.Cancel(); m_selection.Dispose();
    }
    void OnCardsChanged(PlayerData _) => Refresh();
    void OnUrChanged(long? amount)
    { if (amount.HasValue) RefreshDetail(); else gameObject.SetActive(false); }
    void Switch(bool dismantle) { m_dismantle = dismantle; m_selected = -1; Refresh(); }
    void Refresh()
    {
        string previous = m_selected >= 0 ? m_cards[m_selected].CardId : string.Empty;
        var player = PlayerSession.Instance.CurrentPlayer;
        string search = m_Search.text.Trim();
        m_cards = LocalGameConfiguration.Data.AllCards.OrderBy(x => x.CardId, StringComparer.Ordinal)
            .Select(x => new WorkshopCardData(x.CardId, x.SourcePool,
                player.OwnedCards.Where(c => c.CardId == x.CardId && c.Rarity == 0).Sum(c => c.Count),
                player.OwnedCards.Where(c => c.CardId == x.CardId).Sum(c => c.Count), m_dismantle))
            .Where(x => m_dismantle ? x.TotalOwned > 0 : !m_OnlyCraftable.isOn || x.NormalOwned == 0)
            .Where(x => x.CardId.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                LocalizationService.GetText("card." + x.CardId + ".name").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        m_OnlyCraftable.gameObject.SetActive(!m_dismantle);
        m_CraftLabel.color = !m_dismantle ? new Color(.86f, 1f, .35f) : Color.white;
        m_DismantleLabel.color = m_dismantle ? new Color(.86f, 1f, .35f) : Color.white;
        m_Empty.gameObject.SetActive(m_cards.Count == 0);
        m_selected = m_cards.FindIndex(x => x.CardId == previous);
        if (m_selected < 0 && m_cards.Count > 0) m_selected = 0;
        m_List.InitList(AddressKeys.Prefab.CardWorkshopRow, m_cards, Select, m_selected, m_lifetime.Token).Forget();
        m_Detail.gameObject.SetActive(m_selected >= 0);
        if (m_selected >= 0) Select(m_selected);
    }
    void Select(int index)
    {
        m_selected = index;
        int rarity = m_dismantle ? PlayerSession.Instance.CurrentPlayer.OwnedCards
            .Where(x => x.CardId == m_cards[index].CardId && x.Count > 0).Min(x => x.Rarity) : 0;
        m_Detail.ResetSelection(rarity);
        m_selection.Cancel(); m_selection.Dispose();
        m_selection = CancellationTokenSource.CreateLinkedTokenSource(m_lifetime.Token);
        m_Detail.SetCardAsync(m_cards[index], m_selection.Token).Forget();
        RefreshDetail();
    }
    long UnitAmount() => m_dismantle
        ? (m_liveRewards.TryGetValue(m_Detail.Rarity, out long value) ? value : LocalGameConfiguration.CardEconomy.DismantleUr(m_Detail.Rarity))
        : (m_liveCraft > 0 ? m_liveCraft : LocalGameConfiguration.CardEconomy.CraftCostUr);
    long Amount() => checked(UnitAmount() * (m_dismantle ? m_Detail.Quantity : 1));
    void RefreshDetail()
    {
        if (m_selected < 0) return;
        var player = PlayerSession.Instance.CurrentPlayer;
        var owned = Enumerable.Range(0, 5).Select(r => player.OwnedCards.Where(x => x.CardId == m_cards[m_selected].CardId && x.Rarity == r).Sum(x => x.Count)).ToArray();
        m_Detail.Display(m_dismantle, owned, Amount(), m_busy);
        m_CraftTab.interactable = m_DismantleTab.interactable = m_Search.interactable = m_OnlyCraftable.interactable = !m_busy;
    }
    void Confirm() => ConfirmAsync().Forget();
    async UniTask ConfirmAsync()
    {
        string id = m_cards[m_selected].CardId;
        int rarity = m_Detail.Rarity, count = m_dismantle ? m_Detail.Quantity : 1;
        long amount = Amount(); bool dismantle = m_dismantle;
        if (!dismantle && PlayerSession.Instance.CurrentPlayer.Ur < amount)
        {
            m_notice(new LocalizedMessage("err.insufficient_ur"));
            return;
        }
        if (dismantle)
        {
            m_busy = true; RefreshDetail();
            var ct = m_lifetime.Token;
            try
            {
                var decks = await PlayerSession.Instance.GetDecksAsync(ct);
                int remaining = PlayerSession.Instance.CurrentPlayer.OwnedCards.Where(x => x.CardId == id && x.Rarity == rarity).Sum(x => x.Count) - count;
                var blocked = decks.Where(d => d.MainDeck.Concat(d.ExtraDeck).Where(x => x.CardId == id && x.Rarity == rarity).Sum(x => (long)x.Count) > remaining).Select(d => d.Name).ToArray();
                if (blocked.Length > 0)
                {
                    m_notice(new LocalizedMessage("ui.workshop.deck_blocked", new Dictionary<string, object> { ["decks"] = string.Join("、", blocked) }));
                    return;
                }
            }
            catch (BackendApiException ex) { m_notice(ex.UserMessage); return; }
            catch (OperationCanceledException) { return; }
            finally { m_busy = false; if (!ct.IsCancellationRequested) RefreshDetail(); }
        }
        m_confirm(new LocalizedMessage(dismantle ? "ui.workshop.confirm_dismantle" : "ui.workshop.confirm_craft",
            new Dictionary<string, object> { ["name"] = new LocalizedMessage("card." + id + ".name"), ["version"] = new LocalizedMessage("ui.rarity." + rarity),
                ["count"] = count, ["amount"] = amount, ["balance"] = PlayerSession.Instance.CurrentPlayer.Ur,
                ["after"] = dismantle ? checked(PlayerSession.Instance.CurrentPlayer.Ur + amount) : PlayerSession.Instance.CurrentPlayer.Ur - amount }),
            () => Transact(id, rarity, count, amount, dismantle).Forget());
    }
    async UniTask Transact(string id, int rarity, int count, long amount, bool dismantle)
    {
        m_busy = true; RefreshDetail();
        var ct = m_lifetime.Token;
        try
        {
            if (dismantle) await PlayerSession.Instance.DismantleCardAsync(id, rarity, count, amount, ct);
            else await PlayerSession.Instance.CraftCardAsync(id, amount, ct);
        }
        catch (BackendApiException ex)
        {
            if (ex.Code == "CARD_PRICE_CHANGED")
            {
                long actual = long.Parse(ex.Errors["urAmount"][0], System.Globalization.CultureInfo.InvariantCulture);
                if (dismantle) m_liveRewards[rarity] = actual / count; else m_liveCraft = actual;
            }
            m_notice(ex.Code == "CARD_IN_DECK"
                ? new LocalizedMessage("ui.workshop.deck_blocked", new Dictionary<string, object> { ["decks"] = string.Join("、", ex.Errors["decks"]) })
                : ex.UserMessage);
        }
        catch (OperationCanceledException) { }
        finally
        {
            m_busy = false;
            if (!ct.IsCancellationRequested) Refresh();
        }
    }
}
