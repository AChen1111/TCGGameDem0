using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class MaskChangeCard : CardRules
    {
        public override string CardId => "21143940";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("21143940.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new MaskChangeAbility(); }
    }

    sealed class MaskChangeAbility : IAbilityHandler, IActivationTargetSelection
    {
        public string CardId => "21143940";
        public string AbilityId => CardId + ".1";
        public int Speed => 2;
        public IEnumerable<DuelCardState> TargetCandidates(EffectContext context) => context.State.Cards.Where(card =>
            card.Controller == context.Player && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
            && DuelEngine.IsPublic(card) && context.Catalog.Get(card.DefinitionId).BelongsTo(0x8)
            && context.CanTarget(card) && Candidates(context, card.CurrentAttribute).Any());
        static IEnumerable<DuelCardState> Candidates(EffectContext context, int attribute) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.ExtraDeck && context.Catalog.Get(card.DefinitionId).BelongsTo(0xa008)
            && (context.Catalog.Get(card.DefinitionId).Attribute & attribute) != 0
            && context.CanSpecialSummon(card, context.Player, method: SummonMethod.MaskChange));
        public bool CanActivate(EffectContext context) => TargetCandidates(context).Any();
        public string ValidateActivation(EffectContext context, DuelCommand command) => command.Cards.Length == 0 ? "" : "NO_ACTIVATION_COST";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link) { }
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step >= 5) return;
            if (link.Step == 0)
            {
                var target = context.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (target == null || !context.IsAffected(target)) { link.Step = 5; return; }
                int attribute = DuelEngine.IsPublic(target) ? target.CurrentAttribute : context.Catalog.Get(target.DefinitionId).Attribute;
                context.Move(target, DuelZone.Graveyard);
                if (DuelEngine.OnField(target)) { link.Step = 5; return; }
                var candidates = Candidates(context, attribute).ToArray();
                if (candidates.Length == 0) { link.Step = 5; return; }
                link.Step = 1; context.SelectCards(link, candidates, "选择假面英雄"); return;
            }
            if (link.Step == 1) { link.Values["selected-card"] = link.Selected[0]; link.Step = 2; }
            EffectSummonFlow.Resume(context, link, context.Card(link.Values["selected-card"]), 2, method: SummonMethod.MaskChange);
        }
    }
}
