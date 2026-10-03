using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class DuelEngine
    {
        string ValidateAttack(DuelCommand command)
        {
            if (State.Phase != DuelPhase.Battle || State.Window != TimingWindow.Open || State.TurnPlayer != command.Player)
                return "NOT_BATTLE_STEP";
            var attacker = Monsters(command.Player).FirstOrDefault(x => x.InstanceId == command.CardId);
            if (attacker == null || attacker.Position != CardPosition.FaceUpAttack
                || attacker.AttacksThisTurn >= MaximumAttacks(attacker, command.TargetId != 0)
                || HasEffect(EffectRecordKind.CannotAttack, command.Player, attacker)) return "CANNOT_ATTACK";
            var targets = Monsters(1 - command.Player);
            if (command.TargetId == 0 && DirectAttackProhibited(attacker)) return "DIRECT_ATTACK_PROHIBITED";
            if (command.TargetId == 0 ? targets.Count != 0 && !HasEffect(EffectRecordKind.DirectAttack, command.Player, attacker)
                : !targets.Any(x => x.InstanceId == command.TargetId)) return "INVALID_ATTACK_TARGET";
            if (command.TargetId != 0 && HasEffect(EffectRecordKind.EachMonsterOnce, command.Player, attacker)
                && attacker.AttackedTargetsThisTurn.Contains(Card(command.TargetId).Ref)) return "TARGET_ALREADY_ATTACKED";
            return "";
        }

        bool DirectAttackProhibited(DuelCardState card) => ApplicableEffects().Any(record =>
            record.Kind == EffectRecordKind.CannotDirectAttack && (record.Player < 0 || record.Player == card.Controller)
                && (record.Target.InstanceId == 0 || record.Target.Equals(card.Ref)) && !record.Cards.Contains(card.Ref));

        int MaximumAttacks(DuelCardState card, bool attackingMonster) => 1 + ApplicableEffects()
            .Where(e => (e.Kind == EffectRecordKind.ExtraAttacks || attackingMonster && !card.DirectAttackDeclaredThisTurn
                    && e.Kind == EffectRecordKind.ExtraMonsterAttacks)
                && (e.Player < 0 || e.Player == card.Controller) && (e.Target.InstanceId == 0 || e.Target.Equals(card.Ref)))
            .Select(e => e.Value).DefaultIfEmpty(0).Max();

        internal IEnumerable<DuelAction> AttackActions(int player)
        {
            if (State.Phase != DuelPhase.Battle || State.Window != TimingWindow.Open || State.TurnPlayer != player) yield break;
            foreach (var card in Monsters(player))
            {
                var targets = Monsters(1 - player).Where(target => ValidateAttack(new DuelCommand
                { Player = player, CardId = card.InstanceId, TargetId = target.InstanceId }).Length == 0).Select(c => c.Ref).ToList();
                bool direct = ValidateAttack(new DuelCommand { Player = player, CardId = card.InstanceId }).Length == 0;
                if (targets.Count == 0 && !direct) continue;
                yield return new DuelAction { Id = "attack:" + card.InstanceId, Kind = DuelCommandKind.Attack,
                    Card = card.Ref, Targets = targets, CanAttackDirectly = direct };
            }
        }

        void DeclareAttack(DuelCommand command)
        {
            var attacker = Card(command.CardId);
            State.Attacker = attacker.Ref;
            State.AttackTarget = command.TargetId == 0 ? default : Card(command.TargetId).Ref;
            State.AttackTargetsAtDeclaration = Monsters(1 - command.Player).Select(x => x.InstanceId).OrderBy(x => x).ToList();
            State.AttackTargetRefsAtDeclaration = Monsters(1 - command.Player).Select(x => x.Ref).OrderBy(x => x.InstanceId).ToList();
            attacker.AttacksThisTurn++;
            if (command.TargetId == 0) attacker.DirectAttackDeclaredThisTurn = true;
            attacker.AttackedTargetsThisTurn.Add(State.AttackTarget);
            State.BattleStep = BattleStep.Declaration;
            Emit(DuelEventKind.AttackDeclared, command.Player, attacker);
            OpenResponse();
        }

        void AdvanceBattle()
        {
            var attacker = Monsters(State.TurnPlayer).FirstOrDefault(x => x.Ref.Equals(State.Attacker));
            bool calculationCompleted = State.BattleStep == BattleStep.AfterCalculation || State.BattleStep == BattleStep.DamageEnd;
            if (!calculationCompleted && (attacker == null || attacker.Controller != State.TurnPlayer || attacker.Position != CardPosition.FaceUpAttack))
            { CompleteBattle(); return; }
            if (State.BattleStep == BattleStep.Declaration && !State.AttackTargetRefsAtDeclaration.SequenceEqual(
                Monsters(1 - State.TurnPlayer).Select(x => x.Ref).OrderBy(x => x.InstanceId)))
            { OpenBattleReplay(attacker); return; }
            var target = State.AttackTarget.InstanceId == 0 ? null
                : Monsters(1 - State.TurnPlayer).FirstOrDefault(x => x.Ref.Equals(State.AttackTarget));
            if (!calculationCompleted && State.AttackTarget.InstanceId != 0 && target == null) { CompleteBattle(); return; }
            switch (State.BattleStep)
            {
                case BattleStep.Declaration:
                    State.BattleStep = BattleStep.DamageStart;
                    break;
                case BattleStep.DamageStart:
                    State.BattleStep = BattleStep.BeforeCalculation;
                    if (target != null && target.Position == CardPosition.FaceDownDefense)
                    { target.Position = CardPosition.FaceUpDefense; Emit(DuelEventKind.Revealed, target.Controller, target); }
                    break;
                case BattleStep.BeforeCalculation: State.BattleStep = BattleStep.Calculation; break;
                case BattleStep.Calculation:
                    CalculateBattle(attacker, target); State.BattleStep = BattleStep.AfterCalculation; break;
                case BattleStep.AfterCalculation:
                    State.BattleStep = BattleStep.DamageEnd;
                    var destruction = BeginDestruction(null, State.Cards.Where(c => State.BattleDestroyed.Contains(c.Ref)),
                        State.Attacker, State.TurnPlayer, MoveCause.Battle, continuation: "battle");
                    if (!destruction.Completed) return;
                    State.BattleDestroyed.Clear(); break;
                case BattleStep.DamageEnd: CompleteBattle(); return;
            }
            if (!State.Finished) OpenResponse();
            var battleSource = Card(State.Attacker.InstanceId);
            Emit(DuelEventKind.BattleStepChanged, State.TurnPlayer, battleSource, amount: (int)State.BattleStep,
                detail: State.BattleStep.ToString(), cause: MoveCause.Battle,
                before: Snapshot(battleSource), after: Snapshot(battleSource));
        }

        void OpenBattleReplay(DuelCardState attacker)
        {
            var targets = Monsters(1 - attacker.Controller);
            var options = targets.Select(c => new DecisionOption { Id = c.InstanceId.ToString(CultureInfo.InvariantCulture),
                HasCard = true, Card = c.Ref, Label = m_catalog.Get(c.DefinitionId).Name })
                .Where(option => !HasEffect(EffectRecordKind.EachMonsterOnce, attacker.Controller, attacker)
                    || !attacker.AttackedTargetsThisTurn.Take(attacker.AttackedTargetsThisTurn.Count - 1).Contains(option.Card)).ToList();
            if (!DirectAttackProhibited(attacker) && attacker.AttacksThisTurn <= MaximumAttacks(attacker, false)
                && (targets.Count == 0 || HasEffect(EffectRecordKind.DirectAttack, attacker.Controller, attacker)))
                options.Add(new DecisionOption { Id = "direct", Label = "直接攻击" });
            options.Add(new DecisionOption { Id = "cancel", Label = "停止攻击" });
            State.PendingDecision = new DuelDecision { Id = State.NextDecisionId++, Player = attacker.Controller,
                Kind = DecisionKind.ChooseMode, Prompt = "战斗重放：重新选择攻击目标", Min = 1, Max = 1,
                Continuation = "battle.replay", SourceId = attacker.InstanceId, Options = options };
            State.Window = TimingWindow.Decision; State.WaitingSeat = attacker.Controller;
            Emit(DuelEventKind.DecisionOpened, attacker.Controller, mask: 1 << attacker.Controller);
        }

        void AnswerBattleReplay(DuelCommand command)
        {
            var choice = State.PendingDecision.Options.Single(o => o.Id == command.Options[0]);
            State.PendingDecision = null;
            if (choice.Id == "cancel") { CompleteBattle(); return; }
            State.AttackTarget = choice.HasCard ? choice.Card : default;
            var attacker = Card(State.Attacker.InstanceId);
            if (choice.Id == "direct") attacker.DirectAttackDeclaredThisTurn = true;
            attacker.AttackedTargetsThisTurn[attacker.AttackedTargetsThisTurn.Count - 1] = State.AttackTarget;
            State.AttackTargetRefsAtDeclaration = Monsters(1 - command.Player).Select(x => x.Ref).OrderBy(x => x.InstanceId).ToList();
            State.AttackTargetsAtDeclaration = State.AttackTargetRefsAtDeclaration.Select(x => x.InstanceId).ToList();
            AdvanceBattle();
        }

        public bool RedirectAttack(DuelCardState target, bool immediateCalculation = false)
        {
            if (State.BattleStep != BattleStep.Declaration || !Monsters(1 - State.TurnPlayer).Contains(target)) return false;
            var attacker = Monsters(State.TurnPlayer).FirstOrDefault(card => card.Ref.Equals(State.Attacker));
            if (attacker == null || attacker.Position != CardPosition.FaceUpAttack) return false;
            State.AttackTarget = target.Ref;
            attacker.AttackedTargetsThisTurn[attacker.AttackedTargetsThisTurn.Count - 1] = target.Ref;
            State.AttackTargetRefsAtDeclaration = Monsters(1 - State.TurnPlayer).Select(card => card.Ref).OrderBy(reference => reference.InstanceId).ToList();
            State.AttackTargetsAtDeclaration = State.AttackTargetRefsAtDeclaration.Select(reference => reference.InstanceId).ToList();
            if (immediateCalculation)
            {
                RefreshCharacteristics(); CalculateBattle(attacker, target);
                State.BattleStep = BattleStep.AfterCalculation;
                Emit(DuelEventKind.BattleStepChanged, State.TurnPlayer, attacker,
                    amount: (int)State.BattleStep, detail: State.BattleStep.ToString(), cause: MoveCause.Battle);
            }
            return true;
        }

        void CalculateBattle(DuelCardState attacker, DuelCardState target)
        {
            if (target == null) { Damage(1 - attacker.Controller, Math.Max(0, attacker.CurrentAtk), MoveCause.Battle); return; }
            bool attack = target.Position == CardPosition.FaceUpAttack;
            int difference = attacker.CurrentAtk - (attack ? target.CurrentAtk : target.CurrentDef.Value);
            if (difference > 0)
            {
                MarkBattleDestroyed(target);
                if (attack || HasEffect(EffectRecordKind.Piercing, attacker.Controller, attacker)) Damage(target.Controller, difference, MoveCause.Battle);
            }
            else if (difference < 0)
            {
                Damage(attacker.Controller, -difference, MoveCause.Battle);
                if (attack) MarkBattleDestroyed(attacker);
            }
            else if (attack && attacker.CurrentAtk > 0)
            { MarkBattleDestroyed(attacker); MarkBattleDestroyed(target); }
        }

        void MarkBattleDestroyed(DuelCardState card)
        {
            if (!HasEffect(EffectRecordKind.BattleIndestructible, card.Controller, card)) State.BattleDestroyed.Add(card.Ref);
        }

        internal void Damage(int player, int amount, MoveCause cause = MoveCause.Effect)
        {
            State.Players[player].LifePoints = Math.Max(0, State.Players[player].LifePoints - amount);
            Emit(DuelEventKind.Damaged, player, amount: amount, cause: cause);
            if (State.Players[player].LifePoints == 0) Finish(1 - player, "LIFE_POINTS_ZERO");
        }

        void CompleteBattle()
        {
            State.BattleStep = BattleStep.None; State.Attacker = default; State.AttackTarget = default;
            State.BattleDestroyed.Clear(); State.WaitingSeat = State.TurnPlayer;
            State.Window = TimingWindow.Open; State.ConsecutivePasses = 0;
        }
    }
}
