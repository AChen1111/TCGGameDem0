using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class EffectVeilerCard : CardRules
    {
        public override string CardId => "97268402";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("97268402.1", CardRuleKind.ActivatedAbility, "temporary-negation"));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new VeilerAbility(); }
    }

    sealed class VeilerAbility : IAbilityHandler, IActivationSourcePolicy, IActivationTargetSelection
    {
        public string CardId => "97268402";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.Hand;
        public IEnumerable<DuelCardState> TargetCandidates(EffectContext context) => context.State.Cards.Where(c => c.Controller != context.Player
            && (c.Zone == DuelZone.Monster || c.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(c) && !c.CurrentNormal && context.CanTarget(c));
        public bool CanActivate(EffectContext context) => AllowsSource(context) && context.State.TurnPlayer != context.Player
            && (context.State.Phase == DuelPhase.Main1 || context.State.Phase == DuelPhase.Main2) && TargetCandidates(context).Any()
            && !context.Engine.ApplicableEffects().Any(e => e.Kind == EffectRecordKind.BanishOpponentGraveyard
                && (e.Player < 0 || e.Player == context.Source.Owner) && (e.Target.InstanceId == 0 || e.Target.Equals(context.SourceRef))
                && context.Engine.IsAffectedBy(context.Source, e.Source));
        public string ValidateActivation(EffectContext context, DuelCommand command) => command.Cards.Length == 0 ? "" : "NO_ADDITIONAL_COST";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { link.Costs.Add(context.SourceRef); context.MoveAsCost(context.Source, DuelZone.Graveyard); }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            var target = context.State.Cards.FirstOrDefault(c => c.Ref.Equals(link.Targets[0]));
            if (target != null && context.IsAffected(target)) context.AddEffect(new DuelEffectRecord
                { Kind = EffectRecordKind.TargetNegate, Target = target.Ref, ExpiresTurn = context.State.Turn });
            link.Step = 1;
        }
    }
}
