using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>商城可拥有商品的格子数据. 头像与壁纸共用; 头像选择窗也用它展示已拥有状态.</summary>
public class ShopOwnedItemData
{
    public int Id { get; }
    public string Name { get; }
    public string NameKey { get; }
    public Sprite Sprite { get; }
    public long PriceGold { get; }
    public DateTimeOffset? EndsAt { get; }
    public bool Owned { get; }
    public int Index { get; }

    public ShopOwnedItemData(
        int id,
        string name,
        Sprite sprite,
        long priceGold,
        DateTimeOffset? endsAt,
        bool owned,
        int index,
        string catalogType = ShopCatalogTypes.Avatar)
    {
        Id = id;
        Name = name;
        NameKey = "shop." + catalogType + "." + id.ToString("D2");
        Sprite = sprite;
        PriceGold = priceGold;
        EndsAt = endsAt;
        Owned = owned;
        Index = index;
    }
}

/// <summary>商城可拥有商品格子. 头像与壁纸预制体直接挂本脚本.</summary>
public class ShopOwnedItem : MonoBehaviour
{
    [SerializeField] Button m_BtnAll;
    [SerializeField] Image m_ImgMain;
    [SerializeField] TextMeshProUGUI m_TxtTitle;
    [SerializeField] TextMeshProUGUI m_TxtRemainTime;
    [SerializeField] TextMeshProUGUI m_TxtValue;
    [SerializeField] GameObject m_GoOwned;

    ShopOwnedItemData m_Data;
    Action<int> m_OnSelected;

    public void SetData(ShopOwnedItemData data, Action<int> onSelected)
    {
        m_Data = data;
        m_OnSelected = onSelected;
        m_ImgMain.sprite = data.Sprite;
        m_TxtTitle.Localized().SetKey(data.NameKey);
        ShopRemainingTime.Apply(m_TxtRemainTime.Localized(), data.EndsAt);
        m_TxtValue.Localized().SetKey("ui.common.gold_amount", new System.Collections.Generic.Dictionary<string, object> { ["gold"] = data.PriceGold.ToString("N0") });
        ApplyOwnState(data.Owned);
    }

    void Awake()
    {
        m_BtnAll.onClick.AddListener(OnClick);
        ApplyOwnState(false);
    }

    void OnDestroy()
    {
        m_BtnAll.onClick.RemoveListener(OnClick);
    }

    void ApplyOwnState(bool owned)
    {
        m_GoOwned.SetActive(owned);
        m_BtnAll.interactable = !owned;
    }

    void OnClick()
    {
        if (m_Data == null || m_Data.Owned) return;
        ALog.Log(
            $"商城商品点击: Id={m_Data.Id}; Name={m_Data.Name}; PriceGold={m_Data.PriceGold}.",
            ALogCategories.UI);
        m_OnSelected?.Invoke(m_Data.Index);
    }
}
