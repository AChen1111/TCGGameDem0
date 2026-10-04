using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DragonmaidSheouCard : CardRules
    {
        public override string CardId => "24799107";
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.Any(card => DragonmaidFlow.IsMaid(catalog.Get(card.DefinitionId)))
            && materials.Any(card => DragonmaidFlow.IsDragon(catalog.Get(card.DefinitionId)) && card.CurrentLevel >= 5);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("24799107.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("24799107.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("24799107.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("24799107.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new TriggerProgramAbility(CardId, 1, DragonmaidFlow.Field, false, false,
                (c, fact) => DragonmaidFlow.Phase(fact, DuelPhase.Standby),
                c => Small(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var list = Small(c).ToArray();
                        link.Step = 1;
                        if (list.Length == 0) { link.Step = 8; return; }
                        c.SelectCards(link, list, "选择特殊召唤的半龙女仆");
                        return;
                    }
                    if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                    DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
                }).Once(CardId + ".1");
            yield return new ProgramAbility(CardId, 2, 2, DragonmaidFlow.Field,
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().Player != c.Player,
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var previous = c.State.Chain.Single(item => item.Number == link.Values["sheou-link"]);
                        previous.ActivationNegated = true;
                        if (!previous.IsCardActivation)
                        {
                            var source = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(previous.Source) && DuelEngine.OnField(card));
                            if (source != null && c.IsAffected(source)) c.Destroy(source);
                        }
                        if (DuelEngine.OnField(c.Source)) c.Move(c.Source, DuelZone.ExtraDeck);
                        var houses = Houses(c).ToArray();
                        link.Step = 1;
                        if (houses.Length == 0) { link.Step = 8; return; }
                        c.SelectCards(link, houses, "选择特殊召唤的龙女管家");
                        return;
                    }
                    if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                    DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
                }).Pay((c, command, link) => link.Values["sheou-link"] = c.State.Chain.Last().Number).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Small(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Hand, DuelZone.Graveyard).Where(card =>
            {
                var printed = context.Catalog.Get(card.DefinitionId);
                return DragonmaidFlow.IsMaid(printed) && printed.Level > 0 && printed.Level <= 9 && context.CanSpecialSummon(card, context.Player);
            });
        static IEnumerable<DuelCardState> Houses(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.ExtraDeck).Where(card => card.DefinitionId == "41232647"
                && context.CanSpecialSummon(card, context.Player));
    }
}
