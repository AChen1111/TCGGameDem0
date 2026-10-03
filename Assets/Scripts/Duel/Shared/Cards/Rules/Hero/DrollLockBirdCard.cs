using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DrollLockBirdCard : CardRules
    {
        public override string CardId => "94145021";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("94145021.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new DrollLockBirdAbility(); }
    }

    sealed class DrollLockBirdAbility : IAbilityHandler, IActivationSourcePolicy
    {
        public string CardId => "94145021";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.Hand;
        public bool CanActivate(EffectContext context) => AllowsSource(context) && context.State.Phase != DuelPhase.Draw
            && !context.Engine.HasEffect(EffectRecordKind.BanishOpponentGraveyard, context.Source.Owner, context.Source)
            && context.State.LastCheckpointEvents.Any(fact => fact.Kind == DuelEventKind.Moved && fact.From == DuelZone.Deck
                && fact.To == DuelZone.Hand && fact.Player == 1 - context.Player);
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { link.Costs.Add(context.SourceRef); context.MoveAsCost(context.Source, DuelZone.Graveyard); }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step != 0) return;
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.PreventDeckToHand, ExpiresTurn = context.State.Turn });
            link.Step = 1;
        }
    }
}
