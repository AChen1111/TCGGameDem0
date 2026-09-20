using System;
using System.Collections.Generic;
using AChen.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftRequestRowItem : MonoBehaviour, IRowItem<InboxItemData>
{
    [SerializeField] Image m_ImgIcon;
    [SerializeField] TMP_Text m_TxtName;
    [SerializeField] Button m_BtnYes;
    [SerializeField] Button m_BtnNo;

    int m_Bind;
    InboxItemData m_Data;

    public int RowCardCount => 1;

    public void SetRowData(int rowIndex, List<InboxItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        m_Data = allData[rowIndex];
        m_TxtName.text = m_Data.Nickname;
        int version = ++m_Bind;
        SocialPortrait.ApplyAsync(m_ImgIcon, m_Data.AvatarId, version, () => m_Bind).Forget();
    }

    void Awake()
    {
        m_BtnYes.onClick.AddListener(() => Invoke(InboxRowAction.Accept));
        m_BtnNo.onClick.AddListener(() => Invoke(InboxRowAction.Reject));
    }

    void Invoke(InboxRowAction action)
    {
        if (m_Data == null)
        {
            return;
        }

        GetComponentInParent<GiftWindow>()?.HandleRowAction(m_Data, action);
    }
}
