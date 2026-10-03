using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceRozeCard : CardRules
    {
        public override string CardId => "37351133";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("37351133.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("37351133.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("37351133.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, new[] { DuelZone.Hand }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.SummonMethod != SummonMethod.Flip
                    && c.Catalog.Get(fact.DefinitionId).BelongsTo(0x1115) && c.Catalog.Get(fact.DefinitionId).OriginalNameId != CardId,
                c => c.CanSpecialSummon(c.Source, c.Player),
                (c, link) => { if (c.Source.Zone == DuelZone.Hand) EffectSummonFlow.Resume(c, link, c.Source, 0); })
                .Once(CardId + ".1");
            IEnumerable<DuelCardState> Negatable(EffectContext c) => c.State.Cards.Where(card => card.Controller == 1 - c.Player
                && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(card));
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => fact.Before != null && fact.Before.Controller == 1 - c.Player && fact.Before.Zone == DuelZone.ExtraMonster
                    && (fact.Kind == DuelEventKind.Destroyed && fact.Cause == MoveCause.Battle
                        || fact.Kind == DuelEventKind.Moved && fact.Cause == MoveCause.Effect && fact.EffectPlayer == c.Player
                            && fact.To != DuelZone.Monster && fact.To != DuelZone.ExtraMonster && fact.To != DuelZone.SpellTrap && fact.To != DuelZone.Field),
                c => c.CanSpecialSummon(c.Source, c.Player), (c, link) =>
                {
                    if (link.Step < 3 && !EffectSummonFlow.Resume(c, link, c.Source, 0)) return;
                    if (link.Step == 3)
                    {
                        if (c.Source.Zone != DuelZone.Monster || !Negatable(c).Any()) { link.Step = 6; return; }
                        link.Step = 4; c.OpenDecision(link, DecisionKind.YesNo, new[] {
                            new DecisionOption { Id = "yes", Value = "yes", Label = "无效对方怪兽" },
                            new DecisionOption { Id = "no", Value = "no", Label = "不无效" } }, "是否无效一只对方怪兽？"); return;
                    }
                    if (link.Step == 4)
                    {
                        if (link.Answers[0] == "no") { link.Step = 6; return; }
                        link.Step = 5; c.SelectCards(link, Negatable(c), "选择无效的对方怪兽"); return;
                    }
                    if (link.Step == 5)
                    {
                        var card = c.Card(link.Selected[0]);
                        if (c.IsAffected(card)) c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.TargetNegate,
                            Target = card.Ref, ExpiresTurn = c.State.Turn }); link.Step = 6;
                    }
                }).Once(CardId + ".2").Category(EffectCategories.SpecialSummonFromGraveyard);
        }
    }
}
