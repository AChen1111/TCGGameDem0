using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class CalledByTheGraveCard : CardRules
    {
        public override string CardId => "24224830";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("24224830.1", CardRuleKind.ActivatedAbility, "name-negation", "turn-expiry"));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new CalledByTheGraveAbility(); }
    }

    sealed class CalledByTheGraveAbility : IAbilityHandler, IActivationTargetSelection, IEffectCategoryProvider
    {
        public string CardId => "24224830";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public EffectCategories Categories => EffectCategories.BanishFromGraveyard;
        public IEnumerable<DuelCardState> TargetCandidates(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == 1 - context.Player && card.Zone == DuelZone.Graveyard
            && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster);
        public bool CanActivate(EffectContext context) => TargetCandidates(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 ? "" : "NO_ACTIVATION_COST";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step != 0) return;
            link.Step = 1;
            var target = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && card.Zone == DuelZone.Graveyard);
            if (target == null) return;
            string originalName = context.Catalog.Get(target.DefinitionId).OriginalNameId;
            context.Move(target, DuelZone.Banished);
            if (target.Zone != DuelZone.Banished) return;
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.NameNegate,
                Source = context.SourceRef, NameId = originalName, Value = 1, ExpiresTurn = context.State.Turn + 1 });
        }
    }
}
