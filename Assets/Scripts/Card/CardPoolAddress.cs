/// <summary>分池 poolKey 到 Addressable 卡包目录. CardAll 用抽卡结果的 sourcePool, 不在此映射.</summary>
public static class CardPoolAddress
{
    public const string Card01 = "CardBag01_BlueEyes";
    public const string Card02 = "CardBag02_Hero";
    public const string Card03 = "CardBag03_SkyStriker";

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
}
