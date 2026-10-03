using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>分池 poolKey 到 Addressable 卡包目录. CardAll 用抽卡结果的 sourcePool, 不在此映射.</summary>
public static class CardPoolAddress
{
    public const string Card01 = "CardBag01_BlueEyes";
    public const string Card02 = "CardBag02_Hero";
    public const string Card03 = "CardBag03_SkyStriker";

    public static bool IsKnownDrawPool(string poolKey)
    {
        return poolKey is "Card01" or "Card02" or "Card03" or "CardGeneric" or "CardAll";
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

    public static async UniTask<CardArtwork> LoadCardArtworkAsync(string poolKey, string cardId)
    {
        string tag = LocalizationService.CurrentLanguage == GameLanguage.English ? "Cards_EN" : "Cards_CN";
        var atlas = await AddressableLoader.Instance.LoadAtlas(tag);
        return new CardArtwork(atlas.GetSprite(poolKey + "_" + cardId));
    }
}
