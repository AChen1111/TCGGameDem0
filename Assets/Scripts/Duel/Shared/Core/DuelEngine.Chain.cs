using System;
using System.Linq;
using System.Collections.Generic;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        bool ActivationWindow(IAbilityHandler ability, DuelCardState source, int player)
        {
            var definition = m_catalog.Get(source.DefinitionId);
            // 同一魔陷实体的本次卡发动尚在连锁中，不能再次发动同一个发动效果。
            // 按来源世代与效果匹配，保留不同副本及连续魔陷其他效果的合法连锁。
            if (definition.Kind != RuleCardKind.Monster && State.Chain.Any(link => link.IsCardActivation
                && link.Source.Equals(source.Ref) && link.AbilityId == ability.AbilityId)) return false;
            var context = new EffectContext(this, player, source.InstanceId);
            bool queuedTrigger = State.ActiveTrigger != null && State.ActiveTrigger.Source.Equals(source.Ref)
                && State.ActiveTrigger.AbilityId == ability.AbilityId
                || State.PendingTriggers.Any(t => !t.Public && t.Source.Equals(source.Ref) && t.Player == player && t.AbilityId == ability.AbilityId);
            bool phaseAbility = State.Chain.Count == 0 && ability is IPhaseAbility phase && phase.AllowsPhase(context);
            if (ability is ITriggeredAbility && !queuedTrigger && !phaseAbility) return false;
            if (source.Controller != player || State.Window == TimingWindow.Resolving || State.Window == TimingWindow.Decision) return false;
            if (OnField(source) && HasEffect(EffectRecordKind.CannotActivateCard, player, source)) return false;
            if (State.Chain.Count > 0)
            {
                var top = State.Chain[State.Chain.Count - 1];
                int mask = 1 << ((int)definition.Kind - 1);
                if (ApplicableEffects().Any(e => e.Kind == EffectRecordKind.PreventResponses && e.ChainId == State.CurrentChainId
                    && e.ResponseToLink == top.Number && (e.Player < 0 || e.Player == player) && (e.Value & mask) != 0)) return false;
                if (top.IsCardActivation && top.ActivationKind == RuleCardKind.Spell
                    && HasEffect(EffectRecordKind.SpellResponsesBlocked, player)) return false;
            }
            if (!queuedTrigger && State.BattleStep != BattleStep.None && State.BattleStep != BattleStep.Declaration
                && !(ability is IDamageStepAbility damage && damage.AllowsDamageStep(State.BattleStep))) return false;
            if (ability.Speed == 1 && !queuedTrigger && !phaseAbility && !MainOpen(player)) return false;
            if (State.Chain.Count > 0 && !queuedTrigger && (ability.Speed < 2 || ability.Speed < State.Chain[State.Chain.Count - 1].Speed)) return false;
            if (queuedTrigger && State.PrivateTriggerPlayers.Contains(player)
                && State.PendingTriggers.Any(t => !t.Public && t.Source.Equals(source.Ref) && t.AbilityId == ability.AbilityId)) return false;
            if (ability is IActivationUsageLimit usage && State.UsedAbilities.TryGetValue(UsageKey(player, usage, source.Ref), out int used)
                && used >= usage.Limit) return false;
            if (ability is IActivationSourcePolicy policy)
            {
                if (!policy.AllowsSource(context)) return false;
            }
            else if (definition.Kind == RuleCardKind.Spell)
            {
                if (source.Zone != DuelZone.Hand && source.Zone != DuelZone.SpellTrap && source.Zone != DuelZone.Field) return false;
                if (definition.SpellTrapType == RuleSpellTrapType.QuickPlay && (source.Zone == DuelZone.Hand
                    ? State.TurnPlayer != player : source.SetTurn == State.Turn)) return false;
                if (definition.SpellTrapType != RuleSpellTrapType.QuickPlay && !MainOpen(player)) return false;
                if (source.Zone == DuelZone.Hand && !Enumerable.Range(0, 5).Any(slot => FreeSpellSlot(player, slot))) return false;
            }
            else if (definition.Kind == RuleCardKind.Trap && (source.Zone != DuelZone.SpellTrap || source.SetTurn >= State.Turn)) return false;
            if (definition.Kind == RuleCardKind.Spell && (source.Zone == DuelZone.Hand
                || source.Zone == DuelZone.SpellTrap && source.Position == CardPosition.FaceDown
                || source.Zone == DuelZone.Field && source.Position == CardPosition.FaceDown))
            {
                if (definition.SpellTrapType == RuleSpellTrapType.QuickPlay)
                { if (source.Zone == DuelZone.Hand ? State.TurnPlayer != player : source.SetTurn >= State.Turn) return false; }
                else if (!queuedTrigger && !MainOpen(player)) return false;
                if (source.Zone == DuelZone.Hand && definition.SpellTrapType != RuleSpellTrapType.Field
                    && !Enumerable.Range(0, 5).Any(slot => FreeSpellSlot(player, slot))) return false;
            }
            if (definition.Kind == RuleCardKind.Trap && source.Zone == DuelZone.SpellTrap
                && source.Position == CardPosition.FaceDown && source.SetTurn >= State.Turn) return false;
            if (OnField(source) && HasEffect(EffectRecordKind.CannotActivateName, player, source)
                && ApplicableEffects().Any(e => e.Kind == EffectRecordKind.CannotActivateName && e.NameId == definition.OriginalNameId)) return false;
            return ability.CanActivate(context);
        }

        internal bool FreeSpellSlot(int player, int slot) => slot >= 0 && slot < 5 && !State.Cards.Any(x => x.Controller == player
            && x.Zone == DuelZone.SpellTrap && x.Slot == slot);

        string ValidateActivation(DuelCommand command)
        {
            var source = State.Cards.FirstOrDefault(x => x.InstanceId == command.CardId);
            if (source == null || !m_abilities.TryGet(command.AbilityId, out var ability)
                || ability.CardId != source.DefinitionId || !ActivationWindow(ability, source, command.Player)) return "EFFECT_NOT_AVAILABLE";
            if (m_catalog.Get(source.DefinitionId).Kind == RuleCardKind.Spell && m_catalog.Get(source.DefinitionId).SpellTrapType != RuleSpellTrapType.Field && source.Zone == DuelZone.Hand
                && !FreeSpellSlot(command.Player, command.Slot)) return "ZONE_OCCUPIED";
            if (!(ability is IActivationModeSelection) && ability is IActivationTargetSelection targeting && (!(ability is IActivationTargetRequirement requirement) || requirement.RequiresTarget)
                && !targeting.TargetCandidates(new EffectContext(this, command.Player, source.InstanceId)).Any(c => c.InstanceId == command.TargetId))
                return "INVALID_EFFECT_TARGET";
            if (ability is IActivationModeSelection modes)
            {
                if (command.Options.Length != 1) return "ACTIVATION_MODE_REQUIRED";
                var mode = modes.Modes(new EffectContext(this, command.Player, source.InstanceId)).FirstOrDefault(m => m.Id == command.Options[0]);
                if (mode == null || mode.HasTarget && !mode.Targets.Any(t => t.InstanceId == command.TargetId)
                    || !mode.HasTarget && command.TargetId != 0) return "INVALID_ACTIVATION_MODE";
            }
            return ability.ValidateActivation(new EffectContext(this, command.Player, source.InstanceId), command);
        }

        IEnumerable<DuelAction> AbilityActions(int player)
        {
            foreach (var card in State.Cards.Where(x => x.Controller == player))
            foreach (var ability in m_abilities.Handlers.Where(x => x.CardId == card.DefinitionId))
            {
                if (!ActivationWindow(ability, card, player)) continue;
                var context = new EffectContext(this, player, card.InstanceId);
                var action = new DuelAction { Id = "activate:" + card.InstanceId + ":" + ability.AbilityId,
                    Kind = DuelCommandKind.Activate, Card = card.Ref, AbilityId = ability.AbilityId,
                    Slots = card.Zone == DuelZone.Hand && m_catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell
                        ? Enumerable.Range(0, 5).Where(slot => FreeSpellSlot(player, slot)).ToList() : new List<int>() };
                if (ability is IActivationCostSelection cost)
                { action.SelectionCards = cost.CostCandidates(context).Select(c => c.Ref).ToList();
                    action.MinSelections = cost.MinCosts; action.MaxSelections = cost.MaxCosts; }
                action.SelectionIsTarget = ability is IActivationSelectedTargets;
                if (ability is IActivationTargetSelection targeting) action.Targets = targeting.TargetCandidates(context).Select(c => c.Ref).ToList();
                if (ability is IActivationNameDeclaration declaration)
                { action.RequiresNameDeclaration = true; action.DeclarationKind = declaration.DeclaredKind; }
                if (ability is IActivationModeSelection modes)
                {
                    foreach (var mode in modes.Modes(context))
                        yield return new DuelAction { Id = action.Id + ":" + mode.Id, Kind = action.Kind, Card = action.Card,
                            AbilityId = action.AbilityId, Slots = action.Slots, Positions = action.Positions,
                            SelectionCards = action.SelectionCards, MinSelections = action.MinSelections, MaxSelections = action.MaxSelections,
                            SelectionIsTarget = action.SelectionIsTarget,
                            Targets = mode.Targets, ActivationOptions = new[] { mode.Id }, Label = mode.Label };
                }
                else yield return action;
            }
        }

        void Activate(DuelCommand command)
        {
            if (State.Chain.Count == 0) State.CurrentChainId = State.NextChainId++;
            var source = Card(command.CardId);
            var definition = m_catalog.Get(source.DefinitionId);
            var ability = m_abilities.Get(command.AbilityId);
            bool cardActivation = definition.Kind != RuleCardKind.Monster && (source.Zone == DuelZone.Hand
                || OnField(source) && source.Position == CardPosition.FaceDown);
            if (definition.Kind == RuleCardKind.Spell && source.Zone == DuelZone.Hand)
            {
                if (definition.SpellTrapType == RuleSpellTrapType.Field)
                {
                    var previous = State.Cards.FirstOrDefault(c => c.Zone == DuelZone.Field && c.Controller == command.Player);
                    if (previous != null) Move(previous, DuelZone.Graveyard, previous.Owner);
                    Move(source, DuelZone.Field, command.Player);
                }
                else Move(source, DuelZone.SpellTrap, command.Player, command.Slot);
            }
            else if (OnField(source) && (source.Position == CardPosition.FaceDown || source.Position == CardPosition.FaceDownDefense))
            { source.Position = CardPosition.FaceUp; Emit(DuelEventKind.Revealed, command.Player, source); }
            // 手牌中的发动来源已经公开，双方投影都需要该卡的句柄来显示连锁。
            // 不公开其他手牌；来源离开当前区域时 Move 会清除公开标记。
            if (source.Zone == DuelZone.Hand && (source.RevealedToMask & 3) != 3)
            { source.RevealedToMask |= 3; Emit(DuelEventKind.Revealed, command.Player, source); }
            var link = new DuelChainLink { Number = State.Chain.Count + 1, Player = command.Player,
                Source = source.Ref, DefinitionId = source.DefinitionId, AbilityId = ability.AbilityId, Speed = ability.Speed,
                ActivationKind = definition.Kind == RuleCardKind.Monster && source.Zone == DuelZone.SpellTrap ? RuleCardKind.Spell : definition.Kind,
                ActivationZone = source.Zone, IsCardActivation = cardActivation, ActivationSource = Snapshot(source),
                FieldEffectNegated = (source.Zone == DuelZone.Monster || source.Zone == DuelZone.ExtraMonster)
                    && source.Negated && State.Effects.Any(e => e.Kind == EffectRecordKind.TargetNegate && e.Target.Equals(source.Ref) && !e.RequiresSource) };
            link.Categories = ability is IEffectCategoryProvider categories ? categories.Categories : EffectCategories.None;
            if (ability is IActivationModeSelection modes)
            {
                var mode = modes.Modes(new EffectContext(this, command.Player, source.InstanceId)).Single(m => m.Id == command.Options[0]);
                link.ModeId = mode.Id; link.Categories = mode.Categories; link.StringValues["mode"] = mode.Id;
            }
            var trigger = State.ActiveTrigger ?? State.PendingTriggers.FirstOrDefault(t => !t.Public && t.Source.Equals(source.Ref)
                && t.AbilityId == ability.AbilityId && t.Player == command.Player);
            if (trigger != null)
            {
                link.TriggerEvent = State.LastCheckpointEvents.First(f => f.Id == trigger.EventId);
                State.PendingTriggers.Remove(trigger);
                if (!trigger.Public) State.PrivateTriggerPlayers.Add(command.Player);
            }
            if (command.TargetId != 0) link.Targets.Add(Card(command.TargetId).Ref);
            ability.PayCost(new EffectContext(this, command.Player, source.InstanceId), command, link);
            if (ability is IActivationUsageLimit usage)
            {
                string key = UsageKey(command.Player, usage, link.Source);
                State.UsedAbilities.TryGetValue(key, out int used); State.UsedAbilities[key] = used + 1;
            }
            State.Chain.Add(link); State.Window = TimingWindow.ChainResponse;
            State.WaitingSeat = 1 - command.Player; State.ConsecutivePasses = 0; State.PendingPhase = -1;
            Emit(DuelEventKind.Activated, command.Player, source, detail: ability.AbilityId);
            var activated = m_events[m_events.Count - 1];
            activated.LinkNumber = link.Number; activated.ActivationKind = link.ActivationKind;
            activated.IsCardActivation = link.IsCardActivation;
            if (ability is IActivationNameDeclaration) m_events[m_events.Count - 1].DeclaredNameId = command.NameId;
        }

        void ResolveChain()
        {
            State.Window = TimingWindow.Resolving;
            while (State.Chain.Count > 0 && !State.Finished)
            {
                var link = State.Chain[State.Chain.Count - 1];
                if (link.ResolutionGroupId == 0) link.ResolutionGroupId = State.NextEventGroupId++;
                State.CurrentEventGroupId = link.ResolutionGroupId;
                if (link.ActivationNegated)
                    State.Effects.RemoveAll(e => e.RemoveIfActivationNegated && e.ChainId == State.CurrentChainId && e.ResponseToLink == link.Number);
                bool effectNegated = IsLinkNegated(link);
                if (!link.ActivationNegated && !effectNegated && PersistentSourcePresent(link))
                {
                    var context = new EffectContext(this, link.Player, link.Source.InstanceId, link.Source, link.TriggerEvent);
                    if (link.ReplacementCardId.Length != 0)
                        m_rules.Get(link.ReplacementCardId).ResolveReplacement(context, link, link.ReplacementProgram);
                    else m_abilities.Get(link.AbilityId).Resolve(context, link);
                    if (State.PendingDecision != null) return;
                }
                RecoverAfterSpellResolution(link);
                AdvanceLingeringEffects(State.PendingFacts.ToArray());
                var source = Card(link.Source.InstanceId);
                Emit(link.ActivationNegated || effectNegated ? DuelEventKind.Negated : DuelEventKind.Resolved,
                    link.Player, source, detail: link.AbilityId);
                var resolved = m_events[m_events.Count - 1];
                resolved.LinkNumber = link.Number; resolved.ActivationKind = link.ActivationKind;
                resolved.ActivationNegated = link.ActivationNegated;
                resolved.IsCardActivation = link.IsCardActivation;
                var definition = m_catalog.Get(link.DefinitionId);
                if (link.ActivationNegated && m_abilities.Get(link.AbilityId) is IActivationUsageLimit usage && !usage.CountNegatedActivation)
                    State.UsedAbilities[UsageKey(link.Player, usage, link.Source)]--;
                if (link.ActivationNegated && link.IsCardActivation && source.Ref.Equals(link.Source) && OnField(source))
                    Move(source, DuelZone.Graveyard, source.Owner, cause: MoveCause.ActivationNegated);
                else if (source.Ref.Equals(link.Source) && source.Zone == DuelZone.SpellTrap
                    && definition.Kind != RuleCardKind.Monster && definition.SpellTrapType != RuleSpellTrapType.Continuous
                    && definition.SpellTrapType != RuleSpellTrapType.Equip)
                {
                    if (link.ActivationNegated) Move(source, DuelZone.Graveyard, source.Owner, cause: MoveCause.ActivationNegated);
                    else State.ChainCleanup.Add(source.Ref);
                }
                State.Chain.RemoveAt(State.Chain.Count - 1);
            }
            if (!State.Finished)
            {
                foreach (var reference in State.ChainCleanup)
                {
                    var card = State.Cards.FirstOrDefault(c => c.Ref.Equals(reference) && c.Zone == DuelZone.SpellTrap);
                    if (card != null) Move(card, DuelZone.Graveyard, card.Owner);
                }
                State.ChainCleanup.Clear();
                State.PrivateTriggerPlayers.Clear();
                OpenResponse();
            }
        }

        void RecoverAfterSpellResolution(DuelChainLink link)
        {
            var definition = m_catalog.Get(link.DefinitionId);
            if (State.Finished || link.ActivationNegated || definition.Kind != RuleCardKind.Spell) return;
            // 回复不入连锁，在该效果及全部选择结束后检查当前仍有效的永续来源。
            RefreshCharacteristics();
            foreach (var record in ApplicableEffects().Where(e => e.Kind == EffectRecordKind.RecoverOnSpellActivated
                && e.Player == link.Player && definition.BelongsTo(e.SetCode)).ToArray())
            {
                State.Players[record.Player].LifePoints += record.Value;
                Emit(DuelEventKind.Recovered, record.Player, amount: record.Value, cause: MoveCause.Effect,
                    effectSource: record.Source, effectPlayer: record.Player);
            }
        }

        bool PersistentSourcePresent(DuelChainLink link)
        {
            var definition = m_catalog.Get(link.DefinitionId);
            bool persistent = definition.Kind != RuleCardKind.Monster
                && (definition.SpellTrapType == RuleSpellTrapType.Continuous || definition.SpellTrapType == RuleSpellTrapType.Equip
                    || definition.SpellTrapType == RuleSpellTrapType.Field)
                && (link.ActivationZone == DuelZone.SpellTrap || link.ActivationZone == DuelZone.Field);
            return !persistent || State.Cards.Any(c => c.Ref.Equals(link.Source) && OnField(c) && IsPublic(c));
        }

        static string UsageKey(int player, IActivationUsageLimit usage, CardRef source) => player + ":" + usage.UsageKey
            + (usage is IActivationInstanceUsageLimit ? ":" + source : "");

        string ValidateAnswer(DuelCommand command)
        {
            var decision = State.PendingDecision;
            if (command.DecisionId != decision.Id || command.Player != decision.Player) return "STALE_DECISION";
            if (command.Options.Length < decision.Min || command.Options.Length > decision.Max
                || command.Options.Distinct(StringComparer.Ordinal).Count() != command.Options.Length
                || command.Options.Any(id => !decision.Options.Any(option => option.Id == id))) return "INVALID_DECISION_OPTIONS";
            if (decision.AllowedCardGroups.Count > 0 && decision.Kind == DecisionKind.ChooseCards)
            {
                var chosen = command.Options.Select(id => decision.Options.First(o => o.Id == id).Card).ToArray();
                if (!decision.AllowedCardGroups.Any(group => group.Count == chosen.Length && chosen.All(group.Contains))) return "INVALID_CARD_COMBINATION";
            }
            if (decision.MaterialRecipeId.Length != 0)
            {
                var definition = m_catalog.Get(decision.MaterialRecipeId);
                var rules = Rules.Get(decision.MaterialRecipeId);
                var selected = command.Options.Select(id => Card(decision.Options.First(o => o.Id == id).Card.InstanceId)).ToArray();
                if (!rules.MatchesSummonMaterials(definition, selected, m_catalog)
                    || selected.Any(card => !Rules.Get(card.DefinitionId).CanUseAsMaterial(definition, card, State))) return "INVALID_SUMMON_MATERIALS";
            }
            if (decision.UniqueOriginalNames)
            {
                var names = command.Options.Select(id => m_catalog.Get(Card(decision.Options.First(o => o.Id == id).Card.InstanceId).DefinitionId).OriginalNameId).ToArray();
                if (names.Distinct(StringComparer.Ordinal).Count() != names.Length) return "DUPLICATE_CARD_NAME";
            }
            if (decision.RequireSetCapacity)
            {
                var definitions = command.Options.Select(id => m_catalog.Get(Card(decision.Options.First(o => o.Id == id).Card.InstanceId).DefinitionId)).ToArray();
                if (definitions.Count(c => c.SpellTrapType == RuleSpellTrapType.Field) > 1
                    || definitions.Count(c => c.SpellTrapType != RuleSpellTrapType.Field) > Enumerable.Range(0, 5).Count(slot => FreeSpellSlot(decision.Player, slot)))
                    return "INSUFFICIENT_SET_ZONES";
            }
            if (decision.Continuation == "trigger.cost" && m_abilities.Get(State.ActiveTrigger.AbilityId) is IActivationSelectedTargets)
            {
                var trigger = State.ActiveTrigger;
                var selected = command.Options.Select(id => decision.Options.First(o => o.Id == id).Card.InstanceId).ToArray();
                var activation = new DuelCommand { Kind = DuelCommandKind.Activate, Player = trigger.Player,
                    CardId = trigger.Source.InstanceId, AbilityId = trigger.AbilityId, Cards = selected };
                string error = m_abilities.Get(trigger.AbilityId).ValidateActivation(
                    new EffectContext(this, trigger.Player, trigger.Source.InstanceId, trigger.Source), activation);
                if (error.Length != 0) return error;
            }
            return "";
        }

        void Answer(DuelCommand command)
        {
            var decision = State.PendingDecision;
            if (decision.Continuation == "destruction.replace") { AnswerDestruction(command); return; }
            if (decision.Continuation.StartsWith("obligation.", StringComparison.Ordinal)) { ResolveObligationDecision(command); return; }
            if (decision.Continuation.StartsWith("trigger.", StringComparison.Ordinal)) { AnswerTrigger(command); return; }
            if (decision.Continuation == "battle.replay") { AnswerBattleReplay(command); return; }
            if (decision.Continuation == "end.discard")
            {
                foreach (var option in command.Options.Select(id => decision.Options.First(x => x.Id == id)))
                { var card = Card(option.Card.InstanceId); Move(card, DuelZone.Graveyard, card.Owner, cause: MoveCause.Discard); }
                State.PendingDecision = null;
                State.ContinuationAfterTriggers = "end.turn";
                OpenResponse(); return;
            }
            var link = State.Chain[State.Chain.Count - 1];
            var choices = command.Options.Select(id => decision.Options.First(option => option.Id == id)).ToArray();
            link.AnswerKind = decision.Kind;
            link.Answers = choices.Select(option => option.Value).ToList();
            link.Selected = choices.Where(option => option.HasCard).Select(option => option.Card.InstanceId).ToList();
            State.PendingDecision = null;
            ResolveChain();
        }
    }
}
