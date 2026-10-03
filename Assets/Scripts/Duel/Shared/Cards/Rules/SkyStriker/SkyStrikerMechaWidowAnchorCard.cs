using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerMechaWidowAnchorCard : CardRules
    {
        public override string CardId => "98338152";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("98338152.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, SkyFlow.SpellZones, SkyFlow.MainEmpty,
                (c, link) =>
                {
                    foreach (var target in c.State.Cards.Where(card => card.Ref.Equals(link.Targets[0])
                        && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                        && DuelEngine.IsPublic(card) && !card.CurrentNormal && c.IsAffected(card)))
                    {
                        if (link.Step == 0)
                        {
                            c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.TargetNegate,
                                Target = target.Ref, ExpiresTurn = c.State.Turn });
                            link.Step = 1;
                            if (SkyFlow.GraveSpells(c) < 3 || !Enumerable.Range(0, 5).Any(slot => !c.State.Cards.Any(card =>
                                card.Controller == c.Player && card.Zone == DuelZone.Monster && card.Slot == slot))) { link.Step = 3; return; }
                            c.OpenDecision(link, DecisionKind.YesNo, new[] {
                                new DecisionOption { Id = "yes", Value = "yes", Label = "取得控制权" },
                                new DecisionOption { Id = "no", Value = "no", Label = "不取得控制权" } }, "是否取得目标控制权？"); return;
                        }
                        if (link.Step == 1)
                        {
                            if (link.Answers[0] == "no") { link.Step = 3; return; }
                            link.Step = 2;
                            c.OpenDecision(link, DecisionKind.ChooseZone, Enumerable.Range(0, 5)
                                .Where(slot => !c.State.Cards.Any(card => card.Controller == c.Player && card.Zone == DuelZone.Monster && card.Slot == slot))
                                .Select(slot => new DecisionOption { Id = slot.ToString(), Value = slot.ToString(), Label = "区域 " + (slot + 1) }), "选择怪兽区域"); return;
                        }
                        if (link.Step == 2)
                        {
                            int previous = target.Controller;
                            c.Engine.Move(target, DuelZone.Monster, c.Player, int.Parse(link.Answers[0]), target.Position,
                                MoveCause.Effect, link.Source, c.Player);
                            c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.ReturnControl, Target = target.Ref,
                                Player = previous, ExpiresTurn = c.State.Turn }); link.Step = 3;
                        }
                    }
                }).Target(c => c.State.Cards.Where(card => (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                    && DuelEngine.IsPublic(card) && !card.CurrentNormal && !card.Negated && c.CanTarget(card)));
        }
    }
}
