using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class FiliaRegisCard : CardRules
    {
        public override string CardId => "70538272";
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion || method == SummonMethod.Effect && card.Zone != DuelZone.ExtraDeck && card.ProperlySummoned;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var fusions = materials.Where(card =>
            {
                var printed = catalog.Get(card.DefinitionId);
                return DragonmaidFlow.IsDragon(printed) && printed.MonsterType == RuleMonsterType.Fusion;
            }).ToArray();
            var large = materials.Where(card => DragonmaidFlow.IsDragon(catalog.Get(card.DefinitionId)) && card.CurrentLevel >= 7).ToArray();
            return materials.Count == 2 && fusions.Length > 0 && large.Length > 0
                && fusions.Concat(large).Select(card => card.InstanceId).Distinct().Count() == 2;
        }
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("70538272.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("70538272.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("70538272.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("70538272.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, DragonmaidFlow.Field,
                c => (c.State.Phase == DuelPhase.Main1 || c.State.Phase == DuelPhase.Main2) && Foes(c).Any(),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]));
                        link.Values["field"] = target != null && DuelEngine.OnField(target) ? 1 : 0;
                        if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Banished);
                        link.Step = 1;
                        if (link.Values["field"] == 0 || !Dragons(c).Any()) { link.Step = 2; return; }
                        c.SelectCards(link, Dragons(c), "选择回到手卡的龙族");
                        return;
                    }
                    if (link.Step != 1) return;
                    var dragon = c.Card(link.Selected[0]);
                    if (DuelEngine.OnField(dragon) && c.IsAffected(dragon))
                        c.Move(dragon, c.Catalog.Get(dragon.DefinitionId).IsExtra ? DuelZone.ExtraDeck : DuelZone.Hand);
                    link.Step = 2;
                }).Target(Foes).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Battle) && c.State.TurnPlayer != c.Player,
                c => c.Source.ProperlySummoned && Dragons(c).Any() && c.CanSpecialSummon(c.Source, c.Player),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        link.Step = 1;
                        c.SelectCards(link, Dragons(c), "选择回到手卡或额外卡组的龙族");
                        return;
                    }
                    if (link.Step == 1)
                    {
                        var dragon = c.Card(link.Selected[0]);
                        if (DuelEngine.OnField(dragon) && c.IsAffected(dragon))
                            c.Move(dragon, c.Catalog.Get(dragon.DefinitionId).IsExtra ? DuelZone.ExtraDeck : DuelZone.Hand);
                        link.Values["picked"] = c.Source.InstanceId;
                        link.Step = 2;
                    }
                    DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
                }).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Foes(EffectContext context) => context.State.Cards.Where(card =>
            (card.Controller != context.Player && DuelEngine.OnField(card) || card.Owner != context.Player && card.Zone == DuelZone.Graveyard)
            && context.CanTarget(card));
        static IEnumerable<DuelCardState> Dragons(EffectContext context) =>
            DragonmaidFlow.FieldMonsters(context, context.Player).Where(card =>
                DragonmaidFlow.IsDragon(context.Catalog.Get(card.DefinitionId)) && context.CanTarget(card));
    }
}
