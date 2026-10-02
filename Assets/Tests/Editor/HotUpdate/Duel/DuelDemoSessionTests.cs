using System.Linq;
using AChen.Duel.Presentation;
using NUnit.Framework;

public sealed class DuelDemoSessionTests
{
    static DuelDemoSession Create() => new DuelDemoSession(
        Enumerable.Repeat(new DuelCardSpec("89631139", "Card01", CardKind.Monster, CardFrame.Normal), 40).ToArray(),
        new[] { new DuelCardSpec("01948619", "Card02", CardKind.Monster, CardFrame.Link) });

    [Test]
    public void NormalSummonUsesClickTargetAndMovesOnlyAfterConfirmation()
    {
        var session = Create();
        var card = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        var action = card.Actions.Single(x => x.Kind == DuelActionKind.NormalSummon);
        var target = new ZoneRef(DuelZone.Monster, 0, 2);
        session.Submit(new BeginCardAction(card.InstanceId, action.Id));
        Assert.That(session.Current.HasPendingAction, Is.True);
        Assert.That(session.Current.CanInteract, Is.False);
        Assert.That(session.Current.PendingAction.NeedsPosition, Is.False);
        Assert.That(session.Current.PendingAction.Position, Is.EqualTo(CardPosition.FaceUpAttack));
        Assert.That(session.Current.PendingAction.Targets, Does.Contain(target));
        Assert.That(session.Current.Card(card.InstanceId).Zone.Kind, Is.EqualTo(DuelZone.Hand));
        session.Submit(new ConfirmActionTarget(target));
        Assert.That(session.Current.Card(card.InstanceId).Zone, Is.EqualTo(target));
        Assert.That(session.Current.Card(card.InstanceId).Position, Is.EqualTo(CardPosition.FaceUpAttack));
        Assert.That(session.Current.HasPendingAction, Is.False);
        Assert.That(session.Current.Animating, Is.True);
    }

