using System;
using System.Collections.Generic;
using System.Threading;
using AChen.Networking;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class CardWorkshopDetail : MonoBehaviour
{
    [SerializeField] RawImage m_Card;
    [SerializeField] Material[] m_Materials;
    [SerializeField] LocalizedText m_Title;
    [SerializeField] LocalizedText m_Owned;
    [SerializeField] LocalizedText m_Price;
    [SerializeField] TMP_Text m_Quantity;
    [SerializeField] LocalizedText m_ActionLabel;
    [SerializeField] Button[] m_Versions;
    [SerializeField] TMP_Text[] m_VersionLabels;
    [SerializeField] Button m_Minus;
    [SerializeField] Button m_Plus;
    [SerializeField] Button m_Action;
    [SerializeField] Button m_Inspect;
    [SerializeField] GameObject m_QuantityControls;
    [SerializeField] GameObject m_VersionControls;
    public int Rarity { get; private set; }
    public int Quantity { get; private set; } = 1;
    public CardDetailEntry Inspection => new CardDetailEntry(m_cardId, m_pool, m_Card.texture, Rarity);
    public event Action Changed;
    public event Action Submitted;
    public event Action Inspected;
    int m_available;
    string m_cardId, m_pool;
    void Awake()
    {
        for (int i = 0; i < m_Versions.Length; i++)
        { int version = i; m_Versions[i].onClick.AddListener(() => { Rarity = version; Quantity = 1; m_Card.material = m_Materials[Rarity]; Changed(); }); }
        m_Minus.onClick.AddListener(() => { Quantity = Math.Max(1, Quantity - 1); Changed(); });
        m_Plus.onClick.AddListener(() => { Quantity = Math.Min(m_available, Quantity + 1); Changed(); });
        m_Action.onClick.AddListener(() => Submitted());
        m_Inspect.onClick.AddListener(() => Inspected());
    }
    public void ResetSelection(int rarity) { Rarity = rarity; Quantity = 1; m_Card.material = m_Materials[Rarity]; }
    public async UniTask SetCardAsync(WorkshopCardData card, CancellationToken ct)
    {
        m_cardId = card.CardId;
        m_pool = card.SourcePool;
        m_Title.SetKey("card." + card.CardId + ".name");
        m_Card.texture = null;
        var texture = await CardPoolAddress.LoadCardTextureAsync(card.SourcePool, card.CardId);
        ct.ThrowIfCancellationRequested();
        m_Card.texture = texture;
    }
    public void Display(bool dismantle, int[] owned, long amount, bool busy)
    {
        m_available = owned[Rarity];
        Quantity = Math.Max(1, Math.Min(Quantity, m_available));
        for (int i = 0; i < m_Versions.Length; i++)
        {
            m_Versions[i].interactable = !busy;
            m_VersionLabels[i].text = LocalizationService.GetText("ui.rarity." + i) + " ×" + owned[i];
            m_VersionLabels[i].color = Rarity == i ? new Color(.86f, 1f, .35f) : Color.white;
        }
        m_VersionControls.SetActive(dismantle);
        m_QuantityControls.SetActive(dismantle);
        m_Quantity.text = Quantity.ToString();
        m_Owned.SetKey("ui.workshop.owned_version_and_limit", new Dictionary<string, object> { ["version"] = new LocalizedMessage("ui.rarity." + Rarity), ["count"] = m_available,
            ["limit"] = new LocalizedMessage("ui.workshop.limit." + LocalGameConfiguration.DeckRules.GetMaxCopies(m_cardId)) });
        m_Price.SetKey(dismantle ? "ui.workshop.reward" : "ui.workshop.cost", new Dictionary<string, object> { ["amount"] = amount });
        m_ActionLabel.SetKey(dismantle ? "ui.workshop.dismantle" : "ui.workshop.craft");
        m_Action.interactable = !busy && (!dismantle || m_available > 0);
        m_Plus.interactable = !busy && Quantity < m_available;
        m_Minus.interactable = !busy && Quantity > 1;
    }
}
