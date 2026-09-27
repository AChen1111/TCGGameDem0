using System;
using System.Collections.Generic;
using AChen.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendRowItem : MonoBehaviour, IRowItem<FriendSearchHitData>
{
    [SerializeField] AvatarPortraitView m_Portrait;
    [SerializeField] TMP_Text m_TxtName;
    [SerializeField] Button m_BtnAction;

    FriendSearchHitData m_Data;

    public int RowCardCount => 1;

    public void SetRowData(int rowIndex, List<FriendSearchHitData> allData, int selectedIndex, Action<int> onSelected)
    {
        m_Data = allData[rowIndex];
        m_TxtName.text = m_Data.Nickname;
        ApplyAction();
        m_Portrait.SetPortrait(m_Data.AvatarId.Value, m_Data.AvatarFrameId);
    }

    void Awake()
    {
        m_BtnAction.onClick.AddListener(OnActionClick);
    }

    void OnActionClick()
    {
        if (m_Data == null || m_Data.IsPending)
        {
            return;
        }

        GetComponentInParent<FriendWindow>()?.HandleRowAction(m_Data);
    }

    void ApplyAction()
    {
        bool waiting = m_Data.IsPending && !m_Data.IsFriend;
        m_BtnAction.interactable = !waiting;
        string key = FriendRowKeys.ActionKey(m_Data.IsFriend, m_Data.IsPending);
        TMP_Text label = m_BtnAction.GetComponentInChildren<TMP_Text>(true);
        LocalizedText localized = label.Localized();
        if (localized != null) localized.SetKey(key);
        else label.text = LocalizationService.GetText(key);
    }
}
