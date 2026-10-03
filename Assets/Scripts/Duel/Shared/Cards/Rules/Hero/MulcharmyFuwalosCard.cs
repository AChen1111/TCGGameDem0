using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class MulcharmyFuwalosCard : CardRules
    {
        public override string CardId => "42141493";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("42141493.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("42141493.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new MulcharmyFuwalosAbility(); }
    }

    sealed class MulcharmyFuwalosAbility : IAbilityHandler, IActivationSourcePolicy, IActivationUsageLimit
    {
        public string CardId => "42141493";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public string UsageKey => "mulcharmy.effects";
        public int Limit => 2;
        public bool CountNegatedActivation => false;
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.Hand;
        public bool CanActivate(EffectContext context) => AllowsSource(context)
            && !context.State.Cards.Any(card => card.Controller == context.Player && DuelEngine.OnField(card));
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { link.Costs.Add(context.SourceRef); context.MoveAsCost(context.Source, DuelZone.Graveyard); }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step != 0) return;
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.DrawOnOpponentSpecialSummon, Player = context.Player,
                Value = (1 << (int)DuelZone.Deck) | (1 << (int)DuelZone.ExtraDeck), ExpiresTurn = context.State.Turn });
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.EndShuffleHand, Player = context.Player,
                Value = 6, ExpiresTurn = context.State.Turn });
            link.Step = 1;
        }
    }
}
