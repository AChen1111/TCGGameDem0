using System;
using System.Linq;
using AChen.Duel.Presentation;
using NUnit.Framework;

public sealed class BattleHandOrderTests
{
    static CardView Hand(int id, int player = 0) => new CardView(id, player,
        new DuelCardSpec("", "", CardKind.Monster, CardFrame.Normal), new ZoneRef(DuelZone.Hand, player),
        CardPosition.FaceUp, false, Array.Empty<ZoneRef>(), Array.Empty<CardPosition>());

    [Test]
    public void SnapshotReorderingKeepsExistingCardsAndAppendsNewCards()
    {
        var order = new BattleHandOrder();
        order.Update(new[] { Hand(1), Hand(2), Hand(3) });
        order.Update(new[] { Hand(3), Hand(4), Hand(1), Hand(2) });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, order.Cards(0));
        // 后续表现帧、选择阶段和最终快照只改变枚举顺序，不能让旧卡相互换位。
        order.Update(new[] { Hand(4), Hand(2), Hand(3), Hand(1) });
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, order.Cards(0));
    }
    [Test]
    public void UsedCardIsRemovedAndReturnedCardAppendsAtRight()
    {
        var order = new BattleHandOrder();
        order.Update(new[] { Hand(1), Hand(2), Hand(3) });
        var previous = order.Capture();
        order.Update(new[] { Hand(3), Hand(1) });
        CollectionAssert.AreEqual(new[] { 1, 3 }, order.Cards(0));
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, previous[0], "移动起始布局必须保持之前阶段，不回滚当前缓存");
        order.Update(new[] { Hand(2), Hand(3), Hand(1) });
        CollectionAssert.AreEqual(new[] { 1, 3, 2 }, order.Cards(0));
    }
    [Test]
    public void BothSeatsKeepIndependentOrderAndExplicitNewDuelClearsIt()
    {
        var order = new BattleHandOrder();
        order.Update(new[] { Hand(1), Hand(2, 1), Hand(3), Hand(4, 1) });
        order.Update(new[] { Hand(4, 1), Hand(3), Hand(5, 1), Hand(1), Hand(2, 1) });
        CollectionAssert.AreEqual(new[] { 1, 3 }, order.Cards(0));
        CollectionAssert.AreEqual(new[] { 2, 4, 5 }, order.Cards(1));
        Assert.That(order.IndexOf(1, 5), Is.EqualTo(order.Count(1) - 1));
        order.Clear(); order.Update(new[] { Hand(3), Hand(1) });
        CollectionAssert.AreEqual(new[] { 3, 1 }, order.Cards(0));
    }
}
