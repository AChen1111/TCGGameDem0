using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>商城商品数据来源，由已发布的服务端配置实现。</summary>
public interface IShopDataSource
{
    UniTask<List<ShopCardItemData>> LoadCardPacksAsync();
    UniTask<List<AvatarShopItemData>> LoadAvatarsAsync();
    UniTask<List<WallpaperShopItemData>> LoadWallpapersAsync();
}
