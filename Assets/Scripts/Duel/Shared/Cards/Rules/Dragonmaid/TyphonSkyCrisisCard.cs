using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed partial class TyphonSkyCrisisCard : CardRules
    {
        public override string CardId => "93039339";
        public override bool HasSummonRecipe => true;
        public override bool MatchesSummonMaterials(CardDefinition target, IReadOnlyList<DuelCardState> materials, DuelCardCatalog catalog) =>
            materials.Count == 2 && materials.All(card => card.CurrentLevel == 12);
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("93039339.1", CardRuleKind.ContinuousRule),
            CardRuleRequirement.Done("93039339.2", CardRuleKind.ActivatedAbility),
            CardRuleRequirement.Done("93039339.summon", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("93039339.overlay", CardRuleKind.SummonProcedure),
            CardRuleRequirement.Done("93039339.restrictions", CardRuleKind.Restriction));
        public override IEnumerable<ICardSummonProcedure> CreateSummonProcedures() { yield return new TyphonOverlayProcedure(); }
        public override IEnumerable<ICardContinuousRule> CreateContinuousRules()
        {
            yield return new ContinuousProgram(CardId + ".1", (c, output) =>
            {
                if (!DuelEngine.OnField(c.Source) || !DuelEngine.IsPublic(c.Source) || c.Source.Negated || c.Source.SummonMethod != SummonMethod.Xyz) return;
                foreach (var card in c.State.Cards.Where(card => DuelEngine.OnField(card) && DuelEngine.IsPublic(card) && card.CurrentAtk >= 3000))
                    output.Add(new DuelEffectRecord { Kind = EffectRecordKind.CannotActivateCard, Target = card.Ref, Player = -1, RequiresSource = true });
            });
        }
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new ProgramAbility(CardId, 2, 1, DragonmaidFlow.Field, c => c.Source.Materials.Count > 0 && Field(c).Any(), (c, link) =>
            {
                if (link.Step != 0) return;
                var target = c.State.Cards.FirstOrDefault(card => card.Ref.Equals(link.Targets[0]) && DuelEngine.OnField(card));
                if (target != null && c.IsAffected(target)) c.Move(target, DuelZone.Hand);
                link.Step = 1;
            }).Cost(1, 1, c => c.Source.Materials.Select(c.Card), (c, command, link) =>
            {
                var material = c.Card(command.Cards[0]);
                link.Costs.Add(material.Ref);
                c.Source.Materials.Remove(material.InstanceId);
                c.Move(material, DuelZone.Graveyard);
            }).Target(Field).Once(CardId + ".2");
        }
        static IEnumerable<DuelCardState> Field(EffectContext context) =>
            context.State.Cards.Where(card => DuelEngine.OnField(card) && context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster && context.CanTarget(card));
    }

    sealed class TyphonOverlayProcedure : ICardSummonProcedure
    {
        public string Id => "93039339.overlay";
        static bool OpponentExtra(EffectContext context) =>
            context.Engine.HasEffect(EffectRecordKind.OpponentExtraSummonWindow, context.Player);
        static IEnumerable<DuelCardState> Highest(EffectContext context)
        {
            var monsters = DragonmaidFlow.FieldMonsters(context, context.Player).Where(card =>
                context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster).ToArray();
            if (monsters.Length == 0) return monsters;
            int attack = monsters.Max(card => card.CurrentAtk);
            return monsters.Where(card => card.CurrentAtk == attack);
        }
        bool Available(EffectContext context) => context.Source.Zone == DuelZone.ExtraDeck && OpponentExtra(context) && Highest(context).Any();
        public IEnumerable<DuelAction> QueryActions(EffectContext context)
        {
            if (!Available(context)) yield break;
            var choices = Highest(context).ToArray();
            yield return new DuelAction { Id = "procedure:" + context.Source.InstanceId + ":" + Id, Kind = DuelCommandKind.SpecialSummon,
                AbilityId = Id, Card = context.Source.Ref,
                Slots = choices.Select(card => card.Zone == DuelZone.ExtraMonster ? card.Slot + 5 : card.Slot).Distinct().ToList(),
                Positions = new List<CardPosition> { CardPosition.FaceUpAttack, CardPosition.FaceUpDefense },
                SelectionCards = choices.Select(card => card.Ref).ToList(), MinSelections = 1, MaxSelections = 1 };
        }
        public string Validate(EffectContext context, DuelCommand command)
        {
            if (!Available(context) || command.Cards.Length != 1) return "INVALID_TYPHON_MATERIAL";
            var material = Highest(context).FirstOrDefault(card => card.InstanceId == command.Cards[0]);
            int slot = material == null ? -1 : material.Zone == DuelZone.ExtraMonster ? material.Slot + 5 : material.Slot;
            return material != null && command.Slot == slot
                && (command.Position == CardPosition.FaceUpAttack || command.Position == CardPosition.FaceUpDefense) ? "" : "INVALID_TYPHON_MATERIAL";
        }
        public void Execute(EffectContext context, DuelCommand command)
        {
            var material = context.Card(command.Cards[0]);
            var names = material.Materials.Select(id => context.Card(id).DefinitionId).ToList();
            names.Add(material.DefinitionId);
            foreach (int attached in material.Materials.ToArray())
            {
                var underneath = context.Card(attached);
                underneath.HostInstanceId = context.Source.InstanceId;
                context.Source.Materials.Add(attached);
            }
            material.Materials.Clear();
            context.Engine.Move(material, DuelZone.Material, material.Owner, cause: MoveCause.SummonMaterial, wasSummonMaterial: true, materialMethod: SummonMethod.Xyz);
            material.HostInstanceId = context.Source.InstanceId;
            context.Source.Materials.Add(material.InstanceId);
            context.Source.Counters["typhon-overlay"] = 1;
            context.SpecialSummon(context.Source, context.Player, command.Slot, command.Position, method: SummonMethod.Xyz);
            context.Source.SummonMaterialDefinitions = names;
        }
    }

    public sealed partial class TyphonSkyCrisisCard
    {
        public override void AfterSummonConfirmed(EffectContext context)
        {
            if (context.Source.SummonMethod != SummonMethod.Xyz || !context.Source.Counters.ContainsKey("typhon-overlay")) return;
            context.Source.Counters.Remove("typhon-overlay");
            context.State.Players[context.Player].NormalSummonsThisTurn = 1;
            context.AddEffect(new DuelEffectRecord { Kind = EffectRecordKind.CannotSpecialSummon, Player = context.Player, ExpiresTurn = context.State.Turn });
        }
    }
}
