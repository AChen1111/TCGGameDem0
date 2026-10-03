using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class PrototypeSkyStrikerAceAmatsuCard : CardRules
    {
        public override string CardId => "25072579";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("25072579.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("25072579.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("25072579.summon", CardRuleKind.SummonProcedure, "summon-materials"),
            CardRuleRequirement.Done("25072579.restrictions", CardRuleKind.Restriction));
        public override bool AllowsSummonMethod(DuelCardState card, int player, DuelState state, SummonMethod method) =>
            SkyFlow.OnceSpecialSummon(CardId, player, state);
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 1, 2, SkyFlow.MonsterZones,
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().Player == 1 - c.Player
                    && c.State.Chain.Last().ActivationKind == RuleCardKind.Monster
                    && (c.State.Chain.Last().ActivationSource.Zone == DuelZone.Monster || c.State.Chain.Last().ActivationSource.Zone == DuelZone.ExtraMonster)
                    && c.State.Chain.Last().ActivationSource.Attack >= 2000,
                (c, link) =>
                {
                    var original = c.State.Chain.Single(candidate => candidate.Number == link.Values["rewrite-link"]);
                    original.ReplacementCardId = CardId; original.ReplacementProgram = "destroy.opponent.striker.link";
                    original.Step = 0; original.Selected.Clear(); original.Answers.Clear(); original.Targets.Clear();
                    original.Categories = EffectCategories.None;
                }).Pay((c, command, link) => link.Values["rewrite-link"] = c.State.Chain.Last().Number).Once(CardId + ".1");
            yield return new AmatsuBattleAbility(CardId);
        }
        public override void ResolveReplacement(EffectContext context, DuelChainLink link, string program)
        {
            if (program != "destroy.opponent.striker.link") throw new System.InvalidOperationException("Unknown replacement program: " + program);
            if (link.Step == 0)
            {
                var cards = context.State.Cards.Where(card => card.Controller == 1 - context.Player
                    && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
                    && context.Catalog.Get(card.DefinitionId).BelongsTo(0x1115)
                    && context.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Link).ToArray(); link.Step = 1;
                if (cards.Length == 0) { link.Step = 2; return; }
                context.SelectCards(link, cards, "选择破坏的对方闪刀姬连接怪兽"); return;
            }
            if (link.Step == 1)
            {
                var operation = context.DestroyMany(link, new[] { context.Card(link.Selected[0]) });
                if (operation.Completed) link.Step = 2;
            }
        }
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog)
        {
            var cards = materials.Select(card => catalog.Get(card.DefinitionId)).ToArray();
            return cards.Length == 1 && cards.All(c => c.BelongsTo(0x1115));
        }
    }

    sealed class AmatsuBattleAbility : TriggerProgramAbility, IActivationSelectedTargets
    {
        static bool OwnStriker(EffectContext c, DuelCardState card) => card.Controller == c.Player
            && (card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
            && DuelEngine.IsPublic(card) && c.Catalog.Get(card.DefinitionId).BelongsTo(0x1115);
        internal AmatsuBattleAbility(string id) : base(id, 2, SkyFlow.MonsterZones, false, false,
            (c, fact) => fact.Kind == DuelEventKind.AttackDeclared && fact.BattleTarget.InstanceId != 0
                && (fact.BattleAttacker.Equals(c.Source.Ref) || fact.BattleTarget.Equals(c.Source.Ref)),
            c => c.State.Cards.Any(card => OwnStriker(c, card) && c.CanTarget(card))
                && c.State.Cards.Any(card => card.Controller == 1 - c.Player && DuelEngine.OnField(card) && c.CanTarget(card)),
            (c, link) => c.DestroyMany(link, c.State.Cards.Where(card => link.Targets.Contains(card.Ref) && DuelEngine.OnField(card))))
        {
            Once(id + ".2");
            Cost(2, 2, c => c.State.Cards.Where(card => (OwnStriker(c, card)
                || card.Controller == 1 - c.Player && DuelEngine.OnField(card)) && c.CanTarget(card)),
                (c, command, link) => link.Targets.AddRange(command.Cards.Select(c.Card).Select(card => card.Ref)));
            Validate((c, command) => command.Cards.Select(c.Card).Count(card => OwnStriker(c, card)) == 1
                && command.Cards.Select(c.Card).Count(card => card.Controller == 1 - c.Player) == 1 ? "" : "INVALID_TARGET_GROUP");
        }
    }
}
