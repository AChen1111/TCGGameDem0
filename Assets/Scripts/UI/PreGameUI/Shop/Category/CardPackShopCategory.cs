using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>卡包品类.</summary>
public sealed class CardPackShopCategory : ShopCategory
{
    List<ShopCardItemData> m_Items;

    public CardPackShopCategory(IShopDataSource dataSource) : base(dataSource)
    {
    }

    public override string RowPrefabKey => AddressKeys.Prefab.CardPackRowPrefab;
    public override string DisplayName => "卡包";

    public override async UniTask BindAsync(GridListController list, Action<int> onSelected)
    {
        if (m_Items == null)
        {
            m_Items = await DataSource.LoadCardPacksAsync();
            ALog.Log($"商城品类数据加载完成: 品类={DisplayName}, Count={m_Items.Count}", ALogCategories.UI);
        }

        await list.InitList(RowPrefabKey, m_Items, onSelected);
    }

    public override void InvalidateCache()
    {
        m_Items = null;
    }
}
