using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>商城假数据源.正式商品配置接入后新增一个实现替换即可.</summary>
public sealed class FakeShopDataSource : IShopDataSource
{
    const int AvatarCount = 14;
    const int WallpaperCount = 11;

    public UniTask<List<ShopCardItemData>> LoadCardPacksAsync()
    {
        return PreGameUiFakeData.CreateCardPacksAsync();
    }

    public async UniTask<List<AvatarShopItemData>> LoadAvatarsAsync()
    {
        var loads = new UniTask<Sprite>[AvatarCount];
        for (int i = 0; i < AvatarCount; i++)
        {
            loads[i] = AddressableLoader.Instance.LoadSprite(AddressKeys.GetAvatarAddress(i));
        }

        Sprite[] sprites = await UniTask.WhenAll(loads);
        var list = new List<AvatarShopItemData>(AvatarCount);
        for (int i = 0; i < AvatarCount; i++)
        {
            list.Add(new AvatarShopItemData(i, $"头像{i:D2}", sprites[i], 200L * (i + 1), i % 2 == 0, i));
        }

        return list;
    }

    public async UniTask<List<WallpaperShopItemData>> LoadWallpapersAsync()
    {
        // 壁纸商品图暂用同 id 的卡图 c_XX(480x160),壁纸本体资源是近正方形的 w_XX_Down,不适合做横幅
        var loads = new UniTask<Sprite>[WallpaperCount];
        for (int i = 0; i < WallpaperCount; i++)
        {
            loads[i] = AddressableLoader.Instance.LoadSprite($"c_{i:D2}");
        }

        Sprite[] sprites = await UniTask.WhenAll(loads);
        var list = new List<WallpaperShopItemData>(WallpaperCount);
        for (int i = 0; i < WallpaperCount; i++)
        {
            list.Add(new WallpaperShopItemData(i, $"壁纸{i:D2}", sprites[i], 500L * (i + 1), i % 3 == 0, i));
        }

        return list;
    }
}
