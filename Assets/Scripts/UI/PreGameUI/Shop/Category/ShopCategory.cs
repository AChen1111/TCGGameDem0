using Cysharp.Threading.Tasks;

/// <summary>
/// 商城的一个商品品类.GridListController.InitList 是泛型方法,窗口层无法多态调用,
/// 所以把具体数据类型收在子类的 BindAsync 里,窗口层只面对这个非泛型基类.
/// </summary>
public abstract class ShopCategory
{
    protected readonly IShopDataSource DataSource;

    protected ShopCategory(IShopDataSource dataSource)
    {
        DataSource = dataSource;
    }

    /// <summary>行预制体的 Addressable key.</summary>
    public abstract string RowPrefabKey { get; }

    /// <summary>品类名,仅用于日志.</summary>
    public abstract string DisplayName { get; }

    /// <summary>拉数据并绑到列表上.数据首次加载后由子类缓存,再次切回不重复拉取.</summary>
    public abstract UniTask BindAsync(GridListController list);
}
