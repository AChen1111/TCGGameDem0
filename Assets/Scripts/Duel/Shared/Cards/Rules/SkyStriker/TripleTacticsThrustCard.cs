using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TripleTacticsThrustCard : CardRules
    {
        public override string CardId => "35269904";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("35269904.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("35269904.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new TripleTacticsThrustAbility(); }
    }

    sealed class TripleTacticsThrustAbility : IAbilityHandler, IActivationUsageLimit, IEffectCategoryProvider
    {
        public string CardId => "35269904";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public string UsageKey => CardId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public EffectCategories Categories => EffectCategories.AddFromDeckToHand;
        static IEnumerable<DuelCardState> Candidates(EffectContext context) => context.Deck.Where(card =>
            card.DefinitionId != "35269904" && context.Catalog.Get(card.DefinitionId).Kind != RuleCardKind.Monster
            && context.Catalog.Get(card.DefinitionId).SpellTrapType == RuleSpellTrapType.Normal);
        static IEnumerable<int> FreeSlots(EffectContext context) => Enumerable.Range(0, 5).Where(slot =>
            !context.State.Cards.Any(card => card.Controller == context.Player && card.Zone == DuelZone.SpellTrap && card.Slot == slot));
        static bool CanAdd(EffectContext context) => !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player)
            && context.State.Cards.Any(card => card.Controller == 1 - context.Player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
        public bool CanActivate(EffectContext context) => TacticsConditions.OpponentActivatedMonster(context, false) && Candidates(context).Any()
            && (CanAdd(context) || FreeSlots(context).Count() > (context.Source.Zone == DuelZone.Hand ? 1 : 0));
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = Candidates(context).ToArray(); link.Step = 1;
                if (candidates.Length == 0) { link.Step = 4; return; }
                context.SelectCards(link, candidates, "选择通常魔法或通常陷阱"); return;
            }
            if (link.Step == 1)
            {
                link.Values["chosen-card"] = link.Selected[0];
                context.Reveal(context.Card(link.Selected[0])); link.Step = 2;
                var modes = new List<DecisionOption>();
                if (FreeSlots(context).Any()) modes.Add(new DecisionOption { Id = "set", Value = "set", Label = "盖放" });
                if (CanAdd(context)) modes.Add(new DecisionOption { Id = "hand", Value = "hand", Label = "加入手卡" });
                if (modes.Count == 0) { link.Step = 4; return; }
                context.OpenDecision(link, DecisionKind.ChooseMode, modes, "选择盖放或加入手卡"); return;
            }
            if (link.Step == 2)
            {
                var card = context.Card(link.Values["chosen-card"]);
                if (link.Answers[0] == "hand") { context.Move(card, DuelZone.Hand); context.Reveal(card); context.ShuffleDeck(); link.Step = 4; return; }
                link.Step = 3;
                context.OpenDecision(link, DecisionKind.ChooseZone, FreeSlots(context).Select(slot => new DecisionOption { DestinationZone = DuelZone.SpellTrap,
                    Id = slot.ToString(), Value = slot.ToString(), Label = "区域 " + slot }), "选择盖放区域"); return;
            }
            if (link.Step == 3)
            {
                var card = context.Card(link.Values["chosen-card"]);
                if (context.Engine.TryMove(card, DuelZone.SpellTrap, context.Player, int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture),
                    CardPosition.FaceDown, MoveCause.Effect, context.SourceRef, context.Player))
                {
                    card.SetTurn = context.State.Turn;
                    context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotActivateCard, Target = card.Ref,
                        Player = context.Player, ExpiresTurn = context.State.Turn });
                }
                context.ShuffleDeck(); link.Step = 4;
            }
        }
    }
}
