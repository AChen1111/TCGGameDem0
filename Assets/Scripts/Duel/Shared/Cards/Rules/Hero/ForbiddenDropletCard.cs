using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class ForbiddenDropletCard : CardRules
    {
        public override string CardId => "24299458";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("24299458.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("24299458.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new ForbiddenDropletAbility(); }
    }

    sealed class ForbiddenDropletAbility : IAbilityHandler, IActivationCostSelection, IActivationUsageLimit, IDamageStepAbility
    {
        public string CardId => "24299458";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public string UsageKey => CardId;
        public int Limit => 1;
        public bool CountNegatedActivation => false;
        public int MinCosts => 1;
        public int MaxCosts => int.MaxValue;
        public bool AllowsDamageStep(BattleStep step) => step == BattleStep.DamageStart || step == BattleStep.BeforeCalculation;
        public IEnumerable<DuelCardState> CostCandidates(EffectContext context) => context.State.Cards.Where(card =>
            card.InstanceId != context.Source.InstanceId && (card.Owner == context.Player && card.Zone == DuelZone.Hand
                || card.Controller == context.Player && DuelEngine.OnField(card))
            && !context.Engine.HasEffect(EffectRecordKind.BanishOpponentGraveyard, card.Owner, card));
        static IEnumerable<DuelCardState> Candidates(EffectContext context) => HeroContinuousRule.Monsters(context, 1 - context.Player)
            .Where(card => !card.CurrentNormal);
        public bool CanActivate(EffectContext context) => CostCandidates(context).Any() && Candidates(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) => command.TargetId == 0
            && command.Cards.Length >= 1 && command.Cards.Length <= Candidates(context).Count()
            && command.Cards.Distinct().Count() == command.Cards.Length
            && command.Cards.All(id => CostCandidates(context).Any(card => card.InstanceId == id)) ? "" : "INVALID_DROPLET_COST";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        {
            int mask = 0;
            foreach (int id in command.Cards)
            {
                var card = context.Card(id);
                mask |= 1 << ((int)context.Catalog.Get(card.DefinitionId).Kind - 1);
                link.Costs.Add(card.Ref); context.MoveAsCost(card, DuelZone.Graveyard);
            }
            link.Values["cost-count"] = command.Cards.Length;
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.PreventResponses, Player = 1 - context.Player,
                Value = mask, ChainId = context.State.CurrentChainId, ResponseToLink = link.Number });
        }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = Candidates(context).ToArray();
                int count = System.Math.Min(link.Values["cost-count"], candidates.Length);
                link.Step = 1;
                if (count == 0) { link.Step = 2; return; }
                context.SelectCards(link, candidates, "选择无效的效果怪兽", count, count); return;
            }
            if (link.Step == 1)
            {
                foreach (int id in link.Selected)
                {
                    var target = context.Card(id);
                    if (!context.IsAffected(target)) continue;
                    int attack = (target.CurrentAtk + 1) / 2;
                    context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.TargetNegate, Target = target.Ref, ExpiresTurn = context.State.Turn });
                    context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SetAttack, Target = target.Ref, Value = attack, ExpiresTurn = context.State.Turn });
                }
                link.Step = 2;
            }
        }
    }
}
