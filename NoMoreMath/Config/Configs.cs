using BepInEx.Configuration;
using NoMoreMath.EffectiveCost;
using NoMoreMath.EffectiveHealth;
using NoMoreMath.HalcyonBeacon;
using NoMoreMath.HalcyonShrine;
using NoMoreMath.HoldoutZone;
using NoMoreMath.ShrineBlood;
using NoMoreMath.ShrineChance;

namespace NoMoreMath.Config
{
    public static class Configs
    {
        public static EffectiveHealthConfig EffectiveHealth { get; private set; }

        public static EffectiveCostConfig EffectiveCost { get; private set; }

        public static HoldoutZoneConfig HoldoutZone { get; private set; }

        public static HalcyonShrineConfig HalcyonShrine { get; private set; }

        public static HalcyonBeaconConfig HalcyonBeacon { get; private set; }

        public static ShrineChanceConfig ShrineChance { get; private set; }

        public static ShrineBloodConfig ShrineBlood { get; private set; }

        internal static void Init(ConfigFile file)
        {
            ConfigContext configContext = new ConfigContext
            {
                File = file
            };

            ConfigContext effectiveHealthContext = configContext;
            effectiveHealthContext.SectionName = "Effective Health";
            EffectiveHealth = new EffectiveHealthConfig(effectiveHealthContext);

            ConfigContext effectiveCostContext = configContext;
            effectiveCostContext.SectionName = "Effective Cost";
            EffectiveCost = new EffectiveCostConfig(effectiveCostContext);

            ConfigContext holdoutZoneContext = configContext;
            holdoutZoneContext.SectionName = "Holdout Zone";
            HoldoutZone = new HoldoutZoneConfig(holdoutZoneContext);

            ConfigContext halcyonShrineContext = configContext;
            halcyonShrineContext.SectionName = "Halcyon Shrine";
            HalcyonShrine = new HalcyonShrineConfig(halcyonShrineContext);

            ConfigContext halcyonBeaconContext = configContext;
            halcyonBeaconContext.SectionName = "Halcyon Beacon";
            HalcyonBeacon = new HalcyonBeaconConfig(halcyonBeaconContext);

            ConfigContext shrineChanceContext = configContext;
            shrineChanceContext.SectionName = "Chance Shrine";
            ShrineChance = new ShrineChanceConfig(shrineChanceContext);

            ConfigContext bloodChanceContext = configContext;
            bloodChanceContext.SectionName = "Blood Shrine";
            ShrineBlood = new ShrineBloodConfig(bloodChanceContext);
        }
    }
}
