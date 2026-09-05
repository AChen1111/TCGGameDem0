using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>商城商品数据来源.目前只有假数据实现,接服务端时换一个实现即可.</summary>
public interface IShopDataSource
{
    UniTask<List<ShopCardItemData>> LoadCardPacksAsync();
    UniTask<List<AvatarShopItemData>> LoadAvatarsAsync();
    UniTask<List<WallpaperShopItemData>> LoadWallpapersAsync();
}
