using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPackRowItem : MonoBehaviour, IRowItem<ShopCardItemData>
{
    [SerializeField] private ShopCardItem[] m_ShopCardItems;

    public int RowCardCount => m_ShopCardItems != null ? m_ShopCardItems.Length : 0;

    public void SetRowData(int rowIndex, List<ShopCardItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        ShopItemRow.Bind(m_ShopCardItems, rowIndex, allData, onSelected, static (item, data, selected) => item.SetData(data, selected));
    }
}
