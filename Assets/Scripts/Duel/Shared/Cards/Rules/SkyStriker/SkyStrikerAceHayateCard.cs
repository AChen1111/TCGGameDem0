using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceHayateCard : CardRules
    {
        public override string CardId => "08491308";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("08491308.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("08491308.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("08491308.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("08491308.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override bool HasSummonRecipe => true;
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            IEnumerable<DuelCardState> Candidates(EffectContext c) => c.Deck.Where(card => SkyFlow.Striker(c, card));
            yield return new TriggerProgramAbility(CardId, 2, SkyFlow.MonsterZones, false, false,
                (c, fact) => fact.Kind == DuelEventKind.BattleStepChanged && fact.Amount == (int)BattleStep.AfterCalculation
                    && (fact.BattleAttacker.Equals(c.Source.Ref) || fact.BattleTarget.Equals(c.Source.Ref)),
                c => Candidates(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var cards = Candidates(c).ToArray(); link.Step = 1;
                        if (cards.Length == 0) { link.Step = 2; return; }
                        c.SelectCards(link, cards, "选择送去墓地的闪刀卡"); return;
                    }
                    if (link.Step == 1) { c.Move(c.Card(link.Selected[0]), DuelZone.Graveyard); c.ShuffleDeck(); link.Step = 2; }
                }).Category(EffectCategories.SendDeckToGraveyard);
        }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, output) =>
            {
                if (SkyFlow.Active(c)) output.Add(new DuelEffectRecord { Kind = EffectRecordKind.DirectAttack,
                    Source = c.Source.Ref, Target = c.Source.Ref, RequiresSource = true });
            });
        }
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 1 && cards.All(c => c.BelongsTo(0x1115)) && materials[0].CurrentAttribute != 8;
        }
    }
}
