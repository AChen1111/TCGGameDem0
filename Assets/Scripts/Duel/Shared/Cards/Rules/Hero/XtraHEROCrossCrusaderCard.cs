using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class XtraHEROCrossCrusaderCard : CardRules
    {
        public override string CardId => "58004362";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("58004362.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("58004362.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("58004362.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("58004362.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 2 && materials.All(c => c.CurrentRace == 1);
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (context, fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact) && fact.SummonMethod == SummonMethod.Link,
                context => HeroOnlyHistory(context), (context, link) => {
                    if (link.Step >= 3) return;
                    var target = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
                    if (target == null) { link.Step = 3; return; }
                    EffectSummonFlow.Resume(context, link, target, 0);
                }).Target(context => context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.Graveyard
                    && context.Catalog.Get(card.DefinitionId).BelongsTo(0xc008) && context.CanSpecialSummon(card, context.Player)))
                .Pay(RestrictToHero).Once(CardId + ".1").Category(EffectCategories.SpecialSummonFromGraveyard);
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => HeroOnlyHistory(context) && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)
                    && context.Deck.Any(card => context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)),
                (context, link) => HeroChoiceOperations.SelectAndMove(context, link, context.Deck.Where(card =>
                    context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)
                    && context.Catalog.Get(card.DefinitionId).OriginalNameId != link.StringValues["tribute-name"]), DuelZone.Hand))
                .Cost(1, 1, context => HeroContinuousRule.Monsters(context, context.Player).Where(card => context.Catalog.Get(card.DefinitionId).BelongsTo(0xc008)),
                    (context, command, link) => {
                        var tribute = context.Card(command.Cards[0]); link.Costs.Add(tribute.Ref);
                        link.StringValues["tribute-name"] = context.Catalog.Get(tribute.DefinitionId).OriginalNameId;
                        context.MoveAsCost(tribute, DuelZone.Graveyard); RestrictToHero(context, command, link);
                    })
                .Validate((context, command) => context.Deck.Any(card => context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)
                    && context.Catalog.Get(card.DefinitionId).OriginalNameId != context.Catalog.Get(context.Card(command.Cards[0]).DefinitionId).OriginalNameId)
                    ? "" : "NO_DIFFERENT_HERO")
                .Once(CardId + ".2").Category(EffectCategories.AddFromDeckToHand);
        }
        static bool HeroOnlyHistory(EffectContext context) => !context.State.TurnFacts.Any(fact =>
            fact.Kind == DuelEventKind.Summoned && fact.Player == context.Player
            && fact.SummonMethod != SummonMethod.Normal && fact.SummonMethod != SummonMethod.Flip
            && !context.Catalog.Get(fact.DefinitionId).BelongsTo(0x8));
        static void RestrictToHero(EffectContext context, DuelCommand command, DuelChainLink link) =>
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.OnlySpecialSummonSet, Player = context.Player,
                Value = 0x8, ExpiresTurn = context.State.Turn });
    }
}
