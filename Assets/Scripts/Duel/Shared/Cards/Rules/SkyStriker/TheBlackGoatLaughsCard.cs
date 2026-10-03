using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TheBlackGoatLaughsCard : CardRules
    {
        public override string CardId => "49299410";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("49299410.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("49299410.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("49299410.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new BlackGoatAbility(CardId, 1);
            yield return new BlackGoatAbility(CardId, 2);
        }
    }

    sealed class BlackGoatAbility : ProgramAbility, IActivationNameDeclaration
    {
        static readonly CardNameCatalog Names = CardNameCatalog.CreateDefault();
        public RuleCardKind DeclaredKind => RuleCardKind.Monster;
        public IEnumerable<string> NameCandidates(EffectContext context) => Names.ForKind(RuleCardKind.Monster).Select(name => name.NameId);
        internal BlackGoatAbility(string cardId, int number) : base(cardId, number, 2,
            new[] { number == 1 ? DuelZone.SpellTrap : DuelZone.Graveyard }, c => true,
            (c, link) => c.AddEffect(new DuelEffectRecord {
                Kind = number == 1 ? EffectRecordKind.CannotSummonName : EffectRecordKind.CannotActivateName,
                NameId = link.StringValues["declared-name"], ExpiresTurn = c.State.Turn }))
        {
            Once(cardId + ".shared");
            Validate((c, command) => Names.Contains(command.NameId) && Names.Get(command.NameId).Kind == RuleCardKind.Monster
                ? "" : "INVALID_DECLARED_NAME");
            Pay((c, command, link) =>
            {
                link.StringValues["declared-name"] = command.NameId;
                if (number == 2) { link.Costs.Add(c.Source.Ref); c.MoveAsCost(c.Source, DuelZone.Banished); }
            });
        }
    }
}
