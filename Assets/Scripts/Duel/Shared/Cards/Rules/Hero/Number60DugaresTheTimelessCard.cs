using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class Number60DugaresTheTimelessCard : CardRules
    {
        public override string CardId => "66011101";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("66011101.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("66011101.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("66011101.restrictions", CardRuleKind.Restriction));
        public override bool HasSummonRecipe => true;
        public override int MinSummonMaterials => 2;
        public override int MaxSummonMaterials => 2;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => card.CurrentLevel == 4);
        public override IEnumerable<IAbilityHandler> CreateAbilities() { yield return new DugaresAbility(); }
    }

    sealed class DugaresAbility : IAbilityHandler, IActivationSourcePolicy, IActivationUsageLimit, IActivationCostSelection, IActivationModeSelection
    {
        public string CardId => "66011101";
        public string AbilityId => CardId + ".1";
        public int Speed => 1;
        public string UsageKey => AbilityId;
        public int Limit => 1;
        public bool CountNegatedActivation => true;
        public int MinCosts => 2;
        public int MaxCosts => 2;
        public bool AllowsSource(EffectContext context) => HeroDeckTriggerAbility.FaceUpMonster(context);
        public IEnumerable<DuelCardState> CostCandidates(EffectContext context) => context.Source.Materials.Select(context.Card);
        public bool CanActivate(EffectContext context) => AllowsSource(context) && context.Source.Materials.Count >= 2 && Modes(context).Any();
        public IEnumerable<AbilityMode> Modes(EffectContext context)
        {
            if (context.State.Players[context.Player].Deck.Count >= 2 && !context.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, context.Player))
                yield return new AbilityMode { Id = "draw", Label = "抽卡并弃牌", Categories = EffectCategories.AddFromDeckToHand };
            if (RevivalChoices(context).Any() || CostCandidates(context).Any(card =>
                context.Catalog.Get(card.DefinitionId).MonsterType != RuleMonsterType.Link
                && !context.Engine.HasEffect(EffectRecordKind.BanishOpponentGraveyard, card.Owner, card)
                && context.Engine.CanSpecialSummonConditions(card, context.Player, fromMaterial: true)
                && context.Engine.GetSpecialSummonDestinations(context.Source, context.Player).Any()))
                yield return new AbilityMode { Id = "revive", Label = "守备表示特殊召唤", Categories = EffectCategories.SpecialSummonFromGraveyard };
            if (HeroContinuousRule.Monsters(context, context.Player).Any())
                yield return new AbilityMode { Id = "double", Label = "攻击力变成两倍" };
        }
        static IEnumerable<DuelCardState> RevivalChoices(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.Graveyard && context.Catalog.Get(card.DefinitionId).MonsterType != RuleMonsterType.Link
            && context.CanSpecialSummon(card, context.Player));
        public string ValidateActivation(EffectContext context, DuelCommand command) => command.Cards.Length == 2 && command.Cards.Distinct().Count() == 2
            && command.Cards.All(id => context.Source.Materials.Contains(id)) ? "" : "INVALID_XYZ_COST";
        public void PayCost(EffectContext context, DuelCommand command, DuelChainLink link)
        { foreach (int id in command.Cards) { var card = context.Card(id); link.Costs.Add(card.Ref); context.MoveAsCost(card, DuelZone.Graveyard); } }
        static void Skip(EffectContext context, DuelPhase phase) => context.AddEffect(new DuelEffectRecord {
            Kind = EffectRecordKind.SkipPhase, Player = context.Player, Value = (int)phase,
            ExpiresTurn = context.State.Turn + (context.State.TurnPlayer == context.Player ? 2 : 1), ExpiresPhase = phase });
        public void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.ModeId == "draw")
            {
                if (link.Step == 0)
                {
                    context.Engine.Draw(context.Player, 2); link.Step = 1;
                    if (context.State.Finished) { link.Step = 2; return; }
                    context.SelectCards(link, context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.Hand), "选择丢弃的手卡"); return;
                }
                if (link.Step == 1) { context.Move(context.Card(link.Selected[0]), DuelZone.Graveyard); Skip(context, DuelPhase.Draw); link.Step = 2; }
            }
            else if (link.ModeId == "double")
            {
                if (link.Step == 0)
                {
                    var candidates = HeroContinuousRule.Monsters(context, context.Player).ToArray(); link.Step = 1;
                    if (candidates.Length == 0) { Skip(context, DuelPhase.Battle); link.Step = 2; return; }
                    context.SelectCards(link, candidates, "选择攻击力变成两倍的怪兽"); return;
                }
                if (link.Step == 1)
                {
                    var target = context.Card(link.Selected[0]);
                    if (context.IsAffected(target)) context.AddEffect(new DuelEffectRecord {
                        Kind = EffectRecordKind.SetAttack, Target = target.Ref, Value = target.CurrentAtk * 2, ExpiresTurn = context.State.Turn });
                    Skip(context, DuelPhase.Battle); link.Step = 2;
                }
            }
            else if (link.ModeId == "revive")
            {
                if (link.Step == 0)
                {
                    var choices = RevivalChoices(context).ToArray(); link.Step = 1;
                    if (choices.Length == 0) { link.Step = 3; Skip(context, DuelPhase.Main1); return; }
                    context.SelectCards(link, choices, "选择守备表示特殊召唤的怪兽"); return;
                }
                if (link.Step == 1)
                {
                    link.Values["summon-card"] = link.Selected[0]; link.Step = 2;
                    var card = context.Card(link.Selected[0]);
                    context.OpenDecision(link, DecisionKind.ChooseZone, context.Engine.GetSpecialSummonDestinations(card, context.Player).Select(slot =>
                        new DecisionOption { Id = slot.ToString(), Value = slot.ToString(), Label = "区域 " + slot }), "选择区域"); return;
                }
                if (link.Step == 2)
                {
                    context.SpecialSummon(context.Card(link.Values["summon-card"]), context.Player,
                        int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture), CardPosition.FaceUpDefense);
                    Skip(context, DuelPhase.Main1); link.Step = 3;
                }
            }
        }
    }
}
