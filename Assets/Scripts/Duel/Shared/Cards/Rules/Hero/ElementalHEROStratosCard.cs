using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ElementalHEROStratosCard : CardRules
    {
        public override string CardId => "40044918";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("40044918.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new StratosAbility(); }
    }

    sealed class StratosAbility : IAbilityHandler, ITriggeredAbility, IActivationSourcePolicy, IActivationModeSelection
    {
        public string CardId => "40044918";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public bool Mandatory => false;
        public bool OptionalWhen => true;
        public bool AllowsSource(EffectContext context) => HeroDeckTriggerAbility.FaceUpMonster(context);
        public bool CanActivate(EffectContext context) => AllowsSource(context) && Modes(context).Any();
        public bool IsTriggered(EffectContext context, DuelEvent fact) => HeroDeckTriggerAbility.SelfSummoned(context, fact)
            && fact.SummonMethod != SummonMethod.Flip;
        public IEnumerable<AbilityMode> Modes(EffectContext context)
        {
            if (!context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player) && SearchChoices(context).Any())
                yield return new AbilityMode { Id = "search", Label = "检索英雄", Categories = EffectCategories.AddFromDeckToHand };
            if (OtherHeroes(context) > 0 && DestructionChoices(context).Any())
                yield return new AbilityMode { Id = "destroy", Label = "破坏魔法陷阱" };
        }
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        static IEnumerable<DuelCardState> SearchChoices(EffectContext context) => context.Deck.Where(card =>
            context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8));
        static int OtherHeroes(EffectContext context) => HeroContinuousRule.Monsters(context, context.Player).Count(card =>
            card.InstanceId != context.Source.InstanceId && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8));
        static IEnumerable<DuelCardState> DestructionChoices(EffectContext context) => context.State.Cards.Where(card =>
            card.Zone == DuelZone.SpellTrap || card.Zone == DuelZone.Field);
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.ModeId == "search") { HeroChoiceOperations.SelectAndMove(context, link, SearchChoices(context), DuelZone.Hand); return; }
            if (link.Step == 0)
            {
                var candidates = DestructionChoices(context).ToArray();
                int maximum = System.Math.Min(OtherHeroes(context), candidates.Length); link.Step = 1;
                if (maximum == 0) { link.Step = 2; return; }
                context.SelectCards(link, candidates, "选择破坏的魔法陷阱", 1, maximum); return;
            }
            if (link.Step == 1)
            {
                if (!link.Values.ContainsKey("destroy-count"))
                {
                    link.Values["destroy-count"] = link.Selected.Count;
                    for (int index = 0; index < link.Selected.Count; index++) link.Values["destroy-" + index] = link.Selected[index];
                }
                var operation = context.DestroyMany(link, Enumerable.Range(0, link.Values["destroy-count"]).Select(index => context.Card(link.Values["destroy-" + index])));
                if (!operation.Completed) return;
                link.Step = 2;
            }
        }
    }
}
