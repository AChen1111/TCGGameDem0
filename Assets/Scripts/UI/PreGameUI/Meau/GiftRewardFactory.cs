using System.Collections.Generic;
using AChen.Networking;

/// <summary>按礼品数据生成金币/卡规格, 供单测与行展示共用.</summary>
public static class GiftRewardFactory
{
    public const string GoldKind = "gold";
    public const string CardKind = "card";

    public static List<GiftRewardSpec> BuildSpecs(long gold, IReadOnlyList<OwnedCardData> cards)
    {
        var specs = new List<GiftRewardSpec>();
        if (gold > 0)
        {
            specs.Add(new GiftRewardSpec(GoldKind, gold));
        }

        if (cards == null)
        {
            return specs;
        }

        for (int i = 0; i < cards.Count; i++)
        {
            OwnedCardData card = cards[i];
            if (card == null || card.Count <= 0 || string.IsNullOrEmpty(card.CardId))
            {
                continue;
            }

            specs.Add(new GiftRewardSpec(CardKind, card.Count, card.CardId, card.Rarity));
        }

        return specs;
    }
}
