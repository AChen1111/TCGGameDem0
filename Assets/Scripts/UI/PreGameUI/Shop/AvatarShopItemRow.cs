using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>商城头像行.一行若干个格子,数据不足的格子隐藏.</summary>
public class AvatarShopItemRow : MonoBehaviour, IRowItem<AvatarShopItemData>
{
    [SerializeField] AvatarShopItem[] m_Items;

    public int RowCardCount => m_Items != null ? m_Items.Length : 0;

    public void SetRowData(int rowIndex, List<AvatarShopItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        if (m_Items == null) return;
        int count = m_Items.Length;
        for (int i = 0; i < count; i++)
        {
            if (m_Items[i] == null) continue;
            int realIndex = rowIndex * count + i;
            if (allData != null && realIndex < allData.Count)
            {
                m_Items[i].gameObject.SetActive(true);
                m_Items[i].SetData(allData[realIndex], selectedIndex == realIndex, onSelected);
            }
            else
            {
                m_Items[i].gameObject.SetActive(false);
            }
        }
    }
}
