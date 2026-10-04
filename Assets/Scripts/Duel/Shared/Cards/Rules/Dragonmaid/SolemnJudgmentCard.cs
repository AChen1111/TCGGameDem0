using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SolemnJudgmentCard : CardRules
    {
        public override string CardId => "41420027";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("41420027.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("41420027.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new SolemnJudgmentAbility(); }
    }

    sealed class SolemnJudgmentAbility : IAbilityHandler, ISummonNegation, IActivationSourcePolicy
    {
        public string CardId => "41420027";
        public string AbilityId => CardId + ".1";
        public int Speed => 3;
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.SpellTrap
            && (context.Source.Position == CardPosition.FaceDown || context.Source.Position == CardPosition.FaceUp);
        public bool CanActivate(EffectContext context) => context.Source.SetTurn < context.State.Turn
            && context.State.Players[context.Player].LifePoints >= 2 && (SpellTrap(context) || Summon(context));
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        {
            link.Values["solemn-summon"] = Summon(context) ? 1 : 0;
            if (link.Values["solemn-summon"] == 0) link.Values["solemn-link"] = context.State.Chain.Last().Number;
            context.State.Players[context.Player].LifePoints /= 2;
        }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step != 0) return;
            if (link.Values["solemn-summon"] == 1)
            {
                var monster = context.State.Cards.FirstOrDefault(card => card.InstanceId == context.State.PendingSummonId && DuelEngine.OnField(card));
                if (monster != null && context.IsAffected(monster)) context.Move(monster, DuelZone.Graveyard);
            }
            else context.State.Chain.Single(item => item.Number == link.Values["solemn-link"]).ActivationNegated = true;
            link.Step = 1;
        }
        static bool SpellTrap(EffectContext context) => context.State.Chain.Count > 0 && context.State.Chain.Last().IsCardActivation
            && (context.State.Chain.Last().ActivationKind == RuleCardKind.Spell || context.State.Chain.Last().ActivationKind == RuleCardKind.Trap);
        static bool Summon(EffectContext context) => context.State.PendingSummonId != 0 && context.State.Chain.Count == 0
            && context.State.Cards.Any(card => card.InstanceId == context.State.PendingSummonId && DuelEngine.OnField(card));
    }
}
