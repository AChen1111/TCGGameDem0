using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class LightAndDarknessDragonlordCard : CardRules
    {
        public override string CardId => "19652159";
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            method == SummonMethod.Fusion;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => DragonmaidFlow.IsDragon(catalog.Get(card.DefinitionId)) && card.CurrentLevel == 8
                && (DragonmaidFlow.Attr(card, DragonmaidFlow.Light) || DragonmaidFlow.Attr(card, DragonmaidFlow.Dark)))
            && materials.Any(card => DragonmaidFlow.Attr(card, DragonmaidFlow.Light))
            && materials.Any(card => DragonmaidFlow.Attr(card, DragonmaidFlow.Dark));
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("19652159.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("19652159.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("19652159.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("19652159.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("19652159.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, output) =>
            {
                if (!DuelEngine.OnField(c.Source) || !DuelEngine.IsPublic(c.Source) || c.Source.Negated) return;
                output.Add(new DuelEffectRecord { Kind = EffectRecordKind.SetAttribute, Target = c.SourceRef, Value = 48, RequiresSource = true });
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 2, 2, DragonmaidFlow.Field,
                c => c.Source.CurrentAtk >= 1000 && c.Source.CurrentDef >= 1000 && c.State.Chain.Count > 0
                    && c.State.Chain.All(link => link.AbilityId != CardId + ".2") && Responds(c.State.Chain.Last()),
                (c, link) =>
                {
                    if (link.Step != 0) return;
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack, Target = c.SourceRef, Value = -1000 });
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddDefense, Target = c.SourceRef, Value = -1000 });
                    c.State.Chain.Single(item => item.Number == link.Values["lord-link"]).ActivationNegated = true;
                    link.Step = 1;
                }).Pay((c, command, link) => link.Values["lord-link"] = c.State.Chain.Last().Number);
            yield return new TriggerProgramAbility(CardId, 3, new[] { DuelZone.Graveyard, DuelZone.Banished }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Destroyed && fact.Card.Equals(c.Source.Ref) && fact.Before != null
                    && fact.Before.Controller == c.Player && (fact.Cause == MoveCause.Battle || fact.EffectPlayer >= 0 && fact.EffectPlayer != c.Player),
                c => Dragons(c).Any(), (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        link.Step = 1;
                        c.SelectCards(link, Dragons(c), "选择特殊召唤的龙族");
                        return;
                    }
                    if (link.Step == 1) { link.Values["picked"] = link.Selected[0]; link.Step = 2; }
                    DragonmaidFlow.ResumeSummon(c, link, 2, false, SummonMethod.Effect);
                });
        }
        static bool Responds(DuelChainLink link) => link.ActivationKind == RuleCardKind.Monster
            || link.IsCardActivation && (link.ActivationKind == RuleCardKind.Spell || link.ActivationKind == RuleCardKind.Trap);
        static IEnumerable<DuelCardState> Dragons(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.Graveyard).Where(card =>
                DragonmaidFlow.IsDragon(context.Catalog.Get(card.DefinitionId)) && context.CanSpecialSummon(card, context.Player) && context.CanTarget(card));
    }
}
