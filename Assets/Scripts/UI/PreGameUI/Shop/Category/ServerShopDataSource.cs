using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>使用已发布游戏配置和当前玩家资产生成商城数据.</summary>
public sealed class ServerShopDataSource : IShopDataSource
{
    public async UniTask<List<ShopCardItemData>> LoadCardPacksAsync()
    {
        GameConfigStore store = RequireStore();
        CardPackConfig[] configs = store.CardPacks.Values
            .Where(store.IsCardPackVisible)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArray();
        Sprite[] sprites = await LoadSpritesAsync(
            "CardPack",
            configs.Select(value => value.Id).ToArray(),
            configs.Select(value => value.CoverResourceKey).ToArray());
        var result = new List<ShopCardItemData>(configs.Length);
        for (int i = 0; i < configs.Length; i++)
        {
            CardPackConfig config = configs[i];
            result.Add(new ShopCardItemData(
                config.Id,
                config.Title,
                sprites[i],
                config.PriceGold,
                config.EndsAt,
                i));
        }

        return result;
    }

    public async UniTask<List<AvatarShopItemData>> LoadAvatarsAsync()
    {
        GameConfigStore store = RequireStore();
        AvatarConfig[] configs = store.Avatars.Values
            .Where(store.IsAvatarVisible)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArray();
        Sprite[] sprites = await LoadSpritesAsync(
            "Avatar",
            configs.Select(value => value.Id).ToArray(),
            configs.Select(value => value.ResourceKey).ToArray());
        var owned = new HashSet<int>(RequirePlayer().OwnedAvatarIds);
        var result = new List<AvatarShopItemData>(configs.Length);
        for (int i = 0; i < configs.Length; i++)
        {
            AvatarConfig config = configs[i];
            result.Add(new AvatarShopItemData(
                config.Id,
                config.Name,
                sprites[i],
                config.PriceGold,
                config.EndsAt,
                owned.Contains(config.Id),
                i));
        }

        return result;
    }

    public async UniTask<List<WallpaperShopItemData>> LoadWallpapersAsync()
    {
        GameConfigStore store = RequireStore();
        WallpaperConfig[] configs = store.Wallpapers.Values
            .Where(store.IsWallpaperVisible)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArray();
        Sprite[] sprites = await LoadSpritesAsync(
            "Wallpaper",
            configs.Select(value => value.Id).ToArray(),
            configs.Select(value => value.ResourceKey).ToArray());
        var owned = new HashSet<int>(RequirePlayer().OwnedBackgroundIds);
        var result = new List<WallpaperShopItemData>(configs.Length);
        for (int i = 0; i < configs.Length; i++)
        {
            WallpaperConfig config = configs[i];
            result.Add(new WallpaperShopItemData(
                config.Id,
                config.Name,
                sprites[i],
                config.PriceGold,
                config.EndsAt,
                owned.Contains(config.Id),
                i));
        }

        return result;
    }

    public static async UniTask<List<AvatarItemData>> LoadAvatarSelectionAsync()
    {
        AvatarConfig[] configs = RequireStore().Avatars.Values
            .Where(value => value.IsEnabled)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArray();
        Sprite[] sprites = await LoadSpritesAsync(
            "Avatar",
            configs.Select(value => value.Id).ToArray(),
            configs.Select(value => value.ResourceKey).ToArray());
        var owned = new HashSet<int>(RequirePlayer().OwnedAvatarIds);
        var result = new List<AvatarItemData>(configs.Length);
        for (int i = 0; i < configs.Length; i++)
        {
            result.Add(new AvatarItemData
            {
                Id = configs[i].Id,
                Name = configs[i].Name,
                Sprite = sprites[i],
                Owned = owned.Contains(configs[i].Id)
            });
        }

        return result;
    }

    static GameConfigStore RequireStore()
    {
        GameConfigStore store = GameConfigManager.Instance.Store;
        if (!store.HasSnapshot)
        {
            throw new GameConfigDataException("游戏配置尚未初始化");
        }

        return store;
    }

    static PlayerData RequirePlayer()
    {
        PlayerData player = PlayerSession.HasInstance ? PlayerSession.Instance.CurrentPlayer : null;
        return player ?? throw new InvalidOperationException("玩家数据尚未初始化");
    }

    static async UniTask<Sprite[]> LoadSpritesAsync(string table, int[] ids, string[] resourceKeys)
    {
        var tasks = new UniTask<Sprite>[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            tasks[i] = LoadSpriteAsync(table, ids[i], resourceKeys[i]);
        }

        return await UniTask.WhenAll(tasks);
    }

    static async UniTask<Sprite> LoadSpriteAsync(string table, int id, string resourceKey)
    {
        try
        {
            return await AddressableLoader.Instance.LoadSprite(resourceKey);
        }
        catch (Exception exception)
        {
            ALog.LogError(
                $"商城资源加载失败: Table={table}, Id={id}, ResourceKey={resourceKey}, Error={exception.Message}",
                ALogCategories.UI);
            throw;
        }
    }
}
