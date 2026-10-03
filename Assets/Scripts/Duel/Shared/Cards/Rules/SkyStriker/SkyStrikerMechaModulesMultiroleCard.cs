using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class SkyStrikerMechaModulesMultiroleCard : CardRules
    {
        public override string CardId => "24010609";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("24010609.0", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("24010609.1", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("24010609.2", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 0, 1, new[] { DuelZone.Hand, DuelZone.SpellTrap },
                c => c.Source.Zone == DuelZone.Hand || c.Source.Position == CardPosition.FaceDown, (c, link) => { });
            yield return new InstanceProgramAbility(CardId, 1, 1, new[] { DuelZone.SpellTrap },
                c => DuelEngine.IsPublic(c.Source), (c, link) =>
                {
                    if (link.Step != 0) return;
                    c.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.SpellResponsesBlocked,
                        Player = 1 - c.Player, ExpiresTurn = c.State.Turn });
                    var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                    if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Graveyard);
                    link.Step = 1;
                }).Target(c => c.State.Cards.Where(card => card.Controller == c.Player && card.InstanceId != c.Source.InstanceId
                    && DuelEngine.OnField(card) && c.CanTarget(card)));
            yield return new InstancePhaseTriggerProgramAbility(CardId, 2, new[] { DuelZone.SpellTrap }, false, false,
                (c, fact) => fact.Kind == DuelEventKind.PhaseChanged && fact.PhaseAtEvent == DuelPhase.End,
                c => c.State.Phase == DuelPhase.End,
                c => DuelEngine.IsPublic(c.Source) && ActivatedCount(c) > 0 && SetCandidates(c).Any(), ResolveSets);
        }

        static int ActivatedCount(EffectContext context) => context.State.TurnFacts.Count(fact => fact.Kind == DuelEventKind.Activated
            && fact.Player == context.Player && fact.IsCardActivation && fact.ActivationKind == RuleCardKind.Spell
            && context.Catalog.Get(fact.DefinitionId).BelongsTo(0x115)
            && fact.PresentCards.Any(card => card.Ref.Equals(context.SourceRef) && card.Zone == DuelZone.SpellTrap
                && card.Position != CardPosition.FaceDown)
            && !context.State.TurnFacts.Any(result => result.ChainId == fact.ChainId && result.LinkNumber == fact.LinkNumber
                && result.ActivationNegated));

        static IEnumerable<DuelCardState> SetCandidates(EffectContext context) => context.State.Cards.Where(card =>
            card.Owner == context.Player && card.Zone == DuelZone.Graveyard
            && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Spell
            && context.Catalog.Get(card.DefinitionId).BelongsTo(0x115)
            && (context.Catalog.Get(card.DefinitionId).SpellTrapType == RuleSpellTrapType.Field
                || Enumerable.Range(0, 5).Any(slot => context.Engine.FreeSpellSlot(context.Player, slot))));

        static void ResolveSets(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var candidates = SetCandidates(context).ToArray();
                int unique = candidates.Select(card => context.Catalog.Get(card.DefinitionId).OriginalNameId).Distinct().Count();
                int capacity = Enumerable.Range(0, 5).Count(slot => context.Engine.FreeSpellSlot(context.Player, slot)) + 1;
                int maximum = System.Math.Min(ActivatedCount(context), System.Math.Min(unique, capacity));
                link.Step = 1;
                if (maximum == 0) { link.Step = 4; return; }
                context.SelectCards(link, candidates, "选择不同名的闪刀魔法盖放", 1, maximum);
                context.State.PendingDecision.UniqueOriginalNames = true;
                context.State.PendingDecision.RequireSetCapacity = true;
                return;
            }
            if (link.Step == 1)
            {
                link.Values["set-count"] = link.Selected.Count; link.Values["set-index"] = 0;
                for (int i = 0; i < link.Selected.Count; i++)
                { link.Values["set-card." + i] = link.Selected[i]; context.Reveal(context.Card(link.Selected[i])); }
                link.Step = 2; OpenSetZone(context, link); return;
            }
            if (link.Step == 2)
            {
                int index = link.Values["set-index"];
                link.Values["set-zone." + index] = int.Parse(link.Answers[0], System.Globalization.CultureInfo.InvariantCulture);
                link.Values["set-index"] = index + 1;
                if (index + 1 < link.Values["set-count"]) { OpenSetZone(context, link); return; }
                for (int i = 0; i < link.Values["set-count"]; i++)
                {
                    var card = context.Card(link.Values["set-card." + i]); int slot = link.Values["set-zone." + i];
                    if (slot == 5)
                    {
                        var previous = context.State.Cards.FirstOrDefault(c => c.Controller == context.Player && c.Zone == DuelZone.Field);
                        if (previous != null) context.Engine.Move(previous, DuelZone.Graveyard, previous.Owner, cause: MoveCause.Rule);
                    }
                    context.Engine.Move(card, slot == 5 ? DuelZone.Field : DuelZone.SpellTrap, context.Player,
                        slot == 5 ? 0 : slot, CardPosition.FaceDown, MoveCause.Effect, link.Source, context.Player);
                    card.SetTurn = context.State.Turn;
                    var moved = context.State.PendingFacts.Last(f => f.Kind == DuelEventKind.Moved && f.Card.Equals(card.Ref));
                    moved.VisibleToMask = 1 << context.Player;
                    context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.BanishWhenLeavesField, Target = card.Ref });
                }
                link.Step = 4;
            }
        }

        static void OpenSetZone(EffectContext context, DuelChainLink link)
        {
            int index = link.Values["set-index"];
            var card = context.Card(link.Values["set-card." + index]);
            var reserved = Enumerable.Range(0, index).Select(i => link.Values["set-zone." + i]).ToArray();
            var slots = context.Catalog.Get(card.DefinitionId).SpellTrapType == RuleSpellTrapType.Field
                ? new[] { 5 } : Enumerable.Range(0, 5).Where(slot => context.Engine.FreeSpellSlot(context.Player, slot) && !reserved.Contains(slot));
            context.OpenDecision(link, DecisionKind.ChooseZone, slots.Select(slot => new DecisionOption {
                Id = slot.ToString(System.Globalization.CultureInfo.InvariantCulture), Value = slot.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Label = slot == 5 ? "场地区域" : "魔法陷阱区域 " + (slot + 1) }), "选择盖放区域");
        }
    }
}
