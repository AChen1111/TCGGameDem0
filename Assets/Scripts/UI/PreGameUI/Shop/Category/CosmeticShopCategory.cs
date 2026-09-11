using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>头像、壁纸等可拥有外观商品品类.</summary>
public sealed class CosmeticShopCategory : ShopCategory
{
    readonly string m_displayName;
    readonly string m_catalogType;
    readonly string m_rowPrefabKey;
    List<ShopOwnedItemData> m_Items;

    public CosmeticShopCategory(string displayName, string catalogType, string rowPrefabKey)
    {
        m_displayName = displayName;
        m_catalogType = catalogType;
        m_rowPrefabKey = rowPrefabKey;
    }

    public override string DisplayName => m_displayName;

    public override string CatalogType => m_catalogType;

    public override async UniTask BindAsync(
        GridListController list,
        Action<int> onSelected,
        CancellationToken cancellationToken = default)
    {
        m_Items = await ShopCatalogQuery.LoadCosmeticsAsync(m_catalogType, cancellationToken: cancellationToken);
        await list.InitList(m_rowPrefabKey, m_Items, onSelected, cancellationToken: cancellationToken);
    }

    public override bool TryGetPurchaseTarget(int index, out ShopPurchaseTarget target)
    {
        if (m_Items == null || index < 0 || index >= m_Items.Count)
        {
            target = default;
            return false;
        }

        ShopOwnedItemData item = m_Items[index];
        target = new ShopPurchaseTarget(m_catalogType, item.Id, item.Name, item.PriceGold, item.Owned);
        return true;
    }
}
