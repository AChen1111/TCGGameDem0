using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using AChen.Decks;
using AChen.Player;
using Cysharp.Threading.Tasks;

public class DeckListWindow : AWindowController
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnBack;
    [SerializeField] TextMeshProUGUI m_TxtTotal;
    [SerializeField] Button m_BtnNew;
    [SerializeField] GameObject m_GoEmpty;
    // --tag_end: 自动生成--

    [SerializeField] RectTransform m_Content;
    [SerializeField] DeckListItem m_ItemPrefab;
    [SerializeField] CanvasGroup m_Controls;
    bool m_busy;
    protected override void AddListeners()
    { m_BtnBack.onClick.AddListener(UI_Close); m_BtnNew.onClick.AddListener(Create); }
    protected override void RemoveListeners()
    { m_BtnBack.onClick.RemoveListener(UI_Close); m_BtnNew.onClick.RemoveListener(Create); }
    protected override void OnOpen() => Reload().Forget();
    protected override void OnResume() => Reload().Forget();
    async UniTask Reload()
    {
        if (m_busy) return;
        SetBusy(true);
        await RunGuardedAsync(async ct =>
        {
            var decks = await PlayerSession.Instance.GetDecksAsync(ct);
            foreach (Transform child in m_Content) Destroy(child.gameObject);
            foreach (var deck in decks) Instantiate(m_ItemPrefab, m_Content).Bind(deck, this);
            m_TxtTotal.text = decks.Count.ToString();
            m_GoEmpty.SetActive(decks.Count == 0);
        }, "读取卡组", "err.deck.load");
        if (IsOpened) SetBusy(false);
    }
    void SetBusy(bool busy) { m_busy = busy; m_Controls.interactable = !busy; }
    public void Edit(Guid id) => RequestOpenWindow(AddressKeys.Prefab.DeckEditWindow, new DeckEditWindowProperties(id));
    void Create()
    {
        Guid created = Guid.Empty;
        RequestOpenWindow(AddressKeys.Prefab.DeckNameWindow, new DeckNameWindowProperties(string.Empty, false,
            async (name, ct) => { created = (await PlayerSession.Instance.CreateDeckAsync(name, ct)).Id; }, () => Edit(created)));
    }
    public void Delete(DeckData deck) => Confirm("ui.deck.delete_confirm", () => DeleteAsync(deck).Forget(),
        new System.Collections.Generic.Dictionary<string, object> { ["name"] = deck.Name });
    async UniTask DeleteAsync(DeckData deck)
    {
        SetBusy(true);
        await RunGuardedAsync(ct => PlayerSession.Instance.DeleteDeckAsync(deck.Id, deck.Revision, ct), "删除卡组", "err.deck.delete");
        if (!IsOpened) return;
        SetBusy(false);
        await Reload();
    }
}
