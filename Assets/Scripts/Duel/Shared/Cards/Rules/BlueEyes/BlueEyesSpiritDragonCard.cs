using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class BlueEyesSpiritDragonCard : CardRules
    {
        public override string CardId => "59822133";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count >= 2 && materials.Count(c => catalog.Get(c.DefinitionId).IsTuner) == 1 && materials.All(c => c.CurrentLevel > 0) && materials.Sum(c => c.CurrentLevel) == target.Level && materials.Where(c => !catalog.Get(c.DefinitionId).IsTuner).All(c => catalog.Get(c.DefinitionId).BelongsTo(0xdd));
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("59822133.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("59822133.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("59822133.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("59822133.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("59822133.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, records) =>
            {
                if ((c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(c.Source) && !c.Source.Negated)
                    records.Add(new DuelEffectRecord { Kind = EffectRecordKind.SummonGroupLimit, Value = 1 });
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 3, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.Source.SummonMethod == SummonMethod.Synchro && ExtraCandidates(c).Any(), (c, link) =>
                {
                    if (link.Step >= 3) return;
                    if (link.Step == 0)
                    {
                        var cards = ExtraCandidates(c).ToArray(); link.Step = 1;
                        if (cards.Length == 0) { link.Step = 3; return; }
                        c.SelectCards(link, cards, "选择龙族光属性同调怪兽"); return;
                    }
                    if (link.Step == 1)
                    {
                        var card = c.Card(link.Selected[0]); link.Values["spirit-summon-card"] = card.InstanceId;
                        var slots = c.Engine.GetSpecialSummonDestinations(card, c.Player);
                        if (slots.Count == 0) { link.Step = 3; return; }
                        link.Step = 2;
                        c.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption {
                            Id = slot.ToString(System.Globalization.CultureInfo.InvariantCulture), Value = slot.ToString(System.Globalization.CultureInfo.InvariantCulture), Label = "区域 " + (slot + 1) }), "选择守备表示特殊召唤的区域");
                        return;
                    }
                    var summoned = c.Card(link.Values["spirit-summon-card"]);
                    if (c.SpecialSummon(summoned, c.Player, int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture), CardPosition.FaceUpDefense))
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.DelayedDestroy, Target = summoned.Ref,
                            Player = c.Player, ExpiresTurn = c.State.Turn });
                    link.Step = 3;
                }).Pay((c, command, link) => { link.Costs.Add(c.SourceRef); c.MoveAsCost(c.Source, DuelZone.Graveyard); });
            yield return new BlueInstanceDamageProgram(CardId, 2, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().ActivationZone == DuelZone.Graveyard, (c, link) =>
                {
                    if (link.Step != 0) return;
                    c.State.Chain.Single(item => item.Number == link.Values["spirit-counter-link"]).ActivationNegated = true;
                    link.Step = 1;
                }, step => true).Pay((c, command, link) => link.Values["spirit-counter-link"] = c.State.Chain.Last().Number);
        }
        static IEnumerable<DuelCardState> ExtraCandidates(EffectContext c) => c.State.Cards.Where(card => card.Owner == c.Player
            && card.Zone == DuelZone.ExtraDeck && card.DefinitionId != "59822133"
            && c.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Synchro
            && c.Catalog.Get(card.DefinitionId).Race == 8192 && c.Catalog.Get(card.DefinitionId).Attribute == 16 && c.CanSpecialSummon(card, c.Player));
    }
}
