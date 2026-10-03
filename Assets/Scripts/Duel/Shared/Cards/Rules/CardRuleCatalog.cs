using System;
using System.Collections.Generic;
using System.Linq;

namespace AChen.Duel.Core
{
    public sealed class CardRuleCatalog
    {
        readonly DuelCardCatalog m_printed;
        readonly Dictionary<string, CardRules> m_rules;
        public IReadOnlyList<CardRules> Cards { get; }
        public CardRuleCatalog(DuelCardCatalog printed, IEnumerable<CardRules> rules)
        {
            m_printed = printed;
            Cards = Array.AsReadOnly(rules.ToArray());
            m_rules = Cards.ToDictionary(card => card.CardId, StringComparer.Ordinal);
            foreach (var card in Cards)
            {
                if (!printed.Get(card.CardId).IsNormal && card.Support.Requirements.Count == 0)
                    throw new InvalidOperationException("Effect card has no support requirements: " + card.CardId);
                if (card.Support.Requirements.Select(rule => rule.Id).Distinct(StringComparer.Ordinal).Count()
                    != card.Support.Requirements.Count)
                    throw new InvalidOperationException("Duplicate rule requirement: " + card.CardId);
                var abilities = card.CreateAbilities().ToArray();
                var continuous = card.CreateContinuousRules().ToArray();
                foreach (var ability in abilities)
                    if (ability.CardId != card.CardId || !card.Support.Requirements.Any(rule => rule.Id == ability.AbilityId
                        && rule.Kind == CardRuleKind.ActivatedAbility && rule.IsImplemented))
                        throw new InvalidOperationException("Ability not declared as implemented: " + ability.AbilityId);
                foreach (var rule in continuous)
                    if (!card.Support.Requirements.Any(requirement => requirement.Id == rule.RuleId
                        && requirement.Kind == CardRuleKind.ContinuousRule && requirement.IsImplemented))
                        throw new InvalidOperationException("Continuous rule not declared as implemented: " + rule.RuleId);
                foreach (var rule in card.Support.Requirements.Where(requirement => requirement.IsImplemented))
                    if (rule.Kind == CardRuleKind.ActivatedAbility && !abilities.Any(ability => ability.AbilityId == rule.Id)
                        || rule.Kind == CardRuleKind.ContinuousRule && !continuous.Any(continuousRule => continuousRule.RuleId == rule.Id))
                        throw new InvalidOperationException("Implemented rule has no handler: " + rule.Id);
            }
        }
        public CardRules Get(string cardId) => m_rules[m_printed.ResolveCardId(cardId)];
        public IReadOnlyList<MissingCardRule> CheckDeckSupport(IEnumerable<string> cardIds) =>
            Array.AsReadOnly(cardIds.Select(m_printed.ResolveCardId).Distinct(StringComparer.Ordinal)
                .OrderBy(id => id, StringComparer.Ordinal)
                .SelectMany(id => Get(id).Support.Requirements.Where(rule => !rule.IsImplemented)
                    .Select(rule => new MissingCardRule(id, rule))).ToArray());
        public static CardRuleCatalog CreateDefault(DuelCardCatalog printed) => new CardRuleCatalog(printed,
            new CardRules[]
            {
                new EEmergencyCallCard(),
                new DivineDragonKnightFelgrandCard(),
                new XtraHEROWonderDriverCard(),
                new BlueEyesTwinBurstDragonCard(),
                new GalaxyEyesCipherBladeDragonCard(),
                new ReturnOfTheDragonLordsCard(),
                new Number90GalaxyEyesPhotonLordCard(),
                new SageWithEyesOfBlueCard(),
                new SkyStrikerAceHayateCard(),
                new AHeroLivesCard(),
                new DestinyHEROMaliciousCard(),
                new SkyStrikerMobilizeLinkageCard(),
                new ENShuffleCard(),
                new SkyStrikerAceKainaCard(),
                new AshBlossomJoyousSpringCard(),
                new CombinedManeuverEngageZeroCard(),
                new NeoSpacianAquaDolphinCard(),
                new GalaxyEyesCipherDragonCard(),
                new XtraHEROInfernalDevicerCard(),
                new PillarOfTheFutureCyanosCard(),
                new MaskChangeCard(),
                new ElementalHEROSunriseCard(),
                new ContrastHEROChaosCard(),
                new MaxxCCard(),
                new SkyStrikerMechaModulesMultiroleCard(),
                new PolymerizationCard(),
                new CalledByTheGraveCard(),
                new ForbiddenDropletCard(),
                new PrototypeSkyStrikerAceAmatsuCard(),
                new TripleTacticsTalentCard(),
                new SkyStrikerAceRayeCard(),
                new VisionHEROVyonCard(),
                new ReinforcementOfTheArmyCard(),
                new WakeUpYourElementalHEROCard(),
                new BlackRoseMoonlightDragonCard(),
                new SkyStrikerAlternativeLemnisGateCard(),
                new PotOfDesiresCard(),
                new TripleTacticsThrustCard(),
                new SkyStrikerAceRozeCard(),
                new TradeInCard(),
                new BlueEyesAlternativeWhiteDragonCard(),
                new GalaxyEyesFullArmorPhotonDragonCard(),
                new CardsOfConsonanceCard(),
                new ElementalHEROStratosCard(),
                new AzureEyesSilverDragonCard(),
                new DragonShrineCard(),
                new MulcharmyFuwalosCard(),
                new DragonSpiritOfWhiteCard(),
                new MiracleFusionCard(),
                new TheMelodyOfAwakeningDragonCard(),
                new TheBlackGoatLaughsCard(),
                new SkyStrikerAirspaceAreaZeroCard(),
                new ElementalHEROShadowMistCard(),
                new CrystalWingSynchroDragonCard(),
                new GamecielTheSeaTurtleKaijuCard(),
                new ElementalHEROAquaNeosCard(),
                new ElementalHEROShiningNeosWingmanCard(),
                new SkyStrikerAceAzaleaTemperanceCard(),
                new XtraHEROCrossCrusaderCard(),
                new MaskedHERODarkLawCard(),
                new BlueEyesSpiritDragonCard(),
                new DestinyHERODestroyerPhoenixEnforcerCard(),
                new SkyStrikerAceCamelliaCard(),
                new ElementalHEROBlazemanCard(),
                new SkyStrikerMobilizeEngageCard(),
                new SkyStrikerAceKagariCard(),
                new PhoenixWingWindBlastCard(),
                new Number38HopeHarbingerDragonTitanicGalaxyCard(),
                new HieraticSunDragonOverlordOfHeliopolisCard(),
                new CrossoutDesignatorCard(),
                new Number60DugaresTheTimelessCard(),
                new UpstartGoblinCard(),
                new TheWhiteStoneOfAncientsCard(),
                new KarmaCutCard(),
                new TerraformingCard(),
                new GhostBelleHauntedMansionCard(),
                new FavoriteContactCard(),
                new SkyStrikerAceZekeCard(),
                new SkyStrikerAceZeroCard(),
                new TheWhiteStoneOfLegendCard(),
                new FoolishBurialCard(),
                new DrawbreadCard(),
                new StardustSparkDragonCard(),
                new BlueEyesWhiteDragonCard(),
                new ElementalHERONeosCard(),
                new SkyStrikerAceShizukuCard(),
                new ElementalHEROFlameWingmanInfernalRageCard(),
                new DrollLockBirdCard(),
                new EffectVeilerCard(),
                new SkyStrikerMechaWidowAnchorCard(),
                new SkyStrikerAceAzaleaCard(),
            });
    }
}
