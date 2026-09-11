using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AChen.Networking;
using AChen.Player;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>用已发布游戏配置和当前玩家资产组装商城/头像列表数据, 并加载对应图片.</summary>
public static class ShopCatalogQuery
{
    public static async UniTask<List<ShopCardItemData>> LoadCardPacksAsync(CancellationToken cancellationToken = default)
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
            configs.Select(value => value.CoverResourceKey).ToArray(),
            cancellationToken);
        var result = new List<ShopCardItemData>(configs.Length);
        for (int i = 0; i < configs.Length; i++)
        {
            CardPackConfig config = configs[i];
            result.Add(new ShopCardItemData(config.Id, config.Title, sprites[i], config.PriceGold, config.EndsAt, i));
        }

        return result;
    }

    /// <summary>
    /// 加载外观类商品(头像/壁纸). visibleOnly 为 true 时按上架时间过滤(商城), 否则只要求启用(头像选择).
    /// </summary>
    public static UniTask<List<ShopOwnedItemData>> LoadCosmeticsAsync(
        string catalogType,
        bool visibleOnly = true,
        CancellationToken cancellationToken = default)
    {
        GameConfigStore store = RequireStore();
        PlayerData player = RequirePlayer();
        switch (catalogType)
        {
            case ShopCatalogTypes.Avatar:
                return LoadCosmeticsAsync(catalogType, store, store.Avatars.Values, player.OwnedAvatarIds, visibleOnly, cancellationToken);
            case ShopCatalogTypes.Wallpaper:
                return LoadCosmeticsAsync(catalogType, store, store.Wallpapers.Values, player.OwnedBackgroundIds, visibleOnly, cancellationToken);
            default:
                throw new ArgumentOutOfRangeException(nameof(catalogType), catalogType, "未知的商城品类");
        }
    }

    static async UniTask<List<ShopOwnedItemData>> LoadCosmeticsAsync<T>(
        string table,
        GameConfigStore store,
        IEnumerable<T> source,
        IReadOnlyList<int> ownedIds,
        bool visibleOnly,
        CancellationToken cancellationToken)
        where T : CosmeticConfig
    {
        T[] configs = source
            .Where(value => visibleOnly ? store.IsCosmeticVisible(value) : value.IsEnabled)
            .OrderBy(value => value.SortOrder)
            .ThenBy(value => value.Id)
            .ToArray();
        Sprite[] sprites = await LoadSpritesAsync(
            table,
            configs.Select(value => value.Id).ToArray(),
            configs.Select(value => value.ResourceKey).ToArray(),
            cancellationToken);
        var owned = new HashSet<int>(ownedIds);
        var result = new List<ShopOwnedItemData>(configs.Length);
        for (int i = 0; i < configs.Length; i++)
        {
            T config = configs[i];
            result.Add(new ShopOwnedItemData(
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

    static async UniTask<Sprite[]> LoadSpritesAsync(string table, int[] ids, string[] resourceKeys, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tasks = new UniTask<Sprite>[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            tasks[i] = LoadSpriteAsync(table, ids[i], resourceKeys[i]);
        }

        Sprite[] sprites = await UniTask.WhenAll(tasks).AttachExternalCancellation(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return sprites;
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
