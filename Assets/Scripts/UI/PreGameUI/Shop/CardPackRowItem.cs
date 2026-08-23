using System;
using System.Collections.Generic;
using UnityEngine;

public class CardPackRowItem : MonoBehaviour, IRowItem<ShopCardItemData>
{
    [SerializeField] private ShopCardItem[] m_ShopCardItems;

    public int RowCardCount => m_ShopCardItems != null ? m_ShopCardItems.Length : 0;

    public void SetRowData(int rowIndex, List<ShopCardItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        if (m_ShopCardItems == null) return;
        int count = m_ShopCardItems.Length;
        for (int i = 0; i < count; i++)
        {
            if (m_ShopCardItems[i] == null) continue;
            int realIndex = rowIndex * count + i;
            if (allData != null && realIndex < allData.Count)
            {
                m_ShopCardItems[i].gameObject.SetActive(true);
                m_ShopCardItems[i].SetData(allData[realIndex], selectedIndex == realIndex, onSelected);
            }
            else
            {
                m_ShopCardItems[i].gameObject.SetActive(false);
            }
        }
    }
}
