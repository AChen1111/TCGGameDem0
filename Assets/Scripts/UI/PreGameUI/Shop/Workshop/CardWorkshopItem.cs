using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public readonly struct WorkshopCardData
{
    public readonly string CardId, SourcePool;
    public readonly int NormalOwned, TotalOwned;
    public readonly bool Dismantle;
    public WorkshopCardData(string id, string pool, int normal, int total, bool dismantle)
    { CardId = id; SourcePool = pool; NormalOwned = normal; TotalOwned = total; Dismantle = dismantle; }
}

public sealed class CardWorkshopItem : MonoBehaviour
{
    [SerializeField] RawImage m_Card;
    [SerializeField] LocalizedText m_CardName;
    [SerializeField] LocalizedText m_Owned;
    [SerializeField] Image m_Dim;
    [SerializeField] GameObject m_Selected;
    [SerializeField] Button m_Select;
    CancellationTokenSource m_lifetime;
    Action<int> m_onSelected;
    int m_index, m_version;
    void Awake() => m_Select.onClick.AddListener(Select);
    void OnEnable() => m_lifetime = new CancellationTokenSource();
    void OnDisable() { m_lifetime.Cancel(); m_lifetime.Dispose(); }
    void Select() => m_onSelected(m_index);
    public void Bind(WorkshopCardData card, int index, bool selected, Action<int> onSelected)
    {
        m_index = index; m_onSelected = onSelected;
        m_CardName.SetKey("card." + card.CardId + ".name");
        m_Owned.SetKey(card.Dismantle ? "ui.workshop.owned_total" : "ui.workshop.owned_normal",
            new System.Collections.Generic.Dictionary<string, object> { ["count"] = card.Dismantle ? card.TotalOwned : card.NormalOwned });
        m_Dim.enabled = !card.Dismantle && card.NormalOwned > 0;
        m_Selected.SetActive(selected);
        Load(card, ++m_version, m_lifetime.Token).Forget();
    }
    async UniTask Load(WorkshopCardData card, int version, CancellationToken ct)
    {
        m_Card.texture = null;
        var texture = await CardPoolAddress.LoadCardTextureAsync(card.SourcePool, card.CardId);
        if (ct.IsCancellationRequested || version != m_version) return;
        m_Card.texture = texture;
    }
}
