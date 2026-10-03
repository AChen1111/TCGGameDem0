using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceShizukuCard : CardRules
    {
        public override string CardId => "90673289";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("90673289.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("90673289.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("90673289.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("90673289.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override bool HasSummonRecipe => true;
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, output) =>
            {
                if (!SkyFlow.Active(c)) return;
                int decrease = -SkyFlow.GraveSpells(c) * 100;
                foreach (var target in c.State.Cards.Where(card => card.Controller == 1 - c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card)))
                {
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack, Target = target.Ref,
                        Source = c.Source.Ref, RequiresSource = true, Value = decrease });
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.AddDefense, Target = target.Ref,
                        Source = c.Source.Ref, RequiresSource = true, Value = decrease });
                }
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            IEnumerable<DuelCardState> Candidates(EffectContext c)
            {
                var graveNames = c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Graveyard)
                    .Select(card => c.Catalog.Get(card.DefinitionId).OriginalNameId).ToArray();
                return c.Deck.Where(card => c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell
                    && SkyFlow.Striker(c, card) && !graveNames.Contains(c.Catalog.Get(card.DefinitionId).OriginalNameId));
            }
            yield return new InstancePhaseTriggerProgramAbility(CardId, 2, SkyFlow.MonsterZones, false, false,
                (c, fact) => c.State.Phase == DuelPhase.End && c.State.TurnPlayer == c.Player && c.Source.SummonedTurn == c.State.Turn
                    && (fact.Kind == DuelEventKind.PhaseChanged && fact.Detail == DuelPhase.End.ToString()
                        || fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref)),
                c => c.State.Phase == DuelPhase.End && c.State.TurnPlayer == c.Player && c.Source.SummonedTurn == c.State.Turn,
                c => Candidates(c).Any() && !c.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, c.Player),
                (c, link) => SkyFlow.Search(c, link, Candidates(c))).Category(EffectCategories.AddFromDeckToHand);
        }
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 1 && cards.All(c => c.BelongsTo(0x1115)) && materials[0].CurrentAttribute != 2;
        }
    }
}
