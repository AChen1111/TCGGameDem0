using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class HieraticSunDragonOverlordOfHeliopolisCard : CardRules
    {
        public override string CardId => "64332231";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(c => c.CurrentLevel == 8);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("64332231.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("64332231.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("64332231.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 1, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => TributeCandidates(c, 0).Any(), Resolve)
                .Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
                { var material = c.Card(command.Cards[0]); link.Costs.Add(material.Ref); c.MoveAsCost(material, DuelZone.Graveyard); });
        }
        static IEnumerable<DuelCardState> TributeCandidates(EffectContext c, int count) => c.State.Cards.Where(card => card.Controller == c.Player
            && (card.Zone == DuelZone.Hand || card.Zone == DuelZone.Monster || card.Zone == DuelZone.ExtraMonster)
            && c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && c.IsAffected(card)
            && c.State.Cards.Count(DuelEngine.OnField) >= count + 1 + (DuelEngine.OnField(card) ? 1 : 0));
        static void AskTribute(EffectContext c, DuelChainLink link)
        { link.Step = 1; c.SelectCards(link, TributeCandidates(c, link.Values["heliopolis-count"]), "选择一只要解放的怪兽"); }
        static void AskDestruction(EffectContext c, DuelChainLink link)
        {
            int count = link.Values["heliopolis-count"];
            if (count == 0) { link.Step = 4; return; }
            link.Step = 3; c.SelectCards(link, c.State.Cards.Where(DuelEngine.OnField), "选择同等数量的场上卡破坏", count, count);
        }
        static void Resolve(EffectContext c, DuelChainLink link)
        {
            if (link.Step == 0)
            { link.Values["heliopolis-count"] = 0; if (TributeCandidates(c, 0).Any()) AskTribute(c, link); else link.Step = 4; return; }
            if (link.Step == 1)
            {
                var tribute = c.Card(link.Selected[0]);
                if (c.IsAffected(tribute) && c.TryMove(tribute, DuelZone.Graveyard)) link.Values["heliopolis-count"]++;
                if (TributeCandidates(c, link.Values["heliopolis-count"]).Any())
                {
                    link.Step = 2;
                    c.OpenDecision(link, DecisionKind.YesNo, new[] {
                        new DecisionOption { Id = "more", Value = "more", Label = "继续解放" },
                        new DecisionOption { Id = "finish", Value = "finish", Label = "完成解放并选择破坏" } }, "是否继续解放怪兽？");
                }
                else AskDestruction(c, link);
                return;
            }
            if (link.Step == 2) { if (link.Answers[0] == "more") AskTribute(c, link); else AskDestruction(c, link); return; }
            if (link.Step == 3)
            {
                if (c.DestroyMany(link, link.Selected.Select(c.Card)).Completed) link.Step = 4;
            }
        }
    }
}
