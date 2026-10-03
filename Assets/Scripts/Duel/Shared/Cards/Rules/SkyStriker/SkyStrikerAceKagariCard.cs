using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceKagariCard : CardRules
    {
        public override string CardId => "63288574";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("63288574.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63288574.2", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("63288574.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("63288574.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override bool HasSummonRecipe => true;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.Source.Ref), c => true,
                (c, link) =>
                {
                    foreach (var target in c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard))
                    { c.Move(target, DuelZone.Hand); c.Reveal(target); }
                }).Target(c => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.Graveyard
                    && c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell && SkyFlow.Striker(c, card) && c.CanTarget(card)))
                .Category(EffectCategories.AddFromGraveyardToHandDeckExtra);
        }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".2", (c, output) =>
            {
                if (SkyFlow.Active(c)) output.Add(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack,
                    Source = c.Source.Ref, Target = c.Source.Ref, RequiresSource = true, Value = SkyFlow.GraveSpells(c) * 100 });
            });
        }
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 1 && cards.All(c => c.BelongsTo(0x1115)) && materials[0].CurrentAttribute != 4;
        }
    }
}
