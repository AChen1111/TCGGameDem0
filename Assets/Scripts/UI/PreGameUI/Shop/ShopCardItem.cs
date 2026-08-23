using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopCardItemData
{
    public int Id { get; }
    public string Title { get; }
    public Sprite MainSprite { get; }
    public long PriceGold { get; }
    public DateTimeOffset? EndsAt { get; }
    public int Index { get; }

    public ShopCardItemData(
        int id,
        string title,
        Sprite mainSprite,
        long priceGold,
        DateTimeOffset? endsAt,
        int index)
    {
        Id = id;
        Title = title;
        MainSprite = mainSprite;
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
    private int m_Index;
    private Action<int> m_OnSelected;
    public void SetData(ShopCardItemData data,bool isSelected,Action<int> onSelected)
    {
        m_TxtTitile.text = data.Title;
        m_ImgMain.sprite = data.MainSprite;
        m_TxtValue.text = data.PriceGold.ToString("N0");
        m_TxtRemainTime.text = FormatRemainingTime(data.EndsAt);
        m_Index = data.Index;

        //todo:被选择效果
        if(isSelected)
        {
        }

        m_OnSelected = onSelected;
    }

    private void Awake() {
        m_BtnAll.onClick.AddListener(OnSelectedClick);
    }
    private void OnDestroy() {
        m_BtnAll.onClick.RemoveListener(OnSelectedClick);
    }
    private void OnSelectedClick() {
        m_OnSelected?.Invoke(m_Index);//通知上层点击了哪个
    }

    static string FormatRemainingTime(DateTimeOffset? endsAt)
    {
        if (!endsAt.HasValue)
        {
            return string.Empty;
        }

        TimeSpan remaining = endsAt.Value - AChen.Networking.GameConfigManager.Instance.Store.ServerNow;
        if (remaining <= TimeSpan.Zero)
        {
            return "已结束";
        }

        if (remaining.TotalDays >= 1)
        {
            return $"{Math.Ceiling(remaining.TotalDays)}天";
        }

        if (remaining.TotalHours >= 1)
        {
            return $"{Math.Ceiling(remaining.TotalHours)}小时";
        }

        return $"{Math.Max(1, Math.Ceiling(remaining.TotalMinutes))}分钟";
    }
}
