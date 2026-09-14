using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopCardItemData
{
    public int Id { get; }
    public string Title { get; }
    public Sprite MainSprite { get; }
    public string PoolKey { get; }
    public long PriceGold { get; }
    public DateTimeOffset? EndsAt { get; }
    public int Index { get; }

    public ShopCardItemData(
        int id,
        string title,
        Sprite mainSprite,
        string poolKey,
        long priceGold,
        DateTimeOffset? endsAt,
        int index)
    {
        Id = id;
        Title = title;
        MainSprite = mainSprite;
        PoolKey = poolKey ?? string.Empty;
        PriceGold = priceGold;
        EndsAt = endsAt;
        Index = index;
    }
}

public class ShopCardItem : MonoBehaviour
{
    // --tag_start: 自动生成--
    [SerializeField] Button m_BtnAll;
    [SerializeField] TextMeshProUGUI m_TxtTitile;
    [SerializeField] Image m_ImgMain;
    [SerializeField] TextMeshProUGUI m_TxtValue;
    [SerializeField] TextMeshProUGUI m_TxtRemainTime;
    // --tag_end: 自动生成--
    ShopCardItemData m_Data;
    Action<int> m_OnSelected;

    public void SetData(ShopCardItemData data, Action<int> onSelected)
    {
        m_Data = data;
        m_OnSelected = onSelected;
        m_TxtTitile.Localized().SetKey("shop.pack." + data.Id.ToString("D2"));
        m_ImgMain.sprite = data.MainSprite;
        m_TxtValue.Localized().SetKey("ui.common.gold_amount", new System.Collections.Generic.Dictionary<string, object> { ["gold"] = data.PriceGold.ToString("N0") });
        ShopRemainingTime.Apply(m_TxtRemainTime.Localized(), data.EndsAt);
    }

    private void Awake() {
        m_BtnAll.onClick.AddListener(OnSelectedClick);
    }
    private void OnDestroy() {
        m_BtnAll.onClick.RemoveListener(OnSelectedClick);
    }
    private void OnSelectedClick() {
        if (m_Data == null) return;
        ALog.Log(
            $"购买卡包点击: Id={m_Data.Id}; Title={m_Data.Title}; PriceGold={m_Data.PriceGold}.",
            ALogCategories.UI);
        m_OnSelected?.Invoke(m_Data.Index);
    }
}
