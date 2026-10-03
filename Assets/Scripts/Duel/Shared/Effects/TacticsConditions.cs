using System.Linq;

namespace AChen.Duel.Core
{
    internal static class TacticsConditions
    {
        public static bool OpponentActivatedMonster(EffectContext context, bool ownMainOnly) =>
            context.State.TurnFacts.Any(fact => fact.Kind == DuelEventKind.Activated && fact.Player == 1 - context.Player
                && fact.ActivationKind == RuleCardKind.Monster
                && (!ownMainOnly || fact.PhaseAtEvent == DuelPhase.Main1 || fact.PhaseAtEvent == DuelPhase.Main2)
                && !context.State.TurnFacts.Any(result => result.Kind == DuelEventKind.Negated && result.ActivationNegated
                    && result.ChainId == fact.ChainId && result.LinkNumber == fact.LinkNumber));
    }
}
