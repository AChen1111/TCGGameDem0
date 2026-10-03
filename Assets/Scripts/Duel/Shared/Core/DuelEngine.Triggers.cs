using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        void ProcessCheckpoints()
        {
            if (State.Finished || State.Window == TimingWindow.Resolving || State.Window == TimingWindow.ChainResponse
                || State.PendingDecision != null || State.ActiveTrigger != null) return;
            if (State.PendingFacts.Count == 0) { ContinueAfterTriggers(); return; }
            AdvanceLingeringEffects(State.PendingFacts.ToArray());
            var facts = State.PendingFacts.ToArray();
            State.PendingFacts.Clear();
            State.LastCheckpointEvents = facts.ToList();
            State.PendingTriggers.RemoveAll(t => !t.Public);
            long lastGroup = facts.Where(f => f.Kind != DuelEventKind.DecisionOpened
                && !(f.Kind == DuelEventKind.Moved && f.Cause == MoveCause.Rule)).Select(f => f.GroupId).LastOrDefault();
            foreach (var card in State.Cards.OrderBy(c => c.InstanceId))
            foreach (var ability in m_abilities.Handlers.Where(a => a.CardId == card.DefinitionId && a is ITriggeredAbility))
            {
                var triggerRule = (ITriggeredAbility)ability;
                foreach (var fact in facts)
                {
                    var atEvent = fact.PresentCards.FirstOrDefault(c => c.Ref.Equals(card.Ref));
                    if (atEvent == null) continue;
                    var context = new EffectContext(this, card.Controller, card.InstanceId, card.Ref, fact);
                    if (!triggerRule.IsTriggered(context, fact) || !ability.CanActivate(context)) continue;
                    if (triggerRule.OptionalWhen && fact.GroupId != lastGroup) continue;
                    if (State.PendingTriggers.Any(t => t.Source.Equals(card.Ref) && t.AbilityId == ability.AbilityId && t.GroupId == fact.GroupId)) continue;
                    State.PendingTriggers.Add(new PendingTrigger { Source = card.Ref, Player = card.Controller,
                        AbilityId = ability.AbilityId, Mandatory = triggerRule.Mandatory, OptionalWhen = triggerRule.OptionalWhen,
                        EventId = fact.Id, GroupId = fact.GroupId, Public = (atEvent.VisibleToMask & 3) == 3 });
                }
            }
            BuildTriggerChain();
        }

        int TriggerPriority(PendingTrigger trigger) => (trigger.Mandatory ? 0 : 2) + (trigger.Player == State.TurnPlayer ? 0 : 1);

        bool StillCanTrigger(PendingTrigger trigger)
        {
            var source = State.Cards.FirstOrDefault(c => c.Ref.Equals(trigger.Source));
            if (source == null || !m_abilities.TryGet(trigger.AbilityId, out var ability)) return false;
            var fact = State.LastCheckpointEvents.FirstOrDefault(f => f.Id == trigger.EventId);
            if (fact == null || !ability.CanActivate(new EffectContext(this, trigger.Player, source.InstanceId, trigger.Source, fact))) return false;
            return !(ability is IActivationUsageLimit usage && State.UsedAbilities.TryGetValue(UsageKey(trigger.Player, usage, trigger.Source), out int used) && used >= usage.Limit);
        }

        void BuildTriggerChain()
        {
            State.PendingTriggers.RemoveAll(t => !StillCanTrigger(t));
            while (State.PendingTriggers.Any(t => t.Public))
            {
                int priority = State.PendingTriggers.Where(t => t.Public).Min(TriggerPriority);
                var group = State.PendingTriggers.Where(t => t.Public && TriggerPriority(t) == priority).ToArray();
                var first = group[0];
                if (group.Length == 1 && first.Mandatory) { BeginTrigger(first); if (State.PendingDecision != null) return; continue; }
                if (group.Length == 1)
                {
                    State.ActiveTrigger = first;
                    SetTriggerDecision(first, "trigger.offer", DecisionKind.YesNo, new[] {
                        new DecisionOption { Id = "yes", Value = "yes", Label = "发动" },
                        new DecisionOption { Id = "no", Value = "no", Label = "不发动" } });
                }
                else
                {
                    var options = group.Select(t => new DecisionOption { Id = TriggerKey(t), Value = TriggerKey(t),
                        Label = m_catalog.Get(Card(t.Source.InstanceId).DefinitionId).Name + " " + t.AbilityId }).ToList();
                    if (!first.Mandatory) options.Add(new DecisionOption { Id = "skip", Value = "skip", Label = "不发动这些效果" });
                    SetTriggerDecision(first, "trigger.order", DecisionKind.ChooseMode, options);
                }
                return;
            }
            State.ActiveTrigger = null;
            if (State.Chain.Count > 0)
            {
                State.Window = TimingWindow.ChainResponse;
                State.WaitingSeat = 1 - State.Chain[State.Chain.Count - 1].Player;
                State.ConsecutivePasses = 0;
            }
            else
            {
                if (State.Window == TimingWindow.Decision) OpenResponse();
                ContinueAfterTriggers();
            }
        }

        static string TriggerKey(PendingTrigger trigger) => trigger.Source + ":" + trigger.AbilityId + ":" + trigger.EventId;

        void SetTriggerDecision(PendingTrigger trigger, string continuation, DecisionKind kind, IEnumerable<DecisionOption> options, int min = 1, int max = 1)
        {
            State.PendingDecision = new DuelDecision { Id = State.NextDecisionId++, Player = trigger.Player, Kind = kind,
                Prompt = "处理诱发效果", Min = min, Max = max, SourceId = trigger.Source.InstanceId,
                Continuation = continuation, Options = options.ToList() };
            State.Window = TimingWindow.Decision; State.WaitingSeat = trigger.Player;
            Emit(DuelEventKind.DecisionOpened, trigger.Player, mask: 1 << trigger.Player);
        }

        void BeginTrigger(PendingTrigger trigger)
        {
            State.ActiveTrigger = trigger;
            State.TriggerTargetId = 0; State.TriggerCostIds.Clear(); State.TriggerModeId = "";
            var ability = m_abilities.Get(trigger.AbilityId);
            if (ability is IActivationModeSelection modes)
            {
                SetTriggerDecision(trigger, "trigger.mode", DecisionKind.ChooseMode,
                    modes.Modes(new EffectContext(this, trigger.Player, trigger.Source.InstanceId)).Select(m =>
                        new DecisionOption { Id = m.Id, Value = m.Id, Label = m.Label }));
                return;
            }
            PrepareTriggerCosts(trigger);
        }

        void PrepareTriggerCosts(PendingTrigger trigger)
        {
            var ability = m_abilities.Get(trigger.AbilityId);
            var context = new EffectContext(this, trigger.Player, trigger.Source.InstanceId, trigger.Source);
            if (ability is IActivationCostSelection cost && cost.MinCosts > 0)
            {
                SetTriggerDecision(trigger, "trigger.cost", DecisionKind.ChooseCards,
                    cost.CostCandidates(context).Select(c => new DecisionOption { Id = c.InstanceId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Card = c.Ref, HasCard = true, Label = m_catalog.Get(c.DefinitionId).Name }), cost.MinCosts, cost.MaxCosts);
                return;
            }
            PrepareTriggerTarget(trigger);
        }

        void PrepareTriggerTarget(PendingTrigger trigger)
        {
            var ability = m_abilities.Get(trigger.AbilityId);
            var context = new EffectContext(this, trigger.Player, trigger.Source.InstanceId, trigger.Source);
            if (ability is IActivationModeSelection modes)
            {
                var mode = modes.Modes(context).Single(m => m.Id == State.TriggerModeId);
                if (mode.HasTarget)
                {
                    SetTriggerDecision(trigger, "trigger.target", DecisionKind.ChooseCards, mode.Targets.Select(reference =>
                        new DecisionOption { Id = reference.InstanceId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            Card = reference, HasCard = true, Label = m_catalog.Get(Card(reference.InstanceId).DefinitionId).Name }));
                    return;
                }
                CommitTrigger(trigger); return;
            }
            if (ability is IActivationTargetSelection targeting && (!(ability is IActivationTargetRequirement requirement) || requirement.RequiresTarget))
            {
                var targets = targeting.TargetCandidates(context).ToArray();
                if (targets.Length == 0) { State.PendingTriggers.Remove(trigger); State.ActiveTrigger = null; return; }
                SetTriggerDecision(trigger, "trigger.target", DecisionKind.ChooseCards,
                    targets.Select(c => new DecisionOption { Id = c.InstanceId.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        Card = c.Ref, HasCard = true, Label = m_catalog.Get(c.DefinitionId).Name }));
                return;
            }
            CommitTrigger(trigger);
        }

        void CommitTrigger(PendingTrigger trigger)
        {
            var command = new DuelCommand { Kind = DuelCommandKind.Activate, Player = trigger.Player,
                CardId = trigger.Source.InstanceId, AbilityId = trigger.AbilityId,
                TargetId = State.TriggerTargetId, Cards = State.TriggerCostIds.ToArray() };
            if (State.TriggerModeId.Length != 0) command.Options = new[] { State.TriggerModeId };
            var ability = m_abilities.Get(trigger.AbilityId);
            var context = new EffectContext(this, trigger.Player, trigger.Source.InstanceId, trigger.Source);
            if (ability.ValidateActivation(context, command).Length == 0) Activate(command);
            else State.PendingTriggers.Remove(trigger);
            State.ActiveTrigger = null; State.TriggerCostIds.Clear(); State.TriggerTargetId = 0;
        }

        void AnswerTrigger(DuelCommand command)
        {
            var decision = State.PendingDecision;
            var options = command.Options.Select(id => decision.Options.First(o => o.Id == id)).ToArray();
            State.PendingDecision = null;
            if (decision.Continuation == "trigger.order")
            {
                if (options[0].Value == "skip")
                {
                    int priority = State.PendingTriggers.Where(t => t.Public).Min(TriggerPriority);
                    State.PendingTriggers.RemoveAll(t => t.Public && TriggerPriority(t) == priority);
                }
                else BeginTrigger(State.PendingTriggers.First(t => TriggerKey(t) == options[0].Value));
            }
            else if (decision.Continuation == "trigger.offer")
            {
                var trigger = State.ActiveTrigger;
                if (options[0].Value == "yes") BeginTrigger(trigger);
                else { State.PendingTriggers.Remove(trigger); State.ActiveTrigger = null; }
            }
            else if (decision.Continuation == "trigger.cost")
            { State.TriggerCostIds = options.Select(o => o.Card.InstanceId).ToList(); PrepareTriggerTarget(State.ActiveTrigger); }
            else if (decision.Continuation == "trigger.mode")
            { State.TriggerModeId = options[0].Value; PrepareTriggerCosts(State.ActiveTrigger); }
            else if (decision.Continuation == "trigger.target")
            { State.TriggerTargetId = options[0].Card.InstanceId; CommitTrigger(State.ActiveTrigger); }
            if (State.PendingDecision == null) BuildTriggerChain();
        }

        void ContinueAfterTriggers()
        {
            if (State.ContinuationAfterTriggers == "end.turn" && State.Chain.Count == 0 && State.PendingDecision == null)
            { State.ContinuationAfterTriggers = ""; FinishEndPhase(); }
        }
    }
}
