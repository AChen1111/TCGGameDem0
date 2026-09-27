using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CardWorkshopRow : MonoBehaviour, IRowItem<WorkshopCardData>
{
    [SerializeField] CardWorkshopItem[] m_Items;
    public int RowCardCount => m_Items.Length;
    public void SetRowData(int rowIndex, List<WorkshopCardData> data, int selectedIndex, Action<int> onSelected)
    {
        for (int i = 0; i < m_Items.Length; i++)
        {
            int index = rowIndex * m_Items.Length + i;
            m_Items[i].gameObject.SetActive(index < data.Count);
            if (index < data.Count) m_Items[i].Bind(data[index], index, selectedIndex == index, onSelected);
        }
    }
}
