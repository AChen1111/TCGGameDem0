using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
            m_CardPackList.Add(new ShopCardItemData(i, $"卡包 {i}", null, "1000", "31day", i));
            ALog.Log($"卡包 {i}被加入列表","UI");
        }
        m_CardPackListController.InitList(m_CardPackList).Forget();
    }
}
