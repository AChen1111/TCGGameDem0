using System;
using System.Collections.Generic;
using AChen.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GiftRewardRowItem : MonoBehaviour, IRowItem<InboxItemData>
{
    [SerializeField] GiftRewardItem m_Reward;
    [SerializeField] TMP_Text m_TxtName;
    [SerializeField] Button m_BtnYes;

    InboxItemData m_Data;

    public int RowCardCount => 1;

    public void SetRowData(int rowIndex, List<InboxItemData> allData, int selectedIndex, Action<int> onSelected)
    {
        m_Data = allData[rowIndex];
        string titleKey = GiftTitleKeys.Resolve(m_Data.TitleKey, m_Data.Gold, m_Data.Cards);
        LocalizedText localized = m_TxtName.Localized();
        if (localized != null) localized.SetKey(titleKey);
        else m_TxtName.text = LocalizationService.GetText(titleKey);

        OwnedCardData card = FirstCard(m_Data.Cards);
        if (card != null)
        {
            m_Reward.SetCard(card.CardId, card.Count);
        }
        else
        {
            m_Reward.SetGold(m_Data.Gold);
        }
    }

    void Awake()
    {
        m_BtnYes.onClick.AddListener(() =>
        {
            if (m_Data != null)
            {
                GetComponentInParent<GiftWindow>()?.HandleRowAction(m_Data, InboxRowAction.Claim);
            }
        });
    }

    static OwnedCardData FirstCard(IReadOnlyList<OwnedCardData> cards)
    {
        if (cards == null)
        {
            return null;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            OwnedCardData card = cards[i];
            if (card != null && card.Count > 0 && !string.IsNullOrEmpty(card.CardId))
            {
                return card;
            }
        }

        return null;
    }
}
