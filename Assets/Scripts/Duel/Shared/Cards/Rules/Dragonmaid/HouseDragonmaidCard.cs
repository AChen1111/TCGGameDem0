using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class HouseDragonmaidCard : CardRules
    {
        public override string CardId => "41232647";
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion || method == SummonMethod.Effect;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.Any(card => DragonmaidFlow.IsMaid(catalog.Get(card.DefinitionId)))
            && materials.Any(card => DragonmaidFlow.IsDragon(catalog.Get(card.DefinitionId)));
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("41232647.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("41232647.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("41232647.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("41232647.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Standby),
                c => Targets(c).Any() && Summons(c, null).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        link.Step = 1;
                        c.SelectCards(link, Targets(c), "选择等级基准的半龙女仆");
                        return;
                    }
                    if (link.Step == 1)
                    {
                        var basis = c.Card(link.Selected[0]);
                        var list = Summons(c, basis).ToArray();
                        link.Step = 2;
                        if (list.Length == 0) { link.Step = 8; return; }
                        c.SelectCards(link, list, "选择守备表示特殊召唤的半龙女仆");
                        return;
                    }
                    if (link.Step == 2) { link.Values["picked"] = link.Selected[0]; link.Step = 3; }
                    DragonmaidFlow.ResumeSummon(c, link, 3, true, SummonMethod.Effect);
                }).Once(CardId + ".1");
            yield return new TriggerProgramAbility(CardId, 2, DragonmaidFlow.Field, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Moved && fact.To == DuelZone.Hand && fact.Before != null
                    && (fact.Before.Zone == DuelZone.Monster || fact.Before.Zone == DuelZone.ExtraMonster)
                    && fact.Before.Controller == c.Player && !fact.Before.Ref.Equals(c.Source.Ref)
                    && (fact.Before.Position == CardPosition.FaceUpAttack || fact.Before.Position == CardPosition.FaceUpDefense)
                    && c.Catalog.Get(fact.Before.DefinitionId).Race == DragonmaidFlow.Dragon,
                c => Foes(c).Any(), (c, link) =>
                {
                    if (link.Step != 0) return;
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                    if (target != null && c.IsAffected(target)) c.Destroy(target);
                    link.Step = 1;
                }).Target(Foes);
        }
        static IEnumerable<DuelCardState> Targets(EffectContext context) => DragonmaidFlow.FieldMonsters(context, context.Player)
            .Where(card => card.InstanceId != context.Source.InstanceId && DragonmaidFlow.IsMaid(context.Catalog.Get(card.DefinitionId)) && context.CanTarget(card));
        static IEnumerable<DuelCardState> Summons(EffectContext context, DuelCardState basis)
        {
            int level = basis == null ? 0 : basis.CurrentLevel;
            return DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Graveyard).Where(card =>
            {
                var printed = context.Catalog.Get(card.DefinitionId);
                return DragonmaidFlow.IsMaid(printed) && printed.Level > 0 && (printed.Level == level + 1 || printed.Level == level - 1)
                    && context.CanSpecialSummon(card, context.Player);
            });
        }
        static IEnumerable<DuelCardState> Foes(EffectContext context) =>
            DragonmaidFlow.FieldMonsters(context, 1 - context.Player).Where(context.CanTarget);
    }
}
