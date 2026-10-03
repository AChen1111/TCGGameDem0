using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerAlternativeLemnisGateCard : CardRules
    {
        public override string CardId => "34433770";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("34433770.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("34433770.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("34433770.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new LemnisGateReturnAbility(CardId);
            IEnumerable<DuelCardState> Links(EffectContext c) => c.State.Cards.Where(card => card.Owner == c.Player && card.Zone == DuelZone.ExtraDeck
                && c.Catalog.Get(card.DefinitionId).MonsterType == RuleMonsterType.Link && c.Catalog.Get(card.DefinitionId).BelongsTo(0x1115)
                && c.Engine.GetExtraSummonMaterialGroups(card, c.Player).Count > 0);
            yield return new TriggerProgramAbility(CardId, 2, new[] { DuelZone.Graveyard }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.Summoned && fact.After != null && fact.After.Controller == c.Player
                    && fact.SummonMethod != SummonMethod.Normal && fact.SummonMethod != SummonMethod.Flip
                    && c.Catalog.Get(fact.DefinitionId).BelongsTo(0x115), c => Links(c).Any(),
                (c, link) =>
                {
                    if (link.Step == 0)
                    {
                        var links = Links(c).ToArray(); link.Step = 1;
                        if (links.Length == 0) { link.Step = 4; return; }
                        c.SelectCards(link, links, "选择连接召唤的闪刀姬"); return;
                    }
                    if (link.Step == 1)
                    {
                        var target = c.Card(link.Selected[0]); link.Values["link-target"] = target.InstanceId;
                        var groups = c.Engine.GetExtraSummonMaterialGroups(target, c.Player); link.Step = 2;
                        c.SelectCards(link, groups.SelectMany(group => group).Distinct(), "选择连接素材", groups.Min(group => group.Length), groups.Max(group => group.Length));
                        c.State.PendingDecision.AllowedCardGroups = groups.Select(group => group.Select(card => card.Ref).ToList()).ToList(); return;
                    }
                    if (link.Step == 2)
                    {
                        link.Values["material-count"] = link.Selected.Count;
                        for (int index = 0; index < link.Selected.Count; index++) link.Values["material:" + index] = link.Selected[index];
                        var target = c.Card(link.Values["link-target"]);
                        var materials = Enumerable.Range(0, link.Values["material-count"]).Select(index => c.Card(link.Values["material:" + index])).ToArray();
                        var slots = c.Engine.GetExtraSummonDestinations(target, c.Player, materials); link.Step = 3;
                        c.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption {
                            Id = slot.ToString(System.Globalization.CultureInfo.InvariantCulture), Value = slot.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            Label = "区域 " + (slot + 1) }), "选择连接召唤区域"); return;
                    }
                    if (link.Step == 3)
                    {
                        var target = c.Card(link.Values["link-target"]);
                        var materials = Enumerable.Range(0, link.Values["material-count"]).Select(index => c.Card(link.Values["material:" + index])).ToArray();
                        c.Engine.ExtraSummonByEffect(target, materials, c.Player, int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture),
                            effectSource: link.Source); link.Step = 4;
                    }
                }).Pay((c, command, link) => { link.Costs.Add(c.Source.Ref); c.MoveAsCost(c.Source, DuelZone.Banished); })
                .Once(CardId + ".2");
        }
    }

    sealed class LemnisGateReturnAbility : ProgramAbility, IActivationSelectedTargets
    {
        static bool Monster(EffectContext c, DuelCardState card) => c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster
            && c.Catalog.Get(card.DefinitionId).BelongsTo(0x1115);
        static bool Spell(EffectContext c, DuelCardState card) => c.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell && SkyFlow.Striker(c, card);
        static IEnumerable<DuelCardState> Candidates(EffectContext c) => c.State.Cards.Where(card => card.Owner == c.Player
            && card.Zone == DuelZone.Graveyard && (Monster(c, card) || Spell(c, card)) && c.CanTarget(card));
        internal LemnisGateReturnAbility(string id) : base(id, 1, 2, SkyFlow.SpellZones,
            c => Candidates(c).Any(card => Monster(c, card)) && Candidates(c).Any(card => Spell(c, card)), ResolveCards)
        {
            Once(id + ".1"); Category(EffectCategories.AddFromGraveyardToHandDeckExtra);
            Cost(2, 120, Candidates, (c, command, link) => link.Targets.AddRange(command.Cards.Select(c.Card).Select(card => card.Ref)));
            Validate((c, command) => command.Cards.Select(c.Card).Count(card => Monster(c, card))
                == command.Cards.Select(c.Card).Count(card => Spell(c, card)) ? "" : "INVALID_TARGET_GROUP");
        }
        static void ResolveCards(EffectContext c, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                int returned = 0;
                foreach (var card in c.State.Cards.Where(card => link.Targets.Contains(card.Ref) && card.Zone == DuelZone.Graveyard))
                    if (c.IsAffected(card) && c.TryMove(card, DuelZone.Deck)) returned++;
                c.ShuffleDeck(); link.Values["lemnis-bounce"] = returned / 3; link.Step = 1;
                if (returned < 3 || !c.State.Cards.Any(DuelEngine.OnField)) { link.Step = 2; return; }
                var field = c.State.Cards.Where(DuelEngine.OnField).ToArray();
                c.OpenDecision(link, DecisionKind.ChooseCards, field.Select(card => new DecisionOption {
                    Id = card.InstanceId.ToString(System.Globalization.CultureInfo.InvariantCulture), HasCard = true, Card = card.Ref,
                    Label = c.Catalog.Get(card.DefinitionId).Name }), "选择返回手卡的场上卡（可不选）", 0, System.Math.Min(returned / 3, field.Length)); return;
            }
            if (link.Step == 1)
            {
                foreach (int id in link.Selected) { var card = c.Card(id); if (c.IsAffected(card)) c.TryMove(card, DuelZone.Hand); }
                link.Step = 2;
            }
        }
    }
}
