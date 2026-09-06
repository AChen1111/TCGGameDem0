using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>头像品类.</summary>
public sealed class AvatarShopCategory : ShopCategory
{
    List<AvatarShopItemData> m_Items;

    public AvatarShopCategory(IShopDataSource dataSource) : base(dataSource)
    {
    }

    public override string RowPrefabKey => AddressKeys.Prefab.AvatarShopItemRowPrefab;
    public override string DisplayName => "头像";

    public override async UniTask BindAsync(GridListController list, Action<int> onSelected)
    {
        if (m_Items == null)
        {
            m_Items = await DataSource.LoadAvatarsAsync();
            ALog.Log($"商城品类数据加载完成: 品类={DisplayName}, Count={m_Items.Count}", ALogCategories.UI);
        }

        await list.InitList(RowPrefabKey, m_Items, onSelected);
    }

    public override bool TryGetPurchaseTarget(int index, out ShopPurchaseTarget target)
    {
        if (m_Items == null || index < 0 || index >= m_Items.Count)
        {
            target = default;
            return false;
        }

        AvatarShopItemData item = m_Items[index];
        target = new ShopPurchaseTarget(ShopCatalogTypes.Avatar, item.Id, item.Name, item.PriceGold, item.Owned);
        return true;
    }

    public override void InvalidateCache()
    {
        m_Items = null;
    }
}
