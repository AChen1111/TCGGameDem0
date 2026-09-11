using System;
using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>与后端购买接口约定的品类标识.</summary>
public static class ShopCatalogTypes
{
    public const string Avatar = "avatar";
    public const string Wallpaper = "wallpaper";
}

public readonly struct ShopPurchaseTarget
{
    public string CatalogType { get; }
    public int Id { get; }
    public string Name { get; }
    public long PriceGold { get; }
    public bool Owned { get; }

    public ShopPurchaseTarget(string catalogType, int id, string name, long priceGold, bool owned)
    {
        CatalogType = catalogType;
        Id = id;
        Name = name;
        PriceGold = priceGold;
        Owned = owned;
    }
}

/// <summary>
/// 商城的一个商品品类. GridListController.InitList 是泛型方法, 在热更里无法动态构造,
/// 所以把具体数据类型的绑定封装在 BindAsync 里, 窗口侧只面向非泛型基类.
/// </summary>
public abstract class ShopCategory
{
    /// <summary>品类名, 只用于日志.</summary>
    public abstract string DisplayName { get; }

    /// <summary>对应的后端品类标识; 不可购买的品类为 null.</summary>
    public virtual string CatalogType => null;

    /// <summary>用当前配置和玩家资产生成列表; 图片经 AddressableLoader 加载.</summary>
    public abstract UniTask BindAsync(
        GridListController list,
        Action<int> onSelected,
        CancellationToken cancellationToken = default);

    public virtual bool TryGetPurchaseTarget(int index, out ShopPurchaseTarget target)
    {
        target = default;
        return false;
    }
}
