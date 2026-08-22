using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 列控制器
/// </summary>
public class CardPackRowItem : MonoBehaviour
{
    [SerializeField] private ShopCardItem[] m_ShopCardItems;
    [SerializeField] private const int numPerRow = 3;
    
    public void SetRowData(int rowIndex, List<ShopCardItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        if (m_ShopCardItems == null) return;
        int count = m_ShopCardItems.Length;
        for (int i = 0; i < count; i++)
        {
            if (m_ShopCardItems[i] == null) continue;
            int realIndex = rowIndex * numPerRow + i;
            if (realIndex < allData.Count)
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
