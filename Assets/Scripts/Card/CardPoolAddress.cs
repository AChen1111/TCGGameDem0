using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>分池 poolKey 到 Addressable 卡包目录. CardAll 用抽卡结果的 sourcePool, 不在此映射.</summary>
public static class CardPoolAddress
{
    public const string Card01 = "CardBag01_BlueEyes";
    public const string Card02 = "CardBag02_Hero";
    public const string Card03 = "CardBag03_SkyStriker";
    static readonly string[] EnglishExts = { ".jpg", ".png", ".webp" };

    public static bool IsKnownDrawPool(string poolKey)
    {
        return poolKey is "Card01" or "Card02" or "Card03" or "CardAll";
    }

    public static bool TryGetBagFolder(string poolKey, out string folder)
    {
        switch (poolKey)
        {
            case "Card01":
                folder = Card01;
                return true;
            case "Card02":
                folder = Card02;
                return true;
            case "Card03":
                folder = Card03;
                return true;
            default:
                folder = null;
                return false;
        }
    }

    public static async UniTask<Texture> LoadCardTextureAsync(string poolKey, string cardId)
    {
        if (!TryGetBagFolder(poolKey, out string bag) || string.IsNullOrEmpty(cardId))
        {
            return null;
        }

        if (LocalizationService.CurrentLanguage != GameLanguage.English)
        {
            return await LoadAddressAsync($"{bag}/{cardId}.jpg", poolKey, cardId, $"{bag}/{cardId}.jpg");
        }

        for (int i = 0; i < EnglishExts.Length; i++)
        {
            string address = $"{bag}/en/{cardId}{EnglishExts[i]}";
            Texture texture = await TryLoadAddressAsync(address);
            if (texture != null)
            {
                return texture;
            }
        }

        ALog.LogWarning(
            $"加载卡图失败. Language=English; Pool={poolKey}; CardId={cardId}; Address={bag}/en/{cardId}.*",
            ALogCategories.Localization);
        return null;
    }

    static async UniTask<Texture> LoadAddressAsync(string address, string poolKey, string cardId, string logAddress)
    {
        Texture texture = await TryLoadAddressAsync(address);
        if (texture != null)
        {
            return texture;
        }

        ALog.LogWarning(
            $"加载卡图失败. Language={LocalizationService.CurrentLanguage}; Pool={poolKey}; CardId={cardId}; Address={logAddress}",
            ALogCategories.Localization);
        return null;
    }

    static async UniTask<Texture> TryLoadAddressAsync(string address)
    {
        var locHandle = Addressables.LoadResourceLocationsAsync(address, typeof(Texture));
        try
        {
            var locations = await locHandle.Task;
            if (locations == null || locations.Count == 0)
            {
                return null;
            }
        }
        catch
        {
            return null;
        }
        finally
        {
            if (locHandle.IsValid())
            {
                Addressables.Release(locHandle);
            }
        }

        var handle = Addressables.LoadAssetAsync<Texture>(address);
        try
        {
            return await handle.Task;
        }
        catch
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            return null;
        }
    }
}
