using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DeckCardRow : MonoBehaviour, IRowItem<DeckCardData>
{
    [SerializeField] DeckCardCell[] m_Items;
    [SerializeField] RectTransform m_Rect;
    public int RowCardCount => m_Items.Length;
    public void SetRowData(int rowIndex, List<DeckCardData> data, int selectedIndex, Action<int> onSelected)
    {
        // Scale the authored eight-column row with the current viewport width.
        float scale = m_Rect.rect.width / 520f;
        m_Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 94f * scale);
        for (int i = 0; i < m_Items.Length; i++)
        {
            m_Items[i].transform.localScale = Vector3.one * scale;
            ((RectTransform)m_Items[i].transform).anchoredPosition = new Vector2((6f + i * 64f) * scale, -4f * scale);
            int index = rowIndex * m_Items.Length + i;
            m_Items[i].gameObject.SetActive(index < data.Count);
            if (index < data.Count) m_Items[i].Bind(data[index], index, selectedIndex == index, onSelected);
        }
    }
}
