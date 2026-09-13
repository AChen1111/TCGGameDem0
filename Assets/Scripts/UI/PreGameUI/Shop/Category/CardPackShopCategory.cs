using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>卡包品类. 点击后走抽卡, 不走购买接口.</summary>
public sealed class CardPackShopCategory : ShopCategory
{
    List<ShopCardItemData> m_Items;

    public override string DisplayName => "卡包";

    public override async UniTask BindAsync(
        GridListController list,
        Action<int> onSelected,
        CancellationToken cancellationToken = default)
    {
        m_Items = await ShopCatalogQuery.LoadCardPacksAsync(cancellationToken);
        await list.InitList(AddressKeys.Prefab.CardPackRowPrefab, m_Items, onSelected,
            cancellationToken: cancellationToken);
    }

    public override bool TryGetDrawTarget(int index, out ShopDrawTarget target)
    {
        if (m_Items == null || index < 0 || index >= m_Items.Count)
        {
            target = default;
            return false;
        }

        ShopCardItemData item = m_Items[index];
        target = new ShopDrawTarget(item.Id, item.Title, item.PoolKey);
        return true;
    }
}
