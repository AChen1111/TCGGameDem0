using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class ProfileCosmeticItem : MonoBehaviour
{
    [SerializeField] Button m_BtnSelect;
    [SerializeField] AvatarPortraitView m_Portrait;
    [SerializeField] GameObject m_GoSelected;
    [SerializeField] GameObject m_GoEquipped;
    int m_index;
    Action<int> m_select;
    void Awake() => m_BtnSelect.onClick.AddListener(Select);
    void Select() => m_select(m_index);
    public void Bind(ProfileCosmeticData data, int index, bool selected, Action<int> onSelected)
    {
        m_index = index;
        m_select = onSelected;
        m_GoSelected.SetActive(selected);
        m_GoEquipped.SetActive(data.Equipped);
        m_Portrait.SetPortrait(data.AvatarId, data.FrameId);
    }
}
