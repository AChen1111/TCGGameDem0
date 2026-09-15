using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CardDetailWindowProperty : IWindowProperties
{
    public IReadOnlyList<CardDetailEntry> Cards { get; }
    public int Index { get; }
    public Action<bool> OnVisibleChanged { get; }

    public CardDetailWindowProperty(
        IReadOnlyList<CardDetailEntry> cards,
        int index,
        Action<bool> onVisibleChanged = null)
    {
        Cards = cards;
        Index = index;
        OnVisibleChanged = onVisibleChanged;
    }
}

public class CardDetailWindow : AWindowController<CardDetailWindowProperty>
{
    [SerializeField] CardDetailView m_View;

    protected override void OnOpen()
    {
        if (m_View == null || Properties == null)
        {
            ALog.LogError("卡牌详情窗口打开失败. 原因=视图或参数缺失", ALogCategories.UI);
            return;
        }

        // Dim 点击走 UI_Close, 由 Window 出栈, 避免 View 直接 SetActive(false)
        m_View.SetCallbacks(UI_Close, null, OpenZoom);
        m_View.Show(Properties.Cards, Properties.Index);
        Properties.OnVisibleChanged?.Invoke(true);
        int count = Properties.Cards != null ? Properties.Cards.Count : 0;
        ALog.Log($"卡牌详情窗口打开. Count={count}; Index={Properties.Index}", ALogCategories.UI);
    }

    protected override void OnClose()
    {
        if (m_View != null)
        {
            m_View.SetCallbacks(null, null);
        }

        Properties?.OnVisibleChanged?.Invoke(false);
        ALog.Log("卡牌详情窗口关闭", ALogCategories.UI);
    }

    void OpenZoom(Texture texture)
    {
        if (texture == null)
        {
            ALog.LogWarning("卡图放大打开失败. 原因=贴图缺失", ALogCategories.UI);
            return;
        }

        RequestOpenWindow(AddressKeys.Prefab.CardZoomWindow, new CardZoomWindowProperty(texture));
    }
}
