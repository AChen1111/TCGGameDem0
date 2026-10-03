using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonSpiritOfWhiteCard : CardRules
    {
        public override string CardId => "45467446";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("45467446.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("45467446.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("45467446.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("45467446.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 3, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.Cards.Any(card => card.Controller != c.Player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster))
                    && HandWhites(c).Any(), (c, link) =>
                {
                    if (link.Step >= 4) return;
                    if (link.Step == 0)
                    {
                        var candidates = HandWhites(c).ToArray(); link.Step = 1;
                        if (candidates.Length == 0) { link.Step = 4; return; }
                        c.SelectCards(link, candidates, "从手牌特殊召唤青眼白龙"); return;
                    }
                    if (link.Step == 1) link.Values["white-selected"] = link.Selected[0];
                    EffectSummonFlow.Resume(c, link, c.Card(link.Values["white-selected"]), 1);
                }).Pay((c, command, link) => { link.Costs.Add(c.SourceRef); c.MoveAsCost(c.Source, DuelZone.Graveyard); });
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster }, false, true,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.Card.Equals(c.SourceRef) && fact.SummonMethod != SummonMethod.Flip,
                c => true, (c, link) =>
                {
                    if (link.Step != 0) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.SpellTrap || card.Zone == DuelZone.Field));
                    if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Banished);
                    link.Step = 1;
                }).Target(c => c.State.Cards.Where(card => card.Controller != c.Player
                    && (card.Zone == DuelZone.SpellTrap || card.Zone == DuelZone.Field) && c.CanTarget(card)));
        }
        static IEnumerable<DuelCardState> HandWhites(EffectContext c) => c.State.Cards.Where(card => card.Controller == c.Player
            && card.Zone == DuelZone.Hand && card.CurrentNameId == "89631139" && c.CanSpecialSummon(card, c.Player));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, records) =>
            {
                if (c.Source.Zone == DuelZone.Hand || c.Source.Zone == DuelZone.Graveyard)
                    records.Add(new DuelEffectRecord { Kind = EffectRecordKind.TreatAsNormal, Target = c.SourceRef, Value = 1 });
            });
        }
    }
}
