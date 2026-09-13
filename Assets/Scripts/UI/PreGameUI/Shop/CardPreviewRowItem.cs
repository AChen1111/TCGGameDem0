using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPreviewRowItem : MonoBehaviour, IRowItem<CardInUIData>
{
    [SerializeField] CardInUI[] m_Cards;

    public int RowCardCount => m_Cards != null ? m_Cards.Length : 0;

    public void SetRowData(int rowIndex, List<CardInUIData> allData, int selectedIndex, Action<int> onSelected)
    {
        ShopItemRow.Bind(m_Cards, rowIndex, allData, onSelected, static (item, data, selected) => item.SetData(data, selected));
    }
}
