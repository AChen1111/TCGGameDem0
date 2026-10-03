using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ReturnOfTheDragonLordsCard : CardRules
    {
        public override string CardId => "06853254";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("06853254.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("06853254.2", CardRuleKind.ContinuousRule, "destruction-replacement"));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new ReturnOfDragonLordsAbility(); }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".2", (context, records) =>
            {
                if (context.Source.Zone == DuelZone.Graveyard)
                    records.Add(new DuelEffectRecord { Kind = EffectRecordKind.DestroyReplacement,
                        Source = context.SourceRef, Player = context.Source.Owner, Value = 8192,
                        SourceDefinitionId = context.Source.DefinitionId });
            });
        }
    }

    sealed class ReturnOfDragonLordsAbility : IAbilityHandler, IActivationTargetSelection, IEffectCategoryProvider
    {
        public string CardId => "06853254";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public EffectCategories Categories => EffectCategories.SpecialSummonFromGraveyard;
        public IEnumerable<DuelCardState> TargetCandidates(EffectContext context) => context.State.Cards.Where(c =>
            c.Owner == context.Player && c.Zone == DuelZone.Graveyard && c.CurrentRace == 8192
            && (c.CurrentLevel == 7 || c.CurrentLevel == 8) && context.CanSpecialSummon(c, context.Player));
        public bool CanActivate(EffectContext context) => TargetCandidates(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) => command.Cards.Length == 0 ? "" : "NO_ACTIVATION_COST";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step >= 3) return;
            var target = context.State.Cards.FirstOrDefault(c => c.Ref.Equals(link.Targets.Single()) && c.Zone == DuelZone.Graveyard);
            if (target == null || !context.CanSpecialSummon(target, context.Player)) { link.Step = 3; return; }
            EffectSummonFlow.Resume(context, link, target, 0);
        }
    }
}
