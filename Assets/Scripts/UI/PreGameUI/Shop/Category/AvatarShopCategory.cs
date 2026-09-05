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

    public override async UniTask BindAsync(GridListController list)
    {
        if (m_Items == null)
        {
            m_Items = await DataSource.LoadAvatarsAsync();
            ALog.Log($"商城品类数据加载完成: 品类={DisplayName}, Count={m_Items.Count}", ALogCategories.UI);
        }

        await list.InitList(RowPrefabKey, m_Items);
    }
}
