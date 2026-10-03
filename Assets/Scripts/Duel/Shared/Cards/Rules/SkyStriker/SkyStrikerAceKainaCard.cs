using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceKainaCard : CardRules
    {
        public override string CardId => "12421694";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("12421694.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("12421694.2", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("12421694.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("12421694.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override bool HasSummonRecipe => true;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref), c => true,
                (c, link) =>
                {
                    foreach (var target in c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card) && c.IsAffected(card)))
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotAttack, Target = target.Ref,
                            ExpiresTurn = c.State.Turn + (c.State.TurnPlayer == c.Player ? 1 : 0) });
                }).Target(c => c.State.Cards.Where(card => card.Controller == 1 - c.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card) && c.CanTarget(card)));
        }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".2", (c, output) =>
            {
                if (SkyFlow.Active(c)) output.Add(new DuelEffectRecord { Kind = EffectRecordKind.RecoverOnSpellActivated,
                    Source = c.Source.Ref, RequiresSource = true, Player = c.Player, Value = 100, SetCode = 0x115 });
            });
        }
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 1 && cards.All(c => c.BelongsTo(0x1115)) && materials[0].CurrentAttribute != 1;
        }
    }
}
