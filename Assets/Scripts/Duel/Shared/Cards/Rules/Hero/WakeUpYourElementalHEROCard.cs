using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class WakeUpYourElementalHEROCard : CardRules
    {
        public override string CardId => "32828466";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("32828466.1", CardRuleKind.ContinuousRule, "continuous-effects"),
            CardRuleRequirement.Done("32828466.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("32828466.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("32828466.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("32828466.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override bool SuppressSummonModifiersWhenNegated => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2147483647;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count >= 2 && materials.Any(card => catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Fusion && catalog.Get(card.DefinitionId).BelongsTo(0x3008) && materials.Where(other => other != card).All(other => other.CurrentRace == 1));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new HeroContinuousRule(CardId + ".1", (context, output) => {
                if (context.Source.SummonModifiersSuppressed) return;
                output.Add(HeroContinuousRule.Record(context, context.Source, EffectRecordKind.AddAttack, context.Source.SummonMaterialDefinitions.Count * 300));
                int fusions = context.Source.SummonMaterialDefinitions.Count(id => context.Catalog.Get(id).MonsterType == RuleMonsterType.Fusion);
                output.Add(HeroContinuousRule.Record(context, context.Source, EffectRecordKind.ExtraMonsterAttacks, System.Math.Max(0, fusions - 1)));
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, true, false,
                (context, fact) => fact.Kind == DuelEventKind.BattleStepChanged && fact.Amount == (int)BattleStep.AfterCalculation
                    && fact.BattleTarget.InstanceId != 0 && (fact.BattleAttacker.Equals(context.SourceRef) || fact.BattleTarget.Equals(context.SourceRef)),
                context => true, (context, link) => {
                    if (link.Step != 0) return;
                    var fact = link.TriggerEvent;
                    var reference = fact.BattleAttacker.Equals(link.Source) ? fact.BattleTarget : fact.BattleAttacker;
                    var victim = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(reference));
                    if (victim == null) { link.Step = 1; return; }
                    var operation = context.DestroyMany(link, new[] { victim });
                    if (!operation.Completed) return;
                    if (operation.DestroyedBefore.Count > 0) context.Engine.Damage(1 - context.Player,
                        System.Math.Max(0, context.Catalog.Get(operation.DestroyedBefore[0].DefinitionId).Attack));
                    link.Step = 1;
                });
            yield return new TriggerProgramAbility(CardId, 3, new[] { DuelZone.Graveyard, DuelZone.Banished }, true, false,
                (context, fact) => fact.Kind == DuelEventKind.Destroyed && fact.Card.Equals(context.SourceRef)
                    && fact.Before.SummonMethod == SummonMethod.Fusion && fact.Before.ProperlySummoned,
                context => WarriorChoices(context).Any(), (context, link) => {
                    if (link.Step == 0)
                    {
                        var candidates = WarriorChoices(context).ToArray(); link.Step = 1;
                        if (candidates.Length == 0) { link.Step = 5; return; }
                        context.SelectCards(link, candidates, "选择特殊召唤的战士族"); return;
                    }
                    if (link.Step == 1)
                    { link.Values["summon-card"] = link.Selected[0]; link.Values["from-deck"] = context.Card(link.Selected[0]).Zone == DuelZone.Deck ? 1 : 0; }
                    if (link.Step >= 1 && link.Step <= 3 && EffectSummonFlow.Resume(context, link, context.Card(link.Values["summon-card"]), 1))
                    { if (link.Values["from-deck"] == 1) context.ShuffleDeck(); link.Step = 5; }
                }).Category(EffectCategories.SpecialSummonFromDeck);
        }
        static IEnumerable<DuelCardState> WarriorChoices(EffectContext context) => context.State.Cards.Where(card => card.Owner == context.Player
            && (card.Zone == DuelZone.Hand || card.Zone == DuelZone.Deck) && card.CurrentRace == 1 && context.CanSpecialSummon(card, context.Player));
    }
}
