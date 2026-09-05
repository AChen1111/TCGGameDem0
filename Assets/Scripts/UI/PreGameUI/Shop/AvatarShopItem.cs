using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>头像商品数据.PriceGold 和 Owned 暂无对应 UI 元素,先随数据带上,点击时输出到日志.</summary>
public class AvatarShopItemData
{
    public int Id { get; }
    public string Name { get; }
    public Sprite Sprite { get; }
    public long PriceGold { get; }
    public bool Owned { get; }
    public int Index { get; }

    public AvatarShopItemData(int id, string name, Sprite sprite, long priceGold, bool owned, int index)
    {
        Id = id;
        Name = name;
        Sprite = sprite;
        PriceGold = priceGold;
        Owned = owned;
        Index = index;
    }
}

/// <summary>商城里的单个头像格子.挂在行预制体的子 Button 上.</summary>
public class AvatarShopItem : MonoBehaviour
{
    [SerializeField] Button m_BtnAll;
    [SerializeField] Image m_ImgMain;

    AvatarShopItemData m_Data;
    Action<int> m_OnSelected;

    public void SetData(AvatarShopItemData data, bool isSelected, Action<int> onSelected)
    {
        m_Data = data;
        m_OnSelected = onSelected;
        m_ImgMain.sprite = data.Sprite;
        m_ImgMain.color = isSelected ? ShopItemColors.Selected : ShopItemColors.Normal;
    }

    void Awake()
    {
        m_BtnAll.onClick.AddListener(OnClick);
    }

    void OnDestroy()
    {
        m_BtnAll.onClick.RemoveListener(OnClick);
    }

    void OnClick()
    {
        if (m_Data == null) return;
        ALog.Log(
            $"商城头像点击: Id={m_Data.Id}; Name={m_Data.Name}; PriceGold={m_Data.PriceGold}; Owned={m_Data.Owned}.",
            ALogCategories.UI);
        m_OnSelected?.Invoke(m_Data.Index);
    }
}
