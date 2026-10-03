using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class TripleTacticsTalentCard : CardRules
    {
        public override string CardId => "25311006";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("25311006.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("25311006.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new TripleTacticsTalentAbility(); }
    }

    sealed class TripleTacticsTalentAbility : IAbilityHandler, IActivationUsageLimit, IActivationModeSelection
    {
        public string CardId => "25311006";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public string UsageKey => CardId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public IEnumerable<AbilityMode> Modes(EffectContext context)
        {
            if (context.State.Players[context.Player].Deck.Count >= 2 && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player))
                yield return new AbilityMode { Id = "draw", Label = "抽两张卡", Categories = EffectCategories.AddFromDeckToHand };
            if (MonsterChoices(context).Any() && FreeSlots(context).Any()) yield return new AbilityMode { Id = "control", Label = "获得控制权" };
            if (Hand(context).Any()) yield return new AbilityMode { Id = "hand", Label = "确认手卡并返回一张" };
        }
        public bool CanActivate(EffectContext context) => TacticsConditions.OpponentActivatedMonster(context, true) && Modes(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        static IEnumerable<DuelCardState> Hand(EffectContext context) => context.State.Cards.Where(card => card.Owner == 1 - context.Player && card.Zone == DuelZone.Hand);
        static IEnumerable<DuelCardState> MonsterChoices(EffectContext context) => context.State.Cards.Where(card =>
            card.Controller == 1 - context.Player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster));
        static IEnumerable<int> FreeSlots(EffectContext context) => Enumerable.Range(0, 5).Where(slot =>
            !context.State.Cards.Any(card => card.Controller == context.Player && card.Zone == DuelZone.Monster && card.Slot == slot));
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.ModeId == "draw") { if (link.Step == 0) { context.Engine.Draw(context.Player, 2); link.Step = 1; } return; }
            if (link.ModeId == "control")
            {
                if (link.Step == 0)
                {
                    var choices = MonsterChoices(context).ToArray(); link.Step = 1;
                    if (choices.Length == 0 || !FreeSlots(context).Any()) { link.Step = 3; return; }
                    context.SelectCards(link, choices, "选择获得控制权的怪兽"); return;
                }
                if (link.Step == 1)
                {
                    link.Values["controlled-card"] = link.Selected[0]; link.Step = 2;
                    context.OpenDecision(link, DecisionKind.ChooseZone, FreeSlots(context).Select(slot => new DecisionOption {
                        Id = slot.ToString(), Value = slot.ToString(), Label = "区域 " + slot }), "选择控制权转移区域"); return;
                }
                if (link.Step == 2)
                {
                    var card = context.Card(link.Values["controlled-card"]); int original = card.Controller;
                    if (context.IsAffected(card) && context.Engine.TryMove(card, DuelZone.Monster, context.Player,
                        int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture), card.Position, MoveCause.Effect, context.SourceRef, context.Player))
                        context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.ReturnControl, Target = card.Ref,
                            Player = original, ExpiresTurn = context.State.Turn });
                    link.Step = 3;
                }
            }
            else if (link.ModeId == "hand")
            {
                if (link.Step == 0)
                {
                    var hand = Hand(context).ToArray(); link.Step = 1;
                    if (hand.Length == 0) { link.Step = 2; return; }
                    foreach (var card in hand)
                    { link.Values["mask:" + card.InstanceId] = card.RevealedToMask; card.RevealedToMask |= 1 << context.Player; context.Reveal(card); }
                    context.SelectCards(link, hand, "选择返回卡组的对方手卡"); return;
                }
                if (link.Step == 1)
                {
                    var chosen = context.Card(link.Selected[0]); context.Move(chosen, DuelZone.Deck, CardPosition.FaceDown);
                    context.Engine.Shuffle(context.State.Players[1 - context.Player].Deck);
                    context.Engine.Emit(DuelEventKind.Shuffled, 1 - context.Player);
                    foreach (var card in Hand(context)) if (link.Values.TryGetValue("mask:" + card.InstanceId, out int mask)) card.RevealedToMask = mask;
                    link.Step = 2;
                }
            }
        }
    }
}
