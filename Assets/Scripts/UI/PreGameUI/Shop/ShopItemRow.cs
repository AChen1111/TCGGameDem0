using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>商城行按列数把数据填进格子,多出来的格子隐藏.</summary>
public static class ShopItemRow
{
    public static void Bind<TItem, TData>(
        TItem[] items,
        int rowIndex,
        List<TData> allData,
        Action<int> onSelected,
        Action<TItem, TData, Action<int>> setData)
        where TItem : Component
    {
        if (items == null) return;
        int count = items.Length;
        for (int i = 0; i < count; i++)
        {
            TItem item = items[i];
            if (item == null) continue;
            int realIndex = rowIndex * count + i;
            bool hasData = allData != null && realIndex < allData.Count;
            item.gameObject.SetActive(hasData);
            if (hasData)
            {
                setData(item, allData[realIndex], onSelected);
            }
        }
    }
}
