using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class DarkEndEvaporationDragonCard : CardRules
    {
        public override string CardId => "62002838";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("62002838.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("62002838.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("62002838.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 1, new[] { DuelZone.Hand },
                c => Costs(c).Any() && c.Engine.GetSpecialSummonDestinations(c.Source, c.Player).Count > 0, (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        c.Move(c.Card(link.Costs[0].InstanceId), DuelZone.Banished);
                        c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.OnlySummonRace, Player = c.Player,
                            Value = DragonmaidFlow.Dragon, ExpiresTurn = c.State.Turn });
                        link.Step = 1;
                    }
                    DragonmaidFlow.ResumeSummon(c, link, 1, false, SummonMethod.Effect);
                }).Cost(1, 1, Costs, (c, command, link) =>
                {
                    link.Costs.Add(c.Card(command.Cards[0]).Ref);
                    link.Values["picked"] = c.Source.InstanceId;
                }).Once(CardId + ".1");
            yield return new DarkEndChoiceAbility();
        }
        static IEnumerable<DuelCardState> Costs(EffectContext context) =>
            DragonmaidFlow.Mine(context, DuelZone.ExtraDeck).Where(card =>
            {
                var printed = context.Catalog.Get(card.DefinitionId);
                return DragonmaidFlow.IsDragon(printed) && printed.Level == 8;
            });
    }

    sealed class DarkEndChoiceAbility : IAbilityHandler, IActivationUsageLimit, IActivationModeSelection, IActivationSourcePolicy
    {
        public string CardId => "62002838";
        public string AbilityId => CardId + ".2";
        public int Speed => 1;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => true;
        public bool AllowsSource(EffectContext context) => DuelEngine.OnField(context.Source) && DuelEngine.IsPublic(context.Source);
        public IEnumerable<AbilityMode> Modes(EffectContext context)
        {
            if (DragonmaidFlow.FusionTargets(context, card => card.MonsterType == RuleMonsterType.Fusion).Any())
                yield return new AbilityMode { Id = "fusion", Label = "融合召唤" };
            if (context.Source.CurrentAtk >= 500 && context.Source.CurrentDef >= 500 && Attacks(context).Any())
                yield return new AbilityMode { Id = "destroy", Label = "攻击力守备力下降并破坏" };
        }
        public bool CanActivate(EffectContext context) => AllowsSource(context) && Modes(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) =>
            command.Cards.Length == 0 && command.TargetId == 0 ? "" : "NO_ACTIVATION_TARGET";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        static IEnumerable<DuelCardState> Attacks(EffectContext context) => context.State.Cards.Where(card =>
            card.InstanceId != context.Source.InstanceId && DuelEngine.OnField(card) && card.Position == CardPosition.FaceUpAttack && context.CanTarget(card));
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.ModeId == "fusion")
            {
                DragonmaidFlow.Fusion(context, link, card => card.MonsterType == RuleMonsterType.Fusion);
                return;
            }
            if (link.Step == 0)
            {
                link.Step = 1;
                context.SelectCards(link, Attacks(context), "选择破坏的攻击表示怪兽");
                return;
            }
            if (link.Step != 1) return;
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddAttack, Target = context.SourceRef, Value = -500 });
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.AddDefense, Target = context.SourceRef, Value = -500 });
            var target = context.State.Cards.FirstOrDefault(card => card.InstanceId == link.Selected[0] && DuelEngine.OnField(card));
            if (target != null && context.IsAffected(target)) context.Destroy(target);
            link.Step = 2;
        }
    }
}
