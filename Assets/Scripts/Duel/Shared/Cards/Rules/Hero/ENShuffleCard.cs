using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ENShuffleCard : CardRules
    {
        public override string CardId => "10186633";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("10186633.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("10186633.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("10186633.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        { yield return new ENShuffleAbility(); yield return new ENShuffleGraveAbility(); }
    }

    sealed class ENShuffleAbility : IAbilityHandler, IActivationUsageLimit, IEffectCategoryProvider
    {
        public string CardId => "10186633";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => true;
        public EffectCategories Categories => EffectCategories.SpecialSummonFromDeck;
        internal static bool HeroOrSpacian(CardDefinition card) => card.BelongsTo(0x3008) || card.BelongsTo(0x1f);
        static IEnumerable<DuelCardState> FieldChoices(EffectContext context) => HeroContinuousRule.Monsters(context, context.Player).Where(card =>
            HeroOrSpacian(context.Catalog.Get(card.DefinitionId)) && context.IsAffected(card)
            && (!context.Catalog.Get(card.DefinitionId).IsExtra || !context.Engine.HasEffect(EffectRecordKind.CannotReturnToExtra, card.Controller, card))
            && context.Deck.Any(other => HeroOrSpacian(context.Catalog.Get(other.DefinitionId))
                && context.Catalog.Get(other.DefinitionId).OriginalNameId != card.CurrentNameId
                && context.Engine.CanSpecialSummonConditions(other, context.Player)
                && (card.Zone == DuelZone.Monster || context.Engine.GetSpecialSummonDestinations(other, context.Player).Any())));
        static IEnumerable<DuelCardState> SummonChoices(EffectContext context, string name) => context.Deck.Where(card =>
            HeroOrSpacian(context.Catalog.Get(card.DefinitionId)) && context.Catalog.Get(card.DefinitionId).OriginalNameId != name
            && context.CanSpecialSummon(card, context.Player));
        public bool CanActivate(EffectContext context) => FieldChoices(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var choices = FieldChoices(context).ToArray(); link.Step = 1;
                if (choices.Length == 0) { link.Step = 6; return; }
                context.SelectCards(link, choices, "选择返回卡组的怪兽"); return;
            }
            if (link.Step == 1)
            {
                var returned = context.Card(link.Selected[0]); string name = returned.CurrentNameId;
                context.Move(returned, DuelZone.Deck, CardPosition.FaceDown);
                if (DuelEngine.OnField(returned)) { link.Step = 6; return; }
                context.ShuffleDeck();
                var choices = SummonChoices(context, name).ToArray(); link.Step = 2;
                if (choices.Length == 0) { link.Step = 6; return; }
                context.SelectCards(link, choices, "选择不同卡名的怪兽"); return;
            }
            if (link.Step == 2) { link.Values["summon-card"] = link.Selected[0]; link.Step = 3; }
            if (link.Step >= 3 && link.Step <= 5 && EffectSummonFlow.Resume(context, link, context.Card(link.Values["summon-card"]), 3)
                && link.Step == 6) context.ShuffleDeck();
        }
    }

    sealed class ENShuffleGraveAbility : IAbilityHandler, IActivationSourcePolicy, IActivationUsageLimit, IEffectCategoryProvider
    {
        public string CardId => "10186633";
        public string AbilityId => CardId + ".2";
        public int Speed => 1;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => true;
        public EffectCategories Categories => EffectCategories.AddFromGraveyardToHandDeckExtra | EffectCategories.AddFromDeckToHand;
        public bool AllowsSource(EffectContext context) => context.Source.Zone == DuelZone.Graveyard;
        static List<List<DuelCardState>> Groups(EffectContext context)
        {
            var grave = context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.Graveyard).ToArray();
            var groups = grave.Where(card => context.Catalog.Get(card.DefinitionId).OriginalNameId == "89943723")
                .Select(card => new List<DuelCardState> { card }).ToList();
            foreach (var hero in grave.Where(card => context.Catalog.Get(card.DefinitionId).BelongsTo(0x3008)))
                foreach (var spacian in grave.Where(card => context.Catalog.Get(card.DefinitionId).BelongsTo(0x1f)))
                    if (hero.InstanceId != spacian.InstanceId) groups.Add(new List<DuelCardState> { hero, spacian });
            return groups;
        }
        public bool CanActivate(EffectContext context) => AllowsSource(context)
            && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player) && Groups(context).Count > 0;
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { link.Costs.Add(context.SourceRef); context.MoveAsCost(context.Source, DuelZone.Banished); }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var groups = Groups(context); link.Step = 1;
                if (groups.Count == 0) { link.Step = 2; return; }
                context.SelectCards(link, groups.SelectMany(group => group).Distinct(), "选择返回卡组的怪兽", groups.Min(group => group.Count), groups.Max(group => group.Count));
                context.State.PendingDecision.AllowedCardGroups = groups.Select(group => group.Select(card => card.Ref).ToList()).ToList(); return;
            }
            if (link.Step == 1)
            {
                bool allReturned = true;
                foreach (int id in link.Selected)
                {
                    var card = context.Card(id); context.Move(card, DuelZone.Deck, CardPosition.FaceDown);
                    if (card.Zone != (context.Catalog.Get(card.DefinitionId).IsExtra ? DuelZone.ExtraDeck : DuelZone.Deck)) allReturned = false;
                }
                context.ShuffleDeck();
                if (allReturned) context.Engine.Draw(context.Player, 1);
                link.Step = 2;
            }
        }
    }
}
