using System;
using System.Threading;
using AChen.Configuration;
using AChen.Networking;
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
    CancellationTokenSource m_lifetime = new();
    Action<int> m_onSelected;
    int m_index, m_version;
    void Awake() => m_Select.onClick.AddListener(Select);
    // 父物体 OnEnable 会先绑定列表，此时子项本次 OnEnable 还没执行，不能留着已释放的源。
    void OnDisable()
    {
        m_lifetime.Cancel();
        m_lifetime.Dispose();
        m_lifetime = new CancellationTokenSource();
    }
    void OnDestroy()
    {
        m_lifetime.Cancel();
        m_lifetime.Dispose();
    }
    void Select() => m_onSelected(m_index);
    public void Bind(WorkshopCardData card, int index, bool selected, Action<int> onSelected)
    {
        m_index = index; m_onSelected = onSelected;
        m_CardName.SetKey("card." + card.CardId + ".name");
        int limit = LocalGameConfiguration.DeckRules.GetMaxCopies(card.CardId);
        m_Owned.SetKey("ui.workshop.owned_and_limit",
            new System.Collections.Generic.Dictionary<string, object> { ["count"] = card.Dismantle ? card.TotalOwned : card.NormalOwned,
                ["limit"] = new LocalizedMessage("ui.workshop.limit." + limit) });
        m_Dim.enabled = false;
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
