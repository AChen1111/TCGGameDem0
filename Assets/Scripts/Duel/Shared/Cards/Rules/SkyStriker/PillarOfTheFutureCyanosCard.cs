using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class PillarOfTheFutureCyanosCard : CardRules
    {
        public override string CardId => "20357457";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("20357457.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("20357457.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("20357457.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("20357457.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Hand },
                c => c.CanSpecialSummon(c.Source, c.Player),
                (c, link) => { if (c.Source.Zone == DuelZone.Hand) EffectSummonFlow.Resume(c, link, c.Source, 0); })
                .Cost(1, 1, c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Hand
                    && c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell),
                    (c, command, link) => { var card = c.Card(command.Cards[0]); link.Costs.Add(card.Ref); c.MoveAsCost(card, DuelZone.Graveyard); })
                .Once(CardId + ".1");
            IEnumerable<DuelCardState> Roze(EffectContext c) => c.State.Cards.Where(card => card.Owner == c.Player
                && (card.Zone == DuelZone.Deck || card.Zone == DuelZone.Graveyard)
                && c.Catalog.Get(card.DefinitionId).OriginalNameId == "37351133" && c.CanSpecialSummon(card, c.Player));
            yield return new TriggerProgramAbility(CardId, 2, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref) && fact.SummonMethod != SummonMethod.Flip,
                c => Roze(c).Any() && !c.State.TurnFacts.Any(fact => fact.Kind == DuelEventKind.Summoned
                    && fact.Player == c.Player && fact.From == DuelZone.ExtraDeck && c.Catalog.Get(fact.DefinitionId).Race != 32),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.OnlyExtraDeckRace,
                            Player = c.Player, Value = 32, ExpiresTurn = c.State.Turn });
                        var cards = Roze(c).ToArray(); link.Step = 1;
                        if (cards.Length == 0) { link.Step = 4; return; }
                        c.SelectCards(link, cards, "选择特殊召唤的露世"); return;
                    }
                    if (link.Step == 1) link.Values["summon-card"] = link.Selected[0];
                    EffectSummonFlow.Resume(c, link, c.Card(link.Values["summon-card"]), 1);
                }).Once(CardId + ".2").Category(EffectCategories.SpecialSummonFromDeck | EffectCategories.SpecialSummonFromGraveyard);
            IEnumerable<DuelCardState> RecoverableRoze(EffectContext c) => c.State.Cards.Where(card => card.Owner == c.Player
                && (card.Zone == DuelZone.Graveyard || card.Zone == DuelZone.Banished && card.Position != CardPosition.FaceDown)
                && c.Catalog.Get(card.DefinitionId).OriginalNameId == "37351133");
            yield return new ProgramAbility(CardId, 3, 1, new[] { DuelZone.Graveyard }, c => RecoverableRoze(c).Any(),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var cards = RecoverableRoze(c).ToArray(); link.Step = 1;
                        if (cards.Length == 0) { link.Step = 6; return; }
                        c.SelectCards(link, cards, "选择回收或特殊召唤的露世"); return;
                    }
                    if (link.Step == 1)
                    {
                        var card = c.Card(link.Selected[0]); link.Values["roze"] = card.InstanceId;
                        if (!c.CanSpecialSummon(card, c.Player))
                        { if (c.TryMove(card, DuelZone.Hand)) c.Reveal(card); link.Step = 6; return; }
                        link.Step = 2;
                        c.OpenDecision(link, DecisionKind.ChooseMode, new[] {
                            new DecisionOption { Id = "hand", Value = "hand", Label = "加入手卡" },
                            new DecisionOption { Id = "summon", Value = "summon", Label = "特殊召唤" } }, "选择露世的处理方式"); return;
                    }
                    if (link.Step == 2)
                    {
                        if (link.Answers[0] == "hand")
                        { var card = c.Card(link.Values["roze"]); if (c.TryMove(card, DuelZone.Hand)) c.Reveal(card); link.Step = 6; return; }
                        link.Step = 3;
                    }
                    EffectSummonFlow.Resume(c, link, c.Card(link.Values["roze"]), 3);
                }).Pay((c, command, link) => { link.Costs.Add(c.Source.Ref); c.MoveAsCost(c.Source, DuelZone.Banished); })
                .Once(CardId + ".3").Category(EffectCategories.AddFromGraveyardToHandDeckExtra | EffectCategories.SpecialSummonFromGraveyard);
        }
    }
}
