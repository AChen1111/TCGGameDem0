using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>商城可拥有商品行. 一行若干格子, 数据不足的格子隐藏; 头像与壁纸行预制体共用.</summary>
public class ShopOwnedItemRow : MonoBehaviour, IRowItem<ShopOwnedItemData>
{
    [SerializeField] ShopOwnedItem[] m_Items;

    public int RowCardCount => m_Items != null ? m_Items.Length : 0;

    public void SetRowData(int rowIndex, List<ShopOwnedItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        ShopItemRow.Bind(m_Items, rowIndex, allData, onSelected, static (item, data, selected) => item.SetData(data, selected));
    }
}
