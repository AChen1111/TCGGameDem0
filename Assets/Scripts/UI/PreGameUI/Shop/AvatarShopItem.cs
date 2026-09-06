using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>头像商品数据.</summary>
public class AvatarShopItemData
{
    public int Id { get; }
    public string Name { get; }
    public Sprite Sprite { get; }
    public long PriceGold { get; }
    public DateTimeOffset? EndsAt { get; }
    public bool Owned { get; }
    public int Index { get; }

    public AvatarShopItemData(
        int id,
        string name,
        Sprite sprite,
        long priceGold,
        DateTimeOffset? endsAt,
        bool owned,
        int index)
    {
        Id = id;
        Name = name;
        Sprite = sprite;
        PriceGold = priceGold;
        EndsAt = endsAt;
        Owned = owned;
        Index = index;
    }
}

/// <summary>商城里的单个头像格子.挂在行预制体的子 Button 上.</summary>
public class AvatarShopItem : MonoBehaviour
{
    [SerializeField] Button m_BtnAll;
    [SerializeField] Image m_ImgMain;
    [SerializeField] TextMeshProUGUI m_TxtTitle;
    [SerializeField] TextMeshProUGUI m_TxtRemainTime;
    [SerializeField] TextMeshProUGUI m_TxtValue;
    [SerializeField] GameObject m_GoOwned;

    AvatarShopItemData m_Data;
    Action<int> m_OnSelected;

    public void SetData(AvatarShopItemData data, Action<int> onSelected)
    {
        m_Data = data;
        m_OnSelected = onSelected;
        m_ImgMain.sprite = data.Sprite;
        m_TxtTitle.text = data.Name;
        m_TxtRemainTime.text = FormatRemainingTime(data.EndsAt);
        m_TxtValue.text = data.PriceGold.ToString("N0");
        ApplyOwnState(data.Owned ? ShopItemOwnState.Owned : ShopItemOwnState.Unowned);
    }

    void Awake()
    {
        m_BtnAll.onClick.AddListener(OnClick);
        ApplyOwnState(ShopItemOwnState.Unowned);
    }

    void OnDestroy()
    {
        m_BtnAll.onClick.RemoveListener(OnClick);
    }

    void ApplyOwnState(ShopItemOwnState state)
    {
        bool owned = state == ShopItemOwnState.Owned;
        m_GoOwned.SetActive(owned);
        m_BtnAll.interactable = !owned;
    }

    void OnClick()
    {
        if (m_Data == null || m_Data.Owned) return;
        ALog.Log(
            $"商城头像点击: Id={m_Data.Id}; Name={m_Data.Name}; PriceGold={m_Data.PriceGold}; Owned={m_Data.Owned}.",
            ALogCategories.UI);
        m_OnSelected?.Invoke(m_Data.Index);
    }

    static string FormatRemainingTime(DateTimeOffset? endsAt)
    {
        if (!endsAt.HasValue) return string.Empty;
        TimeSpan remaining = endsAt.Value - AChen.Networking.GameConfigManager.Instance.Store.ServerNow;
        if (remaining <= TimeSpan.Zero) return "已结束";
        if (remaining.TotalDays >= 1) return $"{Math.Ceiling(remaining.TotalDays)}天";
        if (remaining.TotalHours >= 1) return $"{Math.Ceiling(remaining.TotalHours)}小时";
        return $"{Math.Max(1, Math.Ceiling(remaining.TotalMinutes))}分钟";
    }
}
