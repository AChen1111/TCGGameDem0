using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class CrossoutDesignatorCard : CardRules
    {
        public override string CardId => "65681983";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("65681983.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("65681983.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new CrossoutDesignatorAbility(); }
    }

    sealed class CrossoutDesignatorAbility : IAbilityHandler, IActivationUsageLimit, IActivationNameDeclaration
    {
        static readonly CardNameCatalog s_names = CardNameCatalog.CreateDefault();
        public string CardId => "65681983";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public string UsageKey => CardId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public RuleCardKind DeclaredKind => (RuleCardKind)0;
        public IEnumerable<string> NameCandidates(EffectContext context) => context.Deck
            .Select(card => context.Catalog.Get(card.DefinitionId).OriginalNameId).Where(s_names.Contains).Distinct(System.StringComparer.Ordinal);
        public bool CanActivate(EffectContext context) => context.Deck.Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 && s_names.Contains(command.NameId)
            && context.Deck.Any(card => context.Catalog.Get(card.DefinitionId).OriginalNameId == command.NameId) ? "" : "INVALID_DECLARED_NAME";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { link.StringValues["declared-name"] = command.NameId; }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = context.Deck.Where(card => context.Catalog.Get(card.DefinitionId).OriginalNameId == link.StringValues["declared-name"]).ToArray();
                link.Step = 1;
                if (candidates.Length == 0) { link.Step = 2; return; }
                context.SelectCards(link, candidates, "选择宣言同名卡并除外"); return;
            }
            if (link.Step == 1)
            {
                var selected = context.Card(link.Selected[0]);
                context.Move(selected, DuelZone.Banished);
                if (selected.Zone == DuelZone.Banished) context.AddEffect(new DuelEffectRecord {
                    Kind = EffectRecordKind.NameNegate, NameId = link.StringValues["declared-name"], ExpiresTurn = context.State.Turn });
                context.ShuffleDeck(); link.Step = 2;
            }
        }
    }
}
