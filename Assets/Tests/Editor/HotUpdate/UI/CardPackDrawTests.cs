using System;
using System.Collections.Generic;
using System.Reflection;
using AChen.Networking;
using NUnit.Framework;
using UnityEngine;

public sealed class CardPackDrawTests
{
    [Test]
    public void Known_draw_pools_map_to_bag_folders()
    {
        Assert.IsTrue(CardPoolAddress.IsKnownDrawPool("Card01"));
        Assert.IsTrue(CardPoolAddress.IsKnownDrawPool("Card02"));
        Assert.IsTrue(CardPoolAddress.IsKnownDrawPool("Card03"));
        Assert.IsTrue(CardPoolAddress.IsKnownDrawPool("CardAll"));
        Assert.IsFalse(CardPoolAddress.IsKnownDrawPool(null));
        Assert.IsFalse(CardPoolAddress.IsKnownDrawPool(""));
        Assert.IsFalse(CardPoolAddress.IsKnownDrawPool("Card99"));

        Assert.IsTrue(CardPoolAddress.TryGetBagFolder("Card01", out string card01));
        Assert.AreEqual(CardPoolAddress.Card01, card01);
        Assert.IsTrue(CardPoolAddress.TryGetBagFolder("Card03", out string card03));
        Assert.AreEqual(CardPoolAddress.Card03, card03);
        Assert.IsFalse(CardPoolAddress.TryGetBagFolder("CardAll", out _));
    }

    [Test]
    public void Card_pack_category_returns_draw_target_from_published_pool()
    {
        var category = new CardPackShopCategory();
        var items = new List<ShopCardItemData>
        {
            new ShopCardItemData(7, "卡包06", null, "Card02", 700, null, 0)
        };
        typeof(CardPackShopCategory)
            .GetField("m_Items", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(category, items);

        Assert.IsTrue(category.TryGetDrawTarget(0, out ShopDrawTarget target));
        Assert.AreEqual(7, target.Id);
        Assert.AreEqual("卡包06", target.Title);
        Assert.AreEqual("Card02", target.PoolKey);
        Assert.AreEqual(700, target.PriceGold);
        Assert.IsFalse(category.TryGetPurchaseTarget(0, out _));
        Assert.IsFalse(category.TryGetDrawTarget(-1, out _));
        Assert.IsFalse(category.TryGetDrawTarget(1, out _));
    }

    [Test]
    public void Config_allows_empty_pool_key_and_rejects_oversized()
    {
        DateTimeOffset publishedAt = new DateTimeOffset(2026, 8, 23, 9, 0, 0, TimeSpan.Zero);
        var emptyPool = new GameConfigSnapshot(
            3,
            1,
            publishedAt,
            Array.Empty<AvatarConfig>(),
            Array.Empty<WallpaperConfig>(),
            new[]
            {
                new CardPackConfig(1, "卡包", "c_00", "", 100, null, null, 0, true)
            });
        GameConfigSnapshotValidator.Validate(emptyPool);

        var oversized = new GameConfigSnapshot(
            3,
            1,
            publishedAt,
            Array.Empty<AvatarConfig>(),
            Array.Empty<WallpaperConfig>(),
            new[]
            {
                new CardPackConfig(1, "卡包", "c_00", new string('x', 33), 100, null, null, 0, true)
            });
        Assert.Throws<GameConfigDataException>(() => GameConfigSnapshotValidator.Validate(oversized));
    }

    [Test]
    public void Window_property_keeps_preloaded_cards()
    {
        Texture texture = new Texture2D(2, 2);
        try
        {
            var cards = new[]
            {
                new CardPickViewData
                {
                    cardId = "14558127",
                    cardShaderType = CardPickController.ToShaderType(1),
                    cardTexture = texture
                }
            };

            var property = new CardPickWindowProperty(cards);
            Assert.AreEqual(1, property.Cards.Count);
            Assert.AreEqual("14558127", property.Cards[0].cardId);
            Assert.AreEqual(CardShaderType.Colorful, property.Cards[0].cardShaderType);
            Assert.AreSame(texture, property.Cards[0].cardTexture);
            Assert.AreEqual(0, new CardPickWindowProperty(null).Cards.Count);
            Assert.AreEqual(CardShaderType.None, CardPickController.ToShaderType(-1));
            Assert.AreEqual(CardShaderType.Outline, CardPickController.ToShaderType(3));
            Assert.AreEqual(CardShaderType.Gold, CardPickController.ToShaderType(4));
            Assert.AreEqual(CardShaderType.None, CardPickController.ToShaderType(5));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void Preview_window_property_keeps_draw_target()
    {
        var target = new ShopDrawTarget(7, "卡包06", "Card02", 700);
        bool confirmed = false;
        var property = new CardPreviewWindowProperty(target, () => confirmed = true, null);

        Assert.AreEqual(7, property.Target.Id);
        Assert.AreEqual("卡包06", property.Target.Title);
        Assert.AreEqual("Card02", property.Target.PoolKey);
        Assert.AreEqual(700, property.Target.PriceGold);
        Assert.IsNotNull(property.OnConfirm);
        Assert.IsNull(property.OnCancel);
        property.OnConfirm();
        Assert.IsTrue(confirmed);
    }
}
