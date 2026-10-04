using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class BorreloadFuriousDragonCard : CardRules
    {
        public override string CardId => "92892239";
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => DragonmaidFlow.IsDragon(catalog.Get(card.DefinitionId)) && DragonmaidFlow.Attr(card, DragonmaidFlow.Dark));
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("92892239.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("92892239.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("92892239.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("92892239.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, DragonmaidFlow.Field, c => Mine(c).Any() && Foes(c).Any(), (c, link) =>
            {
                if (link.Step == 0)
                {
                    link.Step = 1;
                    c.SelectCards(link, Mine(c), "选择破坏的自己怪兽");
                    return;
                }
                if (link.Step == 1)
                {
                    link.Values["own"] = link.Selected[0];
                    link.Step = 2;
                    c.SelectCards(link, Foes(c), "选择破坏的对方卡");
                    return;
                }
                if (link.Step != 2) return;
                foreach (int id in new[] { link.Values["own"], link.Selected[0] })
                {
                    var card = c.State.Cards.FirstOrDefault(item => item.InstanceId == id && DuelEngine.OnField(item));
                    if (card != null && c.IsAffected(card)) c.Destroy(card);
                }
                link.Step = 3;
            }).Once(CardId + ".1");
            yield return new ProgramAbility(CardId, 2, 1, new[] { DuelZone.Graveyard }, c => Links(c).Any(), (c, link) =>
            {
                if (link.Step == 0)
                {
                    c.Move(c.Source, DuelZone.Banished);
                    link.Step = 1;
                    c.SelectCards(link, Links(c), "选择特殊召唤的暗属性连接怪兽");
                    return;
                }
                if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                if (link.Step < 5) DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
                if (link.Step == 5)
                {
                    var summoned = c.Card(link.Values["picked"]);
                    if (DuelEngine.OnField(summoned))
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotActivateCard, Target = summoned.Ref, Player = -1, ExpiresTurn = c.State.Turn });
                    link.Step = 6;
                }
            }).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Mine(EffectContext context) =>
            DragonmaidFlow.FieldMonsters(context, context.Player).Where(context.CanTarget);
        static IEnumerable<DuelCardState> Foes(EffectContext context) =>
            context.State.Cards.Where(card => card.Controller != context.Player && DuelEngine.OnField(card) && context.CanTarget(card));
        static IEnumerable<DuelCardState> Links(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Graveyard).Where(card =>
            {
                var printed = context.Catalog.Get(card.DefinitionId);
                return printed.MonsterType == RuleMonsterType.Link && DragonmaidFlow.Attr(card, DragonmaidFlow.Dark)
                    && context.CanSpecialSummon(card, context.Player) && context.CanTarget(card);
            });
    }
}
