using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ProfileCosmeticRow : MonoBehaviour, IRowItem<ProfileCosmeticData>
{
    [SerializeField] ProfileCosmeticItem[] m_Items;
    public int RowCardCount => m_Items.Length;
    public void SetRowData(int rowIndex, List<ProfileCosmeticData> allData, int selectedIndex, Action<int> onSelected)
    {
        for (int i = 0; i < m_Items.Length; i++)
        {
            m_Items[i].transform.SetSiblingIndex(i);
            int index = rowIndex * m_Items.Length + i;
            m_Items[i].gameObject.SetActive(index < allData.Count);
            if (index < allData.Count) m_Items[i].Bind(allData[index], index, index == selectedIndex, onSelected);
        }
    }
}

public readonly struct ProfileCosmeticData
{
    public readonly int Id, AvatarId, FrameId;
    public readonly bool Equipped;
    public ProfileCosmeticData(int id, int avatarId, int frameId, bool equipped)
    { Id = id; AvatarId = avatarId; FrameId = frameId; Equipped = equipped; }
}
