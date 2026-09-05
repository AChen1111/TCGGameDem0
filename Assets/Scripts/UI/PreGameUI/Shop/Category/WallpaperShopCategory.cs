using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>壁纸品类.</summary>
public sealed class WallpaperShopCategory : ShopCategory
{
    List<WallpaperShopItemData> m_Items;

    public WallpaperShopCategory(IShopDataSource dataSource) : base(dataSource)
    {
    }

    public override string RowPrefabKey => AddressKeys.Prefab.WallpaperShopItemRowPrefab;
    public override string DisplayName => "壁纸";

    public override async UniTask BindAsync(GridListController list)
    {
        if (m_Items == null)
        {
            m_Items = await DataSource.LoadWallpapersAsync();
            ALog.Log($"商城品类数据加载完成: 品类={DisplayName}, Count={m_Items.Count}", ALogCategories.UI);
        }

        await list.InitList(RowPrefabKey, m_Items);
    }
}
