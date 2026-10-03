using System;
using System.Collections.Generic;
using System.Linq;
using AChen.Duel.Presentation;
using NUnit.Framework;
using Core = AChen.Duel.Core;

public sealed class LocalDuelSessionTests
{
    static LocalDuelSession Create(string[] first, string[] second, int hand = 1, string[] extra = null)
    {
        var catalog = Core.DuelCardCatalog.CreateDefault();
        var extraCards=extra??Array.Empty<string>();
        var specs = first.Concat(second).Concat(extraCards).Distinct().Select(id =>
        {
            var card = catalog.Get(id);
            return new DuelCardSpec(id, "Card01", card.Kind == Core.RuleCardKind.Monster ? CardKind.Monster
                : card.Kind == Core.RuleCardKind.Spell ? CardKind.Spell : CardKind.Trap,
                card.IsNormal ? CardFrame.Normal : CardFrame.Effect, CardSpellTrapType.None, CardFlags.None, Array.Empty<DuelActionProfile>());
        });
        var session = new LocalDuelSession(() => new Core.DuelStartRecord { MainDecks = new[] { first, second },
            ExtraDecks=new[]{extraCards,Array.Empty<string>()},OpeningHand = hand, Shuffle = false, FirstPlayer = 0 }, specs,
            new[] { new DuelPlayerView("玩家一", 1010001), new DuelPlayerView("玩家二", 1010002) });
        session.Start(); Drain(session); return session;
    }
    static void Drain(LocalDuelSession session)
    {
        int budget = 512;
        while (session.Current.Animating && budget-- > 0) session.Submit(new AnimationCompleted());
        Assert.That(budget, Is.GreaterThan(0), "演出队列未结束");
    }
    static void Activate(LocalDuelSession session, string definition)
    {
        var card = session.Current.Cards.Single(c => c.Known && c.Definition.CardId == definition);
        session.Submit(new BeginCardAction(card.InstanceId, card.Actions.First(a => a.Kind == DuelActionKind.Activate).Id));
        if (session.Current.HasPendingAction) session.Submit(new ConfirmActionTarget(session.Current.PendingAction.Targets[0]));
        Drain(session);
    }
    [Test]
    public void OpeningUsesRulesAndConcealsOpponentsHandAndDeck()
    {
        var session = Create(new[] { "08240199", "89631139" }, new[] { "89631139", "08240199" });
        Assert.That(session.Current.Phase, Is.EqualTo(DuelPhase.Main1));
        Assert.That(session.Current.CanInteract, Is.True);
        Assert.That(session.Current.Cards.Where(c => c.Owner == 1 && c.Zone.Kind == DuelZone.Hand).All(c => !c.Known && c.Definition.CardId == ""), Is.True);
        Assert.That(session.Current.Cards.Where(c => c.Zone.Kind == DuelZone.MainDeck).All(c => !c.Known), Is.True);
    }
    [Test]
    public void CancelBeforeSummonDoesNotConsumeCardOrNormalSummon()
    {
        var session = Create(new[] { "08240199", "89631139" }, new[] { "89631139", "08240199" });
        var card = session.Current.Cards.Single(c => c.Known && c.Definition.CardId == "08240199");
        session.Submit(new BeginCardAction(card.InstanceId, card.Actions.Single(a => a.Kind == DuelActionKind.NormalSummon).Id));
        Assert.That(session.Current.HasPendingAction, Is.True);
        session.Submit(new CancelCardAction());
        Assert.That(session.Current.Card(card.InstanceId).Zone.Kind, Is.EqualTo(DuelZone.Hand));
        Assert.That(session.Current.Card(card.InstanceId).Actions.Any(a => a.Kind == DuelActionKind.NormalSummon), Is.True);
    }
    [Test]
    public void UpstartResolvesThroughPresentationAndUpdatesBothHandAndLife()
    {
        var session = Create(new[] { "70368879", "08240199", "89631139" }, new[] { "89631139", "08240199" });
        Activate(session, "70368879");
        Assert.That(session.Current.Players.Single(p => p.Name == "玩家二").LP, Is.EqualTo(9000));
        Assert.That(session.Current.InZone(new ZoneRef(DuelZone.Hand, 0)).Count, Is.EqualTo(1));
        Assert.That(session.Current.Cards.Any(c => c.Known && c.Definition.CardId == "70368879" && c.Zone.Kind == DuelZone.Graveyard), Is.True);
    }
    [Test]
    public void AshResponseSwitchesSeatAndResolvesChainInReverseWithoutPersistentGray()
    {
        var session = Create(new[] { "70368879", "08240199", "89631139" }, new[] { "14558127", "08240199", "89631139" });
        var order = new List<int>();
        session.Changed += change => { if (change.Kind == DuelChangeKind.ChainResolved) order.Add(change.LinkNumber); };
        Activate(session, "70368879");
        Assert.That(session.Current.ViewingSeat, Is.EqualTo(1));
        Assert.That(session.Current.Choice.Active, Is.True);
        var choice = session.Current.Choice;
        session.Submit(new ConfirmDuelSelection(choice.Id, new[] { choice.Options.Single(o => o.DefinitionId == "14558127").Key }));
        Drain(session);
        Assert.That(order, Is.EqualTo(new[] { 2, 1 }));
        Assert.That(session.Current.Players.All(p => p.LP == 8000), Is.True);
        Assert.That(session.Current.Cards.All(c => !c.Negated), Is.True);
    }
    [Test]
    public void StaleChoiceIsRejectedAndClockPausesDuringForcedPresentation()
    {
        var session = Create(new[] { "70368879", "08240199", "89631139" }, new[] { "14558127", "08240199" });
        Activate(session, "70368879"); var choice = session.Current.Choice; bool rejected = false;
        session.Changed += change => { if (change.Kind == DuelChangeKind.Rejected) rejected = true; };
        session.Submit(new ConfirmDuelSelection(choice.Id - 1, new[] { choice.Options[0].Key }));
        Assert.That(rejected, Is.True); Assert.That(session.Current.Choice.Id, Is.EqualTo(choice.Id));
        session.Submit(new CancelCardAction());
        Assert.That(session.Current.Animating, Is.True);
        var seconds = session.Current.Seconds.ToArray(); session.Tick(30);
        Assert.That(session.Current.Seconds, Is.EqualTo(seconds)); Drain(session);
    }
    [Test]
    public void VeilerUsesTargetChoiceAndGrayExpiresAtEndOfTurn()
    {
        var session = Create(new[] { "08240199", "89631139", "89631139" }, new[] { "97268402", "89631139", "89631139" });
        var source = session.Current.Cards.Single(c => c.Known && c.Definition.CardId == "08240199");
        session.Submit(new BeginCardAction(source.InstanceId, source.Actions.Single(a => a.Kind == DuelActionKind.NormalSummon).Id));
        session.Submit(new ConfirmActionTarget(session.Current.PendingAction.Targets[0])); Drain(session);
        var response = session.Current.Choice;
        session.Submit(new ConfirmDuelSelection(response.Id, new[] { response.Options.Single(o => o.DefinitionId == "97268402").Key }));
        var target = session.Current.Choice;
        session.Submit(new ConfirmDuelSelection(target.Id, new[] { target.Options.Single(o => o.CardId == source.InstanceId).Key })); Drain(session);
        Assert.That(session.Current.Card(source.InstanceId).Negated, Is.True);
        session.Submit(new ChangePhase(DuelPhase.End)); Drain(session);
        Assert.That(session.Current.Card(source.InstanceId).Negated, Is.False);
    }
    [Test]
    public void ZeroDamageDirectAttackStillProducesOneImpactAndClearsPreview()
    {
        var session = Create(new[] { "97268402", "89631139", "89631139" }, new[] { "89631139", "89631139", "89631139" });
        var source = session.Current.Cards.Single(c => c.Known && c.Definition.CardId == "97268402");
        session.Submit(new BeginCardAction(source.InstanceId, source.Actions.Single(a => a.Kind == DuelActionKind.NormalSummon).Id));
        session.Submit(new ConfirmActionTarget(session.Current.PendingAction.Targets[0])); Drain(session);
        session.Submit(new ChangePhase(DuelPhase.End)); Drain(session);
        session.Submit(new ChangePhase(DuelPhase.End)); Drain(session);
        session.Submit(new ChangePhase(DuelPhase.Battle)); Drain(session);
        int impacts = 0;
        session.Changed += change => { if(change.Kind == DuelChangeKind.Attack) { impacts++; Assert.That(change.TargetId, Is.Zero); } };
        session.Submit(new BeginCardAction(source.InstanceId, session.Current.Card(source.InstanceId).Actions.Single(a => a.Kind == DuelActionKind.Attack).Id));
        session.Submit(new PreviewDuelTarget(0)); Assert.That(session.HasAttackPreview, Is.True);
        session.Submit(new ConfirmDuelSelection(session.Current.Choice.Id, new[] { "direct" })); Drain(session);
        Assert.That(impacts, Is.EqualTo(1)); Assert.That(session.DeclaredAttacker, Is.Zero); Assert.That(session.HasAttackPreview, Is.False);
    }
    [Test]
    public void ResetDuringMovementDiscardsQueuedEventsAndPendingSelection()
    {
        var session = Create(new[] { "08240199", "89631139" }, new[] { "89631139", "08240199" });
        var card = session.Current.Cards.Single(c => c.Known && c.Definition.CardId == "08240199");
        session.Submit(new BeginCardAction(card.InstanceId, card.Actions.Single(a => a.Kind == DuelActionKind.NormalSummon).Id));
        session.Submit(new ConfirmActionTarget(session.Current.PendingAction.Targets[0]));
        Assert.That(session.Current.Animating, Is.True);
        session.Submit(new ResetDuel()); Drain(session);
        Assert.That(session.Current.Card(card.InstanceId).Zone.Kind, Is.EqualTo(DuelZone.Hand));
        Assert.That(session.Current.HasPendingAction, Is.False); Assert.That(session.Current.Choice.Active, Is.False);
    }
    [Test]
    public void CrossoutOffersRuleNameCandidatesAndCannotCancelCommittedBanishSelection()
    {
        var session = Create(new[] { "65681983", "08240199", "89631139" }, new[] { "89631139", "08240199" });
        var card=session.Current.Cards.Single(c=>c.Known && c.Definition.CardId=="65681983");
        session.Submit(new BeginCardAction(card.InstanceId,card.Actions.First(a=>a.Kind==DuelActionKind.Activate).Id));
        var names=session.Current.Choice; Assert.That(names.Searchable,Is.True);Assert.That(names.Options.Count,Is.EqualTo(2));
        string whiteName=Core.DuelCardCatalog.CreateDefault().Get("08240199").OriginalNameId;
        session.Submit(new ConfirmDuelSelection(names.Id,new[]{whiteName}));
        session.Submit(new ConfirmActionTarget(session.Current.PendingAction.Targets[0]));Drain(session);
        var banish=session.Current.Choice;Assert.That(banish.Active,Is.True);Assert.That(banish.CanCancel,Is.False);
        session.Submit(new CancelCardAction());Assert.That(session.Current.Choice.Id,Is.EqualTo(banish.Id));
        session.Submit(new ConfirmDuelSelection(banish.Id,new[]{banish.Options.Single(o=>o.DefinitionId=="08240199").Key}));Drain(session);
        Assert.That(session.Current.Cards.Any(c=>c.Known && c.Definition.CardId=="08240199" && c.Zone.Kind==DuelZone.Banished),Is.True);
    }
    [Test]
    public void EmptyResponsesDoNotFlipViewingSeatDuringOpeningAnimations()
    {
        var session=Create(new[]{"08240199","89631139"},new[]{"89631139","08240199"});
        var seats=new List<int>();session.Changed+=change=>seats.Add(change.View.ViewingSeat);
        session.Submit(new ResetDuel());Drain(session);
        Assert.That(seats.All(seat=>seat==0),Is.True);
    }
    [Test]
    public void LinkSummonOffersExtraDeckActionAndConsumesLegalMaterial()
    {
        var session=Create(new[]{"26077389","89631139","89631139"},new[]{"89631139","89631139","89631139"},extra:new[]{"08491308"});
        var raye=session.Current.Cards.Single(c=>c.Known&&c.Definition.CardId=="26077389");
        session.Submit(new BeginCardAction(raye.InstanceId,raye.Actions.Single(a=>a.Kind==DuelActionKind.NormalSummon).Id));
        session.Submit(new ConfirmActionTarget(session.Current.PendingAction.Targets[0]));Drain(session);
        if(session.Current.Choice.IsResponse){session.Submit(new CancelCardAction());Drain(session);}
        var link=session.Current.Cards.Single(c=>c.Known&&c.Definition.CardId=="08491308");
        Assert.That(link.Zone.Kind,Is.EqualTo(DuelZone.ExtraDeck));
        session.Submit(new BeginCardAction(link.InstanceId,link.Actions.Single(a=>a.Kind==DuelActionKind.SpecialSummon).Id));
        var material=session.Current.Choice;
        session.Submit(new ConfirmDuelSelection(material.Id,new[]{material.Options.Single(o=>o.CardId==raye.InstanceId).Key}));
        Assert.That(session.Current.PendingAction.Targets.All(z=>z.Kind==DuelZone.ExtraMonster),Is.True);
        session.Submit(new ConfirmActionTarget(session.Current.PendingAction.Targets[0]));Drain(session);
        Assert.That(session.Current.Card(link.InstanceId).Zone.Kind,Is.EqualTo(DuelZone.ExtraMonster));
        Assert.That(session.Current.Card(raye.InstanceId).Zone.Kind,Is.EqualTo(DuelZone.Graveyard));
    }
    [Test]
    public void ResponseAllowsBrowsingCardsButMandatorySelectionKeepsInputLocked()
    {
        var session=Create(new[]{"70368879","08240199","89631139"},new[]{"14558127","08240199"});
        Activate(session,"70368879");
        Assert.That(session.Current.Choice.IsResponse,Is.True);Assert.That(session.Current.CanBrowseCards,Is.True);
        Assert.That(session.Current.CanInteract,Is.False);
    }
    [Test]
    public void TimeoutEndsDuelAndResetRestoresClockAndOpening()
    {
        var session = Create(new[] { "08240199", "89631139" }, new[] { "89631139", "08240199" });
        session.Tick(181); Drain(session);
        Assert.That(session.Current.Finished, Is.True); Assert.That(session.Current.Outcome, Does.Contain("超时"));
        session.Submit(new ResetDuel()); Drain(session);
        Assert.That(session.Current.Finished, Is.False); Assert.That(session.Current.Seconds, Is.EqualTo(new[] { 180f, 180f }));
        Assert.That(session.Current.Phase, Is.EqualTo(DuelPhase.Main1));
    }
}
