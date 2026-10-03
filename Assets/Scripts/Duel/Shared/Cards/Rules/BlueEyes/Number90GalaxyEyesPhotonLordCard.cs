using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class Number90GalaxyEyesPhotonLordCard : CardRules
    {
        public override string CardId => "08165596";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(c => c.CurrentLevel == 8);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("08165596.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("08165596.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("08165596.3", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("08165596.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("08165596.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 3, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.TurnPlayer != c.Player && DeckCandidates(c).Any(), (c, link) =>
                {
                    if (link.Step >= 3) return;
                    if (link.Step == 0)
                    {
                        var candidates = DeckCandidates(c).ToArray(); link.Step = 1;
                        if (candidates.Length == 0) { link.Step = 3; return; }
                        c.SelectCards(link, candidates, "选择光子或银河卡"); return;
                    }
                    if (link.Step == 1)
                    {
                        link.Values["lord-selected"] = link.Selected[0];
                        var modes = new List<DecisionOption>();
                        if (!c.Engine.HasEffect(EffectRecordKind.PreventDeckToHand, c.Player))
                            modes.Add(new DecisionOption { Id = "hand", Value = "hand", Label = "加入手牌" });
                        if (c.Source.Ref.Equals(c.SourceRef) && (c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster))
                            modes.Add(new DecisionOption { Id = "material", Value = "material", Label = "作为超量素材" });
                        link.Step = 2;
                        if (modes.Count == 0) { link.Step = 3; return; }
                        c.OpenDecision(link, DecisionKind.ChooseMode, modes, "选择光子或银河卡的去向"); return;
                    }
                    var selected = c.Card(link.Values["lord-selected"]);
                    if (link.Answers[0] == "hand")
                    { if (c.TryMove(selected, DuelZone.Hand, CardPosition.FaceDown)) c.Reveal(selected); }
                    else if (c.Source.Ref.Equals(c.SourceRef) && (c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster))
                    { c.Move(selected, DuelZone.Material); selected.HostInstanceId = c.Source.InstanceId; c.Source.Materials.Add(selected.InstanceId); }
                    c.ShuffleDeck(); link.Step = 3;
                }).Once(CardId + ".3").Category(EffectCategories.AddFromDeckToHand);
            yield return new ProgramAbility(CardId, 2, 2, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                c => c.State.Chain.Count > 0 && c.State.Chain.Last().Player != c.Player && c.State.Chain.Last().ActivationKind == RuleCardKind.Monster,
                (c, link) =>
                {
                    if (link.Step != 0) return;
                    var previous = c.State.Chain.Single(item => item.Number == link.Values["photon-counter-link"]);
                    if (!c.Engine.TryNegateEffect(previous, c.SourceRef) || link.Values["photon-galaxy-cost"] == 0) { link.Step = 1; return; }
                    var source = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(previous.Source));
                    if (c.DestroyMany(link, source == null ? System.Array.Empty<DuelCardState>() : new[] { source }).Completed) link.Step = 1;
                }).Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
                {
                    link.Values["photon-counter-link"] = c.State.Chain.Last().Number;
                    var material = c.Card(command.Cards[0]); link.Values["photon-galaxy-cost"] = c.Catalog.Get(material.DefinitionId).BelongsTo(0x7b) ? 1 : 0;
                    link.Costs.Add(material.Ref); c.MoveAsCost(material, DuelZone.Graveyard);
                }).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> DeckCandidates(EffectContext c) => c.Deck.Where(card =>
            c.Catalog.Get(card.DefinitionId).BelongsTo(0x55) || c.Catalog.Get(card.DefinitionId).BelongsTo(0x7b));
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, output) =>
            {
                if ((c.Source.Zone == DuelZone.Monster || c.Source.Zone == DuelZone.ExtraMonster) && DuelEngine.IsPublic(c.Source) && !c.Source.Negated
                    && c.Source.Materials.Any(id => c.Catalog.Get(c.Card(id).DefinitionId).BelongsTo(0x55)))
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.EffectIndestructible, Target = c.SourceRef });
            });
        }
    }
}
