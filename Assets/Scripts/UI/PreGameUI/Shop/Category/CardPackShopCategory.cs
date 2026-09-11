using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>卡包品类. 购买链路尚未接入后端, TryGetPurchaseTarget 保持基类默认(不可购买).</summary>
public sealed class CardPackShopCategory : ShopCategory
{
    public override string DisplayName => "卡包";

    public override async UniTask BindAsync(
        GridListController list,
        Action<int> onSelected,
        CancellationToken cancellationToken = default)
    {
        var items = await ShopCatalogQuery.LoadCardPacksAsync(cancellationToken);
        await list.InitList(AddressKeys.Prefab.CardPackRowPrefab, items, onSelected,
            cancellationToken: cancellationToken);
    }
}
