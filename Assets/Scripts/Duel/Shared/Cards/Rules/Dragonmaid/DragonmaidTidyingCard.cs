using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonmaidTidyingCard : CardRules
    {
        public override string CardId => "57416183";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("57416183.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("57416183.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("57416183.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Hand, DuelZone.SpellTrap },
                c => Dragons(c).Any() && OpponentCards(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        link.Step = 1;
                        c.SelectCards(link, Dragons(c), "选择回到手卡的龙族");
                        return;
                    }
                    if (link.Step == 1)
                    {
                        link.Values["own"] = link.Selected[0];
                        link.Step = 2;
                        c.SelectCards(link, OpponentCards(c), "选择回到手卡的对方卡");
                        return;
                    }
                    if (link.Step != 2) return;
                    foreach (int id in new[] { link.Values["own"], link.Selected[0] })
                    {
                        var card = c.State.Cards.FirstOrDefault(item => item.InstanceId == id);
                        if (card != null && c.IsAffected(card)) c.Move(card, DuelZone.Hand);
                    }
                    link.Step = 3;
                }).Once(CardId);
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Graveyard }, c => Maids(c).Any(), (c, link) =>
            {
                if (link.Step == 0)
                {
                    c.MoveAsCost(c.Source, DuelZone.Banished);
                    var list = Maids(c).ToArray();
                    link.Step = 1;
                    if (list.Length == 0) { link.Step = 8; return; }
                    c.SelectCards(link, list, "选择特殊召唤的半龙女仆");
                    return;
                }
                if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                if (link.Step < 5) DragonmaidFlow.ResumeSummon(c, link, 2, true, SummonMethod.Effect);
                if (link.Step == 5)
                {
                    var summoned = c.Card(link.Values["picked"]);
                    if (DuelEngine.OnField(summoned))
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.EndReturnToHand, Target = summoned.Ref,
                            ExpiresTurn = c.State.Turn, ExpiresPhase = DuelPhase.End });
                    link.Step = 6;
                }
            }).Once(CardId);
        }
        static IEnumerable<DuelCardState> Dragons(EffectContext context) => DragonmaidFlow.FieldMonsters(context, context.Player)
            .Where(card => DragonmaidFlow.IsDragon(context.Catalog.Get(card.DefinitionId)) && context.CanTarget(card));
        static IEnumerable<DuelCardState> OpponentCards(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner != context.Player && (DuelEngine.OnField(card) || card.Zone == DuelZone.Graveyard) && context.CanTarget(card));
        static IEnumerable<DuelCardState> Maids(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Graveyard).Where(card =>
                DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && context.CanSpecialSummon(card, context.Player));
    }
}
