using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerMobilizeLinkageCard : CardRules
    {
        public override string CardId => "09726840";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("09726840.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            IEnumerable<DuelCardState> OtherField(EffectContext c) => c.State.Cards.Where(card => card.Controller == c.Player
                && DuelEngine.OnField(card) && card.InstanceId != c.Source.InstanceId);
            IEnumerable<DuelCardState> Extra(EffectContext c, bool checkCapacity) => c.State.Cards.Where(card => card.Owner == c.Player
                && card.Zone == DuelZone.ExtraDeck && c.Catalog.Get(card.DefinitionId).BelongsTo(0x1115)
                && c.Engine.CanSpecialSummonConditions(card, c.Player)
                && (!checkCapacity || c.Engine.GetSpecialSummonDestinations(card, c.Player).Any(slot => slot >= 5)));
            yield return new ProgramAbility(CardId, 1, 2, SkyFlow.SpellZones,
                c => SkyFlow.MainEmpty(c) && OtherField(c).Any() && Extra(c, false).Any()
                    && (OtherField(c).Any(card => card.Zone == DuelZone.ExtraMonster)
                        || !c.State.Cards.Any(card => card.Controller == c.Player && card.Zone == DuelZone.ExtraMonster)
                            && Enumerable.Range(0, 2).Any(slot => !c.State.Cards.Any(card => card.Zone == DuelZone.ExtraMonster && card.Slot == slot))),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var cards = OtherField(c).ToArray(); link.Step = 1;
                        if (cards.Length == 0) { link.Step = 6; return; }
                        c.SelectCards(link, cards, "选择送去墓地的其他卡"); return;
                    }
                    if (link.Step == 1)
                    {
                        var card = c.Card(link.Selected[0]);
                        if (!c.IsAffected(card) || !c.TryMove(card, DuelZone.Graveyard) || card.Zone != DuelZone.Graveyard) { link.Step = 6; return; }
                        var cards = Extra(c, true).ToArray(); link.Step = 2;
                        if (cards.Length == 0) { link.Step = 6; return; }
                        c.SelectCards(link, cards, "选择特殊召唤的闪刀姬"); return;
                    }
                    if (link.Step == 2) link.Values["linkage-card"] = link.Selected[0];
                    if (!EffectSummonFlow.Resume(c, link, c.Card(link.Values["linkage-card"]), 2, extraOnly: true)) return;
                    if (link.Step == 5)
                    {
                        var summoned = c.Card(link.Values["linkage-card"]);
                        var attributes = c.State.Cards.Where(card => card.Controller == c.Player && (DuelEngine.OnField(card) || card.Zone == DuelZone.Graveyard)
                            && c.Catalog.Get(card.DefinitionId).BelongsTo(0x1115)).Select(card => card.CurrentAttribute).ToArray();
                        if (summoned.Zone == DuelZone.ExtraMonster && attributes.Contains(16) && attributes.Contains(32))
                            c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack, Target = summoned.Ref, Value = 1000 });
                        link.Step = 6;
                    }
                }).Pay((c, command, link) => c.AddEffect(new DuelEffectRecord {
                    Kind = EffectRecordKind.OnlyExtraDeckSet, Player = c.Player, Value = 0x1115, ExpiresTurn = c.State.Turn,
                    RemoveIfActivationNegated = true, ChainId = c.State.CurrentChainId, ResponseToLink = link.Number }));
        }
    }
}
