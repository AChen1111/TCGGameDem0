using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DestinyHERODestroyerPhoenixEnforcerCard : CardRules
    {
        public override string CardId => "60461804";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("60461804.1", CardRuleKind.ContinuousRule, "continuous-effects"),
            CardRuleRequirement.Done("60461804.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("60461804.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("60461804.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("60461804.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.Any(card => card.CurrentLevel >= 6 && catalog.Get(card.DefinitionId).BelongsTo(0x8) && materials.Any(other => other != card && catalog.Get(other.DefinitionId).BelongsTo(0xc008)));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new HeroContinuousRule(CardId + ".1", (context, output) => {
                int loss = context.State.Cards.Count(card => card.Owner == context.Player && card.Zone == DuelZone.Graveyard
                    && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)) * 200;
                foreach (var target in HeroContinuousRule.Monsters(context, 1 - context.Player))
                    output.Add(HeroContinuousRule.Record(context, target, EffectRecordKind.AddAttack, -loss));
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 2, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => context.State.Cards.Count(DuelEngine.OnField) >= 2
                    && context.State.Cards.Any(card => card.Controller == context.Player && DuelEngine.OnField(card)), ResolveDestruction)
                .Once(CardId + ".2");
            yield return new TriggerProgramAbility(CardId, 3, new[] { DuelZone.Graveyard, DuelZone.Banished }, false, false,
                (context, fact) => fact.Kind == DuelEventKind.Destroyed && fact.Card.Equals(context.Source.Ref)
                    && (fact.Cause == MoveCause.Effect || fact.Cause == MoveCause.Battle), context => true,
                (context, link) => {
                    if (link.Step != 0) return;
                    context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.DelayedSummon,
                        Player = context.Player, Value = 0xc008, ExpiresTurn = context.State.Turn + 1, ExpiresPhase = DuelPhase.Standby });
                    link.Step = 1;
                }).Once(CardId + ".3").Category(EffectCategories.SpecialSummonFromGraveyard);
        }
        static void ResolveDestruction(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var own = context.State.Cards.Where(card => card.Controller == context.Player && DuelEngine.OnField(card)).ToArray();
                link.Step = 1;
                if (own.Length == 0) { link.Step = 3; return; }
                context.SelectCards(link, own, "选择自己场上的卡"); return;
            }
            if (link.Step == 1)
            {
                link.Values["destroy-own"] = link.Selected[0];
                var other = context.State.Cards.Where(card => card.InstanceId != link.Selected[0] && DuelEngine.OnField(card)).ToArray();
                link.Step = 2;
                if (other.Length == 0) { link.Step = 3; return; }
                context.SelectCards(link, other, "选择场上的另一张卡"); return;
            }
            if (link.Step == 2)
            {
                if (!link.Values.ContainsKey("destroy-other")) link.Values["destroy-other"] = link.Selected[0];
                var operation = context.DestroyMany(link, new[] { context.Card(link.Values["destroy-own"]), context.Card(link.Values["destroy-other"]) });
                if (!operation.Completed) return;
                link.Step = 3;
            }
        }
    }
}
