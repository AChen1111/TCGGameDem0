using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>开窗参数.外部 OpenWindow(id, new ShopWindowProperties(list)).</summary>
public sealed class ShopWindowProperties : IWindowProperties
{
    public List<ShopCardItemData> CardPacks { get; }
    public int SelectedIndex { get; }

    public ShopWindowProperties(List<ShopCardItemData> cardPacks, int selectedIndex = -1)
    {
        CardPacks = cardPacks;
        SelectedIndex = selectedIndex;
    }
}

/// <summary>商城窗口.OnOpen 读取 Properties 填列表.卡包数据后续由新类提供.</summary>
public class ShopWindow : AWindowController<ShopWindowProperties>
{
    [SerializeField] CardPackListController m_CardPackListController;

    protected override void OnOpen()
    {
        BindList();
    }

    protected override void OnResume()
    {
        BindList();
    }

    void BindList()
    {
        m_CardPackListController.InitList(Properties.CardPacks, null, Properties.SelectedIndex).Forget();
    }
}
