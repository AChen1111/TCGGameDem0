using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class FavoriteContactCard : CardRules
    {
        public override string CardId => "75047173";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("75047173.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("75047173.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new FavoriteContactAbility(); }
    }

    sealed class FavoriteContactAbility : IAbilityHandler, IActivationUsageLimit, IEffectCategoryProvider
    {
        public string CardId => "75047173";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public string UsageKey => CardId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public EffectCategories Categories => EffectCategories.AddFromGraveyardToHandDeckExtra;
        static IEnumerable<DuelCardState> Materials(EffectContext context) => context.State.Cards.Where(card =>
            context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && (!context.Catalog.Get(card.DefinitionId).IsExtra || !context.Engine.HasEffect(EffectRecordKind.CannotReturnToExtra, card.Controller, card))
            && (card.Controller == context.Player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                || card.Owner == context.Player && (card.Zone == DuelZone.Hand || card.Zone == DuelZone.Graveyard
                    || card.Zone == DuelZone.Banished && card.Position != CardPosition.FaceDown)));
        static IReadOnlyList<DuelCardState[]> Groups(EffectContext context, DuelCardState target) =>
            context.Engine.GetFusionMaterialGroups(target, context.Player, Materials(context), maxMaterials: 2)
                .Where(group => group.Any(card => context.Catalog.Get(card.DefinitionId).BelongsTo(0x8))).ToArray();
        static IEnumerable<DuelCardState> Targets(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.ExtraDeck
            && context.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Fusion
            && context.Engine.CanSpecialSummonConditions(card, context.Player, ignore: true) && Groups(context, card).Count > 0);
        public bool CanActivate(EffectContext context) => Targets(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var targets = Targets(context).ToArray(); link.Step = 1;
                if (targets.Length == 0) { link.Step = 6; return; }
                context.SelectCards(link, targets, "选择至爱接触的融合怪兽"); return;
            }
            if (link.Step == 1)
            {
                var target = context.Card(link.Selected[0]); link.Values["summon-card"] = target.InstanceId;
                var rules = context.Engine.Rules.Get(target.DefinitionId);
                var candidates = Groups(context, target).SelectMany(group => group).Distinct().ToArray(); link.Step = 2;
                context.SelectCards(link, candidates, "选择回到卡组的素材", rules.MinSummonMaterials, System.Math.Min(rules.MaxSummonMaterials, candidates.Length));
                context.State.PendingDecision.MaterialRecipeId = target.DefinitionId; return;
            }
            if (link.Step == 2)
            {
                link.Step = 3;
                context.OpenDecision(link, DecisionKind.OrderCards, link.Selected.Select(id => {
                    var card = context.Card(id); return new DecisionOption { Id = id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        HasCard = true, Card = card.Ref, Label = context.Catalog.Get(card.DefinitionId).Name }; }),
                    "选择卡组底的素材顺序", link.Selected.Count, link.Selected.Count); return;
            }
            if (link.Step == 3)
            {
                bool neosReturned = false;
                bool allReturned = true;
                foreach (int id in link.Selected)
                {
                    var card = context.Card(id); bool neos = context.Catalog.Get(card.DefinitionId).OriginalNameId == "89943723";
                    context.Move(card, DuelZone.Deck, CardPosition.FaceDown);
                    if (neos && card.Zone == DuelZone.Deck) neosReturned = true;
                    if (card.Zone != (context.Catalog.Get(card.DefinitionId).IsExtra ? DuelZone.ExtraDeck : DuelZone.Deck)) allReturned = false;
                }
                if (!allReturned) { link.Step = 6; return; }
                link.Values["neos-returned"] = neosReturned ? 1 : 0;
            }
            if (link.Step >= 3 && link.Step <= 5)
            {
                var target = context.Card(link.Values["summon-card"]);
                if (EffectSummonFlow.Resume(context, link, target, 3, ignore: true) && link.Step == 6
                    && (target.Zone == DuelZone.Monster || target.Zone == DuelZone.ExtraMonster) && link.Values["neos-returned"] == 1)
                    context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotReturnToExtra, Target = target.Ref });
            }
        }
    }
}