    [Test]
    public void PendingClickActionLocksOtherCommandsAndCancellationPreservesTheSnapshot()
    {
        var session = Create();
        var before = session.Current;
        var card = before.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        session.Submit(new BeginCardAction(card.InstanceId, card.Actions.Single(x => x.Kind == DuelActionKind.NormalSummon).Id));
        session.Submit(new EndTurn());
        session.Submit(new MoveCard(card.InstanceId, new ZoneRef(DuelZone.Graveyard, 0)));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.SpellTrap, 0)));
        Assert.That(session.Current.ActivePlayer, Is.Zero);
        Assert.That(session.Current.HasPendingAction, Is.True);
        session.Submit(new PauseTimer(true));
        Assert.That(session.Current.TimerPaused, Is.True);
        session.Submit(new CancelCardAction());
        Assert.That(session.Current.CanInteract, Is.True);
        Assert.That(session.Current.Card(card.InstanceId).Zone, Is.EqualTo(card.Zone));
        Assert.That(session.Current.InZone(card.Zone).Count, Is.EqualTo(before.InZone(card.Zone).Count));
    }

    [Test]
    public void MonsterSetHasFixedFaceDownDefenseAndExtraMonstersHaveNoSetAction()
    {
        var session = Create();
        var card = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        var set = card.Actions.Single(x => x.Kind == DuelActionKind.SetMonster);
        session.Submit(new BeginCardAction(card.InstanceId, set.Id));
        Assert.That(session.Current.PendingAction.NeedsPosition, Is.False);
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.Monster, 0, 3)));
        Assert.That(session.Current.Card(card.InstanceId).Position, Is.EqualTo(CardPosition.FaceDownDefense));
        session.Submit(new AnimationCompleted());
        Assert.That(session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck, 0))[0].Actions,
            Has.None.Matches<DuelActionView>(x => x.Kind == DuelActionKind.SetMonster));
    }

    [Test]
    public void SpecialSummonChoosesPositionBeforeTargetAndLinkOnlyUsesAttack()
    {
        var normal = new DuelCardSpec("89631139", "Card01", CardKind.Monster, CardFrame.Normal);
        var fusion = new DuelCardSpec("fusion", "Card02", CardKind.Monster, CardFrame.Fusion);
        var link = new DuelCardSpec("link", "Card02", CardKind.Monster, CardFrame.Link);
        var session = new DuelDemoSession(new[] { normal }, new[] { fusion, link }, 1);
        var extra = session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck, 0));
        var action = extra[0].Actions.Single(x => x.Kind == DuelActionKind.SpecialSummon);
        session.Submit(new BeginCardAction(extra[0].InstanceId, action.Id));
        Assert.That(session.Current.PendingAction.NeedsPosition, Is.True);
        Assert.That(session.Current.PendingAction.Targets, Is.Empty);
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.ExtraMonster, -1)));
        Assert.That(session.Current.Card(extra[0].InstanceId).Zone.Kind, Is.EqualTo(DuelZone.ExtraDeck));
        session.Submit(new ChooseActionPosition(CardPosition.FaceUpDefense));
        Assert.That(session.Current.PendingAction.NeedsPosition, Is.False);
        Assert.That(session.Current.PendingAction.Targets, Does.Contain(new ZoneRef(DuelZone.Monster, 0, 4)));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.Monster, 0, 4)));
        Assert.That(session.Current.Card(extra[0].InstanceId).Position, Is.EqualTo(CardPosition.FaceUpDefense));
        session.Submit(new AnimationCompleted());
        var linkAction = session.Current.Card(extra[1].InstanceId).Actions.Single(x => x.Kind == DuelActionKind.SpecialSummon);
        Assert.That(linkAction.Positions, Is.EqualTo(new[] { CardPosition.FaceUpAttack }));
        session.Submit(new BeginCardAction(extra[1].InstanceId, linkAction.Id));
        Assert.That(session.Current.PendingAction.NeedsPosition, Is.False);
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.ExtraMonster, -1)));
        Assert.That(session.Current.Card(extra[1].InstanceId).Position, Is.EqualTo(CardPosition.FaceUpAttack));
    }

    [Test]
    public void SpellAndTrapProfilesPublishTheirOwnPlacementActions()
    {
        var spell = new DuelCardSpec("spell", "Card01", CardKind.Spell, CardFrame.Spell);
        var trap = new DuelCardSpec("trap", "Card01", CardKind.Trap, CardFrame.Trap);
        var field = new DuelCardSpec("field", "Card01", CardKind.Spell, CardFrame.Spell, CardSpellTrapType.Field);
        var session = new DuelDemoSession(new[] { spell, trap, field }, new DuelCardSpec[0], 3);
        var hand = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0));
        var activate = hand[0].Actions.Single(x => x.Kind == DuelActionKind.Activate);
        session.Submit(new BeginCardAction(hand[0].InstanceId, activate.Id));
        Assert.That(session.Current.PendingAction.Position, Is.EqualTo(CardPosition.FaceUp));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.SpellTrap, 0, 1)));
        session.Submit(new AnimationCompleted());
        var set = session.Current.Card(hand[1].InstanceId).Actions.Single(x => x.Kind == DuelActionKind.SetSpellTrap);
        Assert.That(session.Current.Card(hand[1].InstanceId).Actions, Has.None.Matches<DuelActionView>(x => x.Kind == DuelActionKind.Activate));
        session.Submit(new BeginCardAction(hand[1].InstanceId, set.Id));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.SpellTrap, 0, 2)));
        Assert.That(session.Current.Card(hand[1].InstanceId).Position, Is.EqualTo(CardPosition.FaceDown));
        Assert.That(hand[2].Actions.Single(x => x.Kind == DuelActionKind.Activate).TargetsFor(CardPosition.FaceUp),
            Is.EqualTo(new[] { new ZoneRef(DuelZone.Field, 0) }));
    }

    [Test]
    public void ActivateEffectPublishesAnEffectWithoutChangingHighlightsOrLocation()
    {
        var session = Create();
        var card = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        session.Submit(new MoveCard(card.InstanceId, new ZoneRef(DuelZone.Graveyard, 0)));
        session.Submit(new AnimationCompleted());
        Assert.That(session.Current.Card(card.InstanceId).Actions, Has.None.Matches<DuelActionView>(x => x.Kind == DuelActionKind.Activate));
        session.Submit(new SetCardEffectAvailable(card.InstanceId, true));
        var action = session.Current.Card(card.InstanceId).Actions.Single(x => x.Kind == DuelActionKind.Activate);
        DuelViewChange emitted = new DuelViewChange(DuelChangeKind.Reset, session.Current);
        session.Changed += change => emitted = change;
        session.Submit(new BeginCardAction(card.InstanceId, action.Id));
        Assert.That(emitted.Kind, Is.EqualTo(DuelChangeKind.Effect));
        Assert.That(emitted.InstanceId, Is.EqualTo(card.InstanceId));
        Assert.That(session.Current.Card(card.InstanceId).EffectAvailable, Is.True);
        Assert.That(session.Current.Card(card.InstanceId).Zone.Kind, Is.EqualTo(DuelZone.Graveyard));
        Assert.That(session.Current.HasPendingAction, Is.False);
        Assert.That(session.Current.Animating, Is.True);
    }

    [Test]
    public void PositionActionChangesOnlyRepresentationAfterItsChoice()
    {
        var session = Create();
        var card = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        var zone = new ZoneRef(DuelZone.Monster, 0);
        session.Submit(new MoveCard(card.InstanceId, zone));
        session.Submit(new AnimationCompleted());
        var action = session.Current.Card(card.InstanceId).Actions.Single(x => x.Kind == DuelActionKind.ChangePosition);
        session.Submit(new BeginCardAction(card.InstanceId, action.Id));
        Assert.That(session.Current.PendingAction.NeedsPosition, Is.True);
        Assert.That(session.Current.PendingAction.HasTarget, Is.False);
        session.Submit(new ChooseActionPosition(CardPosition.FaceUpDefense));
        Assert.That(session.Current.Card(card.InstanceId).Position, Is.EqualTo(CardPosition.FaceUpDefense));
        Assert.That(session.Current.Card(card.InstanceId).Zone, Is.EqualTo(zone));
        Assert.That(session.Current.HasPendingAction, Is.False);
        Assert.That(session.Current.Animating, Is.True);
    }

    [Test]
    public void PendulumActionUsesOnlyFreeSpellTrapEnds()
    {
        var pendulum = new DuelCardSpec("pendulum", "Card01", CardKind.Monster, CardFrame.Effect,
            CardSpellTrapType.None, CardFlags.Pendulum | CardFlags.Effect);
        var trap = new DuelCardSpec("trap", "Card01", CardKind.Trap, CardFrame.Trap);
        var session = new DuelDemoSession(new[] { pendulum, trap }, new DuelCardSpec[0], 2);
        var hand = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0));
        session.Submit(new MoveCard(hand[1].InstanceId, new ZoneRef(DuelZone.SpellTrap, 0, 4)));
        session.Submit(new AnimationCompleted());
        var action = session.Current.Card(hand[0].InstanceId).Actions.Single(x => x.Kind == DuelActionKind.Pendulum);
        Assert.That(action.TargetsFor(CardPosition.FaceUp), Is.EqualTo(new[] { new ZoneRef(DuelZone.SpellTrap, 0, 0) }));
        session.Submit(new BeginCardAction(hand[0].InstanceId, action.Id));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.SpellTrap, 0, 2)));
        Assert.That(session.Current.HasPendingAction, Is.True);
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.SpellTrap, 0, 0)));
        Assert.That(session.Current.Card(hand[0].InstanceId).Position, Is.EqualTo(CardPosition.FaceUp));
    }

    [Test]
    public void DebugPlacementCanChooseTheOtherSeatWithoutPickingTheFirstZone()
    {
        var session = Create();
        var card = session.Current.InZone(new ZoneRef(DuelZone.Hand, 1))[0];
        Assert.That(card.Actions, Is.Empty);
        session.Submit(new BeginDebugPlacement(card.InstanceId));
        Assert.That(session.Current.HasPendingAction, Is.True);
        Assert.That(session.Current.PendingAction.Kind, Is.EqualTo(DuelActionKind.DebugPlacement));
        Assert.That(session.Current.Card(card.InstanceId).Zone.Kind, Is.EqualTo(DuelZone.Hand));
        session.Submit(new ChooseActionPosition(CardPosition.FaceDownDefense));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.Monster, 0, 4)));
        Assert.That(session.Current.HasPendingAction, Is.True);
        var chosen = new ZoneRef(DuelZone.Monster, 1, 4);
        session.Submit(new ConfirmActionTarget(chosen));
        Assert.That(session.Current.Card(card.InstanceId).Zone, Is.EqualTo(chosen));
        Assert.That(session.Current.InZone(new ZoneRef(DuelZone.Monster, 1, 0)), Is.Empty);
    }

    [Test]
    public void VisibilityHidesOpponentHandWithoutChangingCardStateOrInspection()
    {
        var session = Create();
        IDuelVisibilityPolicy visibility = new DemoVisibilityPolicy();
        var mine = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        var opponent = session.Current.InZone(new ZoneRef(DuelZone.Hand, 1))[0];
        Assert.That(visibility.IsFaceVisible(0, mine), Is.True);
        Assert.That(visibility.IsFaceVisible(0, opponent), Is.False);
        Assert.That(visibility.CanInspect(0, opponent), Is.True);
        Assert.That(opponent.Position, Is.EqualTo(CardPosition.FaceUp));
        Assert.That(visibility.IsFaceVisible(1, opponent), Is.True);
        session.Submit(new MoveCard(mine.InstanceId, new ZoneRef(DuelZone.Monster, 0)));
        session.Submit(new AnimationCompleted());
        session.Submit(new ChangePosition(mine.InstanceId, CardPosition.FaceDownDefense));
        Assert.That(visibility.IsFaceVisible(0, session.Current.Card(mine.InstanceId)), Is.False);
    }

    [Test]
    public void PublishedActionsHonorExplicitDemoTargetsAndCurrentMainPhase()
    {
        var profile = new DuelActionProfile("demo-specific-summon", DuelActionKind.SpecialSummon,
            new[] { DuelZone.ExtraDeck }, new[] { CardPosition.FaceUpAttack },
            new[] { new DuelTargetSlots(DuelZone.Monster, 3) });
        var fusion = new DuelCardSpec("fusion", "Card02", CardKind.Monster, CardFrame.Fusion,
            CardSpellTrapType.None, CardFlags.None, new[] { profile });
        var session = new DuelDemoSession(new[] { new DuelCardSpec("normal", "Card01", CardKind.Monster, CardFrame.Normal) }, new[] { fusion }, 1);
        var mine = session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck, 0))[0];
        var other = session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck, 1))[0];
        Assert.That(other.Actions, Is.Empty);
        Assert.That(mine.Actions.Single().TargetsFor(CardPosition.FaceUpAttack), Is.EqualTo(new[] { new ZoneRef(DuelZone.Monster, 0, 3) }));
        session.Submit(new BeginCardAction(mine.InstanceId, "special-summon"));
        Assert.That(session.Current.HasPendingAction, Is.False);
        session.Submit(new ChangePhase(DuelPhase.End));
        session.Submit(new AnimationCompleted());
        Assert.That(session.Current.Card(mine.InstanceId).Actions, Is.Empty);
    }

    [Test]
    public void FullFieldRemovesPlacementActionsAndResetCancelsPendingSelection()
    {
        var session = Create();
        var cards = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0)).ToArray();
        for (int slot = 0; slot < 5; slot++)
        {
            var action = session.Current.Card(cards[slot].InstanceId).Actions.Single(x => x.Kind == DuelActionKind.NormalSummon);
            session.Submit(new BeginCardAction(cards[slot].InstanceId, action.Id));
            session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.Monster, 0, slot)));
            session.Submit(new AnimationCompleted());
        }
        var draw = session.Current.InZone(new ZoneRef(DuelZone.MainDeck, 0))[0];
        session.Submit(new MoveCard(draw.InstanceId, new ZoneRef(DuelZone.Hand, 0)));
        session.Submit(new AnimationCompleted());
        Assert.That(session.Current.Card(draw.InstanceId).Actions, Is.Empty);
        session.Submit(new BeginDebugPlacement(draw.InstanceId));
        Assert.That(session.Current.HasPendingAction, Is.False);
        session.Submit(new ResetDuel());
        var resetCard = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        session.Submit(new BeginCardAction(resetCard.InstanceId, resetCard.Actions.Single(x => x.Kind == DuelActionKind.SpecialSummon).Id));
        Assert.That(session.Current.PendingAction.NeedsPosition, Is.True);
        session.Submit(new ResetDuel());
        Assert.That(session.Current.HasPendingAction, Is.False);
        Assert.That(session.Current.CanInteract, Is.True);
        Assert.That(session.Current.InZone(new ZoneRef(DuelZone.Hand, 0)).Count, Is.EqualTo(5));
    }

    [Test]
    public void DemoPlayerProfilesRemainReadOnlyAcrossReset()
    {
        var normal = new DuelCardSpec("normal", "Card01", CardKind.Monster, CardFrame.Normal);
        var players = new[] { new DuelPlayerView("甲", 7), new DuelPlayerView("乙", 9) };
        var session = new DuelDemoSession(new[] { normal }, new DuelCardSpec[0], 1, 180, players);
        players[0] = new DuelPlayerView("外部修改", 99);
        Assert.That(session.Current.Players[0].Name, Is.EqualTo("甲"));
        Assert.That(session.Current.Players[1].AvatarId, Is.EqualTo(9));
        Assert.That(session.Current.Players[0].LP, Is.EqualTo(8000));
        session.Submit(new ResetDuel());
        Assert.That(session.Current.Players[0].Name, Is.EqualTo("甲"));
    }

    [Test]
    public void DebugTransfersRespectDeckPartitionsAndClickActionsShareExtraOccupancy()
    {
        var link = new DuelCardSpec("link", "Card02", CardKind.Monster, CardFrame.Link);
        var normal = new DuelCardSpec("normal", "Card01", CardKind.Monster, CardFrame.Normal);
        var session = new DuelDemoSession(new[] { normal }, new[] { link, link }, 1);
        var hand = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        var extra = session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck, 0)).ToArray();
        session.Submit(new MoveCard(hand.InstanceId, new ZoneRef(DuelZone.ExtraDeck, 0)));
        Assert.That(session.Current.Card(hand.InstanceId).Zone.Kind, Is.EqualTo(DuelZone.Hand));
        session.Submit(new MoveCard(extra[0].InstanceId, new ZoneRef(DuelZone.Hand, 0)));
        Assert.That(session.Current.Card(extra[0].InstanceId).Zone.Kind, Is.EqualTo(DuelZone.ExtraDeck));
        session.Submit(new BeginCardAction(extra[0].InstanceId, extra[0].Actions.Single().Id));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.ExtraMonster, -1, 0)));
        session.Submit(new AnimationCompleted());
        Assert.That(session.Current.Card(extra[1].InstanceId).Actions, Is.Empty);
        var enemy = session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck, 1))[0];
        session.Submit(new BeginDebugPlacement(enemy.InstanceId));
        Assert.That(session.Current.PendingAction.Targets, Is.EqualTo(new[] { new ZoneRef(DuelZone.ExtraMonster, -1, 1) }));
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.ExtraMonster, -1, 0)));
        Assert.That(session.Current.HasPendingAction, Is.True);
        session.Submit(new ConfirmActionTarget(new ZoneRef(DuelZone.ExtraMonster, -1, 1)));
        Assert.That(session.Current.Cards.Count(x => x.Zone.Kind == DuelZone.ExtraMonster), Is.EqualTo(2));
    }

    [Test]
    public void DirectPendulumTransferToASpellTrapEndUsesSpellRepresentation()
    {
        var pendulum = new DuelCardSpec("pendulum", "Card01", CardKind.Monster, CardFrame.Effect,
            CardSpellTrapType.None, CardFlags.Pendulum);
        var session = new DuelDemoSession(new[] { pendulum }, new DuelCardSpec[0], 1);
        var card = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0];
        session.Submit(new MoveCard(card.InstanceId, new ZoneRef(DuelZone.SpellTrap, 0, 4)));
        Assert.That(session.Current.Card(card.InstanceId).Position, Is.EqualTo(CardPosition.FaceUp));
        Assert.That(session.Current.Card(card.InstanceId).AvailablePositions,
            Is.EqualTo(new[] { CardPosition.FaceUp, CardPosition.FaceDown }));
    }

    [Test]
    public void PlacementChangesZonesOnlyAfterConfirmation()
    {
        var session = Create();
        int id = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0))[0].InstanceId;
        var target = new ZoneRef(DuelZone.Monster, 0, 2);
        session.Submit(new PreparePlacement(id, target));
        Assert.That(session.Current.InZone(new ZoneRef(DuelZone.Hand, 0)).Count, Is.EqualTo(5));
        Assert.That(session.Current.InZone(target), Is.Empty);
        session.Submit(new ConfirmPlacement(CardPosition.FaceUpAttack));
        Assert.That(session.Current.Card(id).Zone, Is.EqualTo(target));
        Assert.That(session.Current.InZone(new ZoneRef(DuelZone.Hand, 0)).Count, Is.EqualTo(4));
    }

    [Test]
    public void CancelPlacementPreservesTheCardAndZoneCounts()
    {
        var session = Create();
        var before = session.Current;
        int id = before.InZone(new ZoneRef(DuelZone.Hand, 0))[0].InstanceId;
        session.Submit(new PreparePlacement(id, new ZoneRef(DuelZone.Monster, 0)));
        session.Submit(new CancelPlacement());
        Assert.That(session.Current.HasPlacement, Is.False);
        Assert.That(session.Current.Card(id).Zone, Is.EqualTo(before.Card(id).Zone));
        Assert.That(session.Current.Cards.Count, Is.EqualTo(before.Cards.Count));
    }

    [Test]
    public void PlacementCannotReplacePendingChoiceOrInterruptAnimation()
    {
        var session = Create();
        var hand = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0));
        session.Submit(new PreparePlacement(hand[0].InstanceId, new ZoneRef(DuelZone.Monster, 0)));
        session.Submit(new PreparePlacement(hand[1].InstanceId, new ZoneRef(DuelZone.Monster, 0, 1)));
        Assert.That(session.Current.Placement.InstanceId, Is.EqualTo(hand[0].InstanceId));
        session.Submit(new ConfirmPlacement(CardPosition.FaceUpAttack));
        session.Submit(new PreparePlacement(hand[1].InstanceId, new ZoneRef(DuelZone.Monster, 0, 1)));
        Assert.That(session.Current.HasPlacement, Is.False);
        session.Submit(new AnimationCompleted());
        session.Submit(new PreparePlacement(hand[1].InstanceId, new ZoneRef(DuelZone.Monster, 0, 1)));
        Assert.That(session.Current.Placement.InstanceId, Is.EqualTo(hand[1].InstanceId));
    }

    [Test]
    public void OccupiedOrWrongKindTargetDoesNotRemoveACardFromHand()
    {
        var session = Create();
        var hand = session.Current.InZone(new ZoneRef(DuelZone.Hand, 0));
        var zone = new ZoneRef(DuelZone.Monster, 0, 2);
        session.Submit(new PreparePlacement(hand[0].InstanceId, zone));
        session.Submit(new ConfirmPlacement(CardPosition.FaceUpAttack));
        session.Submit(new AnimationCompleted());
        session.Submit(new PreparePlacement(hand[1].InstanceId, zone));
        Assert.That(session.Current.HasPlacement, Is.False);
        session.Submit(new PreparePlacement(hand[1].InstanceId, new ZoneRef(DuelZone.SpellTrap, 0)));
        Assert.That(session.Current.HasPlacement, Is.False);
        Assert.That(session.Current.InZone(zone).Single().InstanceId, Is.EqualTo(hand[0].InstanceId));
        Assert.That(session.Current.Card(hand[1].InstanceId).Zone.Kind, Is.EqualTo(DuelZone.Hand));
    }

    [Test]
    public void ResetDuringAnimationRestoresTheOpeningSnapshot()
    {
        var session = Create();
        var opening = session.Current;
        int id = opening.InZone(new ZoneRef(DuelZone.Hand, 0))[0].InstanceId;
        session.Submit(new PreparePlacement(id, new ZoneRef(DuelZone.Monster, 0)));
        session.Submit(new ConfirmPlacement(CardPosition.FaceUpAttack));
        session.Submit(new ResetDuel());
        Assert.That(session.Current.CanInteract, Is.True);
        Assert.That(session.Current.InZone(new ZoneRef(DuelZone.Hand, 0)).Count, Is.EqualTo(5));
        Assert.That(session.Current.Card(id).Zone, Is.EqualTo(opening.Card(id).Zone));
        Assert.That(opening.InZone(new ZoneRef(DuelZone.Hand, 0)).Count, Is.EqualTo(5));
    }

    [Test]
    public void DuplicateCardsRemainIndependentAcrossPileTransfers()
    {
        var session=Create();var opening=session.Current;var hand=opening.InZone(new ZoneRef(DuelZone.Hand,0));
        foreach(var zone in new[]{DuelZone.Graveyard,DuelZone.Banished,DuelZone.Hand,DuelZone.MainDeck})
        {session.Submit(new MoveCard(hand[0].InstanceId,new ZoneRef(zone,0)));session.Submit(new AnimationCompleted());}
        Assert.That(session.Current.Cards.Count,Is.EqualTo(opening.Cards.Count));
        Assert.That(session.Current.Card(hand[0].InstanceId).Zone.Kind,Is.EqualTo(DuelZone.MainDeck));
        Assert.That(session.Current.Card(hand[1].InstanceId).Zone.Kind,Is.EqualTo(DuelZone.Hand));
        Assert.That(opening.Card(hand[0].InstanceId).Zone.Kind,Is.EqualTo(DuelZone.Hand));
    }

    [Test]
    public void SharedExtraSlotsAndLinkPositionRemainConstrained()
    {
        var link=new DuelCardSpec("01948619","Card02",CardKind.Monster,CardFrame.Link);
        var session=new DuelDemoSession(new[]{new DuelCardSpec("89631139","Card01",CardKind.Monster,CardFrame.Normal)},new[]{link,link},1);
        var mine=session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck,0));var other=session.Current.InZone(new ZoneRef(DuelZone.ExtraDeck,1));
        session.Submit(new PreparePlacement(mine[0].InstanceId,new ZoneRef(DuelZone.ExtraMonster,-1)));
        session.Submit(new ConfirmPlacement(CardPosition.FaceUpDefense));
        Assert.That(session.Current.HasPlacement,Is.True);
        session.Submit(new ConfirmPlacement(CardPosition.FaceUpAttack));session.Submit(new AnimationCompleted());
        session.Submit(new PreparePlacement(mine[1].InstanceId,new ZoneRef(DuelZone.ExtraMonster,-1,1)));
        Assert.That(session.Current.HasPlacement,Is.False);
        session.Submit(new PreparePlacement(other[0].InstanceId,new ZoneRef(DuelZone.ExtraMonster,-1)));
        Assert.That(session.Current.HasPlacement,Is.False);
        session.Submit(new PreparePlacement(other[0].InstanceId,new ZoneRef(DuelZone.ExtraMonster,-1,1)));
        session.Submit(new ConfirmPlacement(CardPosition.FaceUpAttack));session.Submit(new AnimationCompleted());
        Assert.That(session.Current.Cards.Count(c=>c.Zone.Kind==DuelZone.ExtraMonster),Is.EqualTo(2));
    }

    [Test]
    public void TimerStopsAtZeroWithoutEndingTheTurnAndCanBePaused()
    {
        var session=Create();session.Submit(new PauseTimer(true));session.Tick(30);
        Assert.That(session.Current.Seconds[0],Is.EqualTo(180));
        session.Submit(new PauseTimer(false));session.Tick(200);
        Assert.That(session.Current.Seconds[0],Is.Zero);
        Assert.That(session.Current.ActivePlayer,Is.Zero);Assert.That(session.Current.Phase,Is.EqualTo(DuelPhase.Main1));
        Assert.That(session.Current.Seconds[1],Is.EqualTo(180));
        session.Submit(new ResetTimer());Assert.That(session.Current.Seconds[0],Is.EqualTo(180));
    }

    [Test]
    public void FirstTurnAndForwardPhaseTransitionsHaveExplicitChoices()
    {
        var session=Create();session.Submit(new ChangePhase(DuelPhase.Battle));
        Assert.That(session.Current.Phase,Is.EqualTo(DuelPhase.Main1));
        session.Submit(new EndTurn());session.Submit(new AnimationCompleted());
        Assert.That(session.Current.ActivePlayer,Is.EqualTo(1));Assert.That(session.Current.Phase,Is.EqualTo(DuelPhase.Draw));
        foreach(var phase in new[]{DuelPhase.Standby,DuelPhase.Main1,DuelPhase.Battle,DuelPhase.Main2,DuelPhase.End})
        {session.Submit(new ChangePhase(phase));session.Submit(new AnimationCompleted());}
        Assert.That(session.Current.Phase,Is.EqualTo(DuelPhase.End));Assert.That(session.Current.Turn,Is.EqualTo(2));
    }
}
