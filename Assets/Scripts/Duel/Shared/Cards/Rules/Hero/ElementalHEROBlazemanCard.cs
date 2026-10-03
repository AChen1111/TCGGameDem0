using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ElementalHEROBlazemanCard : CardRules
    {
        public override string CardId => "63060238";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("63060238.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63060238.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("63060238.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new HeroDeckTriggerAbility(CardId, 1, context => context.Deck.Where(card => card.DefinitionId == "24094653"),
                DuelZone.Hand, HeroDeckTriggerAbility.SelfSummoned, HeroDeckTriggerAbility.FaceUpMonster,
                EffectCategories.AddFromDeckToHand, CardId);
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => Candidates(context).Any(), (context, link) => {
                    if (link.Step >= 3) return;
                    if (HeroChoiceOperations.SelectAndMove(context, link, Candidates(context), DuelZone.Graveyard)
                        && link.Step == 2)
                    {
                        if (link.Selected.Count > 0)
                        {
                            var selected = context.Card(link.Selected[0]);
                            if (selected.Zone == DuelZone.Graveyard && context.Source.Ref.Equals(link.Source) && DuelEngine.OnField(context.Source))
                            {
                                context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetAttribute, Target = link.Source,
                                    Value = selected.CurrentAttribute, ExpiresTurn = context.State.Turn, ResetIfSourceNegated = true });
                                context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetAttack, Target = link.Source,
                                    Value = selected.CurrentAtk, ExpiresTurn = context.State.Turn, ResetIfSourceNegated = true });
                                context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetDefense, Target = link.Source,
                                    Value = selected.CurrentDef.Value, ExpiresTurn = context.State.Turn, ResetIfSourceNegated = true });
                            }
                        }
                        context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.OnlySummonMethod,
                            Player = context.Player, Value = (int)SummonMethod.Fusion, ExpiresTurn = context.State.Turn });
                        link.Step = 3;
                    }
                }).Once(CardId).Category(EffectCategories.SendDeckToGraveyard);
        }
        static IEnumerable<DuelCardState> Candidates(EffectContext context) => context.Deck.Where(card =>
            card.DefinitionId != "63060238" && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && context.Catalog.Get(card.DefinitionId).BelongsTo(0x3008));
    }
}
