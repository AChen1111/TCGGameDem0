using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class MaskedHERODarkLawCard : CardRules
    {
        public override string CardId => "58481572";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("58481572.1", CardRuleKind.ContinuousRule, "continuous-effects"),
            CardRuleRequirement.Done("58481572.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("58481572.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("58481572.restrictions", CardRuleKind.Restriction));
        public override bool AllowsEffectSpecialSummon(DuelCardState card, int player, DuelState state) => false;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) => method == SummonMethod.MaskChange;
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new HeroContinuousRule(CardId + ".1", (context, output) => output.Add(new DuelEffectRecord {
                Kind = EffectRecordKind.BanishOpponentGraveyard, Source = context.SourceRef, Player = 1 - context.Player, RequiresSource = true }));
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceTriggerProgramAbility(CardId, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, false,
                (context, fact) => fact.Kind == DuelEventKind.Moved && fact.From == DuelZone.Deck && fact.To == DuelZone.Hand
                    && fact.Player == 1 - context.Player && context.State.Phase != DuelPhase.Draw,
                context => context.State.Cards.Any(card => card.Owner == 1 - context.Player && card.Zone == DuelZone.Hand),
                (context, link) => {
                    if (link.Step != 0) return;
                    var hand = context.State.Cards.Where(card => card.Owner == 1 - context.Player && card.Zone == DuelZone.Hand).OrderBy(card => card.InstanceId).ToArray();
                    if (hand.Length > 0) context.Move(hand[context.Engine.NextRandom((uint)hand.Length)], DuelZone.Banished);
                    link.Step = 1;
                });
        }
    }
}
