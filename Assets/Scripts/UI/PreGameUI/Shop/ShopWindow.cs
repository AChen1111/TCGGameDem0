using System.Collections.Generic;
using UnityEngine;

public class ShopWindow : AWindowController
{
    [SerializeField] private CardPackListController m_CardPackListController;
    private List<ShopCardItemData> m_CardPackList = new List<ShopCardItemData>();
    protected override void OnOpen()
    {
        m_CardPackList.Clear();
        for (int i = 0; i < 10; i++)
        {
            m_CardPackList.Add(new ShopCardItemData(i, $"卡包 {i}", null, "1000 钻", "3天", i));
        }
        m_CardPackListController.InitList(m_CardPackList);
    }
}
