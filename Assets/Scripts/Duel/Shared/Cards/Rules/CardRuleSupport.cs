using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public enum CardRuleKind { ActivatedAbility, ContinuousRule, SummonProcedure, Restriction }

    public sealed class CardRuleRequirement
    {
        public string Id { get; }
        public CardRuleKind Kind { get; }
        public bool IsImplemented { get; }
        public IReadOnlyList<string> MechanismDependencies { get; }
        public CardRuleRequirement(string id, CardRuleKind kind, bool isImplemented, params string[] dependencies)
        {
            Id = id; Kind = kind; IsImplemented = isImplemented;
            MechanismDependencies = Array.AsReadOnly((string[])dependencies.Clone());
        }
        public static CardRuleRequirement Done(string id, CardRuleKind kind, params string[] dependencies) =>
            new CardRuleRequirement(id, kind, true, dependencies);
        public static CardRuleRequirement Missing(string id, CardRuleKind kind, params string[] dependencies) =>
            new CardRuleRequirement(id, kind, false, dependencies);
    }

    public sealed class CardRuleSupport
    {
        public IReadOnlyList<CardRuleRequirement> Requirements { get; }
        public bool IsComplete => Requirements.All(rule => rule.IsImplemented);
        public CardRuleSupport(params CardRuleRequirement[] requirements) =>
            Requirements = Array.AsReadOnly((CardRuleRequirement[])requirements.Clone());
    }

    public sealed class MissingCardRule
    {
        public string CardId { get; }
        public string RuleId { get; }
        public CardRuleKind Kind { get; }
        public IReadOnlyList<string> MechanismDependencies { get; }
        public MissingCardRule(string cardId, CardRuleRequirement requirement)
        {
            CardId = cardId; RuleId = requirement.Id; Kind = requirement.Kind;
            MechanismDependencies = requirement.MechanismDependencies;
        }
    }
}
