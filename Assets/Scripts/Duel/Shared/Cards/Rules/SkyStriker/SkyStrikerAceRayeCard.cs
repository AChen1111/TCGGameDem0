using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAceRayeCard : CardRules
    {
        public override string CardId => "26077389";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("26077389.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("26077389.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("26077389.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            IEnumerable<DuelCardState> Extra(EffectContext c) => c.State.Cards.Where(card => card.Owner == c.Player
                && card.Zone == DuelZone.ExtraDeck && c.Catalog.Get(card.DefinitionId).BelongsTo(0x1115)
                && c.CanSpecialSummon(card, c.Player) && c.Engine.GetSpecialSummonDestinations(card, c.Player).Any(slot => slot >= 5));
            yield return new ProgramAbility(CardId, 1, 2, SkyFlow.MonsterZones,
                c => Extra(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var candidates = Extra(c).ToArray(); link.Step = 1;
                        if (candidates.Length == 0) { link.Step = 4; return; }
                        c.SelectCards(link, candidates, "选择特殊召唤的闪刀姬"); return;
                    }
                    if (link.Step == 1) link.Values["summon-card"] = link.Selected[0];
                    EffectSummonFlow.Resume(c, link, c.Card(link.Values["summon-card"]), 1, extraOnly: true);
                }).Pay((c, command, link) => { link.Costs.Add(c.Source.Ref); c.MoveAsCost(c.Source, DuelZone.Graveyard); })
                .Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => fact.Before != null && fact.Before.Controller == c.Player
                    && c.Catalog.Get(fact.Before.DefinitionId).BelongsTo(0x1115)
                    && c.Catalog.Get(fact.Before.DefinitionId).MonsterType == RuleMonsterType.Link
                    && (fact.Kind == DuelEventKind.Destroyed && fact.Cause == MoveCause.Battle
                        || fact.Kind == DuelEventKind.Moved && fact.Cause == MoveCause.Effect && fact.EffectPlayer == 1 - c.Player
                            && (fact.Before.Zone == DuelZone.Monster || fact.Before.Zone == DuelZone.ExtraMonster)
                            && fact.To != DuelZone.Monster && fact.To != DuelZone.ExtraMonster && fact.To != DuelZone.SpellTrap && fact.To != DuelZone.Field),
                c => c.CanSpecialSummon(c.Source, c.Player),
                (c, link) => EffectSummonFlow.Resume(c, link, c.Source, 0)).Once(CardId + ".2");
        }
    }
}
