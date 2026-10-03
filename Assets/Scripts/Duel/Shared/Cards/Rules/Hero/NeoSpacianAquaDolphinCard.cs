using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class NeoSpacianAquaDolphinCard : CardRules
    {
        public override string CardId => "17955766";
        public override CardRuleSupport Support => new CardRuleSupport(
            CardRuleRequirement.Done("17955766.1", CardRuleKind.ActivatedAbility));
        public override IEnumerable<IAbilityHandler> CreateAbilities()
        {
            yield return new InstanceProgramAbility(CardId, 1, 1, new[] { DuelZone.Monster, DuelZone.ExtraMonster },
                context => context.State.Cards.Any(card => card.Owner == 1 - context.Player && card.Zone == DuelZone.Hand), Resolve)
                .Cost(1, 1, context => context.State.Cards.Where(card => card.Owner == context.Player && card.Zone == DuelZone.Hand),
                    (context, command, link) => { var card = context.Card(command.Cards[0]); link.Costs.Add(card.Ref); context.MoveAsCost(card, DuelZone.Graveyard); })
                .Once(CardId + ".1", false);
        }
        static void ClearConfirmation(EffectContext context, DuelChainLink link)
        {
            foreach (var card in context.State.Cards)
                if (link.Values.TryGetValue("reveal-generation:" + card.InstanceId, out int generation) && card.Generation == generation)
                    card.RevealedToMask = link.Values["reveal-mask:" + card.InstanceId];
        }
        static void Resolve(EffectContext context, DuelChainLink link)
        {
            if (link.Step == 0)
            {
                var hand = context.State.Cards.Where(card => card.Owner == 1 - context.Player && card.Zone == DuelZone.Hand).ToArray();
                foreach (var card in hand)
                {
                    link.Values["reveal-generation:" + card.InstanceId] = card.Generation;
                    link.Values["reveal-mask:" + card.InstanceId] = card.RevealedToMask;
                    card.RevealedToMask |= 1 << context.Player; context.Reveal(card);
                }
                var monsters = hand.Where(card => context.Catalog.Get(card.DefinitionId).Kind == RuleCardKind.Monster).ToArray();
                link.Step = 1;
                if (monsters.Length == 0) { ClearConfirmation(context, link); link.Step = 2; return; }
                context.SelectCards(link, monsters, "选择确认手卡中的怪兽"); return;
            }
            if (link.Step == 1)
            {
                if (!link.Values.ContainsKey("chosen")) link.Values["chosen"] = link.Selected[0];
                var selected = context.Card(link.Values["chosen"]);
                if (HeroContinuousRule.Monsters(context, context.Player).Any(card => card.CurrentAtk >= selected.CurrentAtk))
                {
                    var operation = context.DestroyMany(link, new[] { selected });
                    if (!operation.Completed) return;
                    if (operation.Destroyed.Count > 0) context.Engine.Damage(1 - context.Player, 500);
                }
                else context.Engine.Damage(context.Player, 500);
                ClearConfirmation(context, link); link.Step = 2;
            }
        }
    }
}
