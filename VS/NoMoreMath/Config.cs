using BepInEx.Configuration;
using NoMoreMath.Utility;
using System.IO;

namespace NoMoreMath
{
    public class Config
    {
        public static ConfigEntry<string> BloodShrineMoney;
        public static ConfigEntry<string> ChanceShrineUses;
        public static ConfigEntry<string> EffectiveHealth;
        public static ConfigEntry<string> EffectiveFullHealth;
        public static ConfigEntry<string> EffectivePurchaseCost;
        public static ConfigEntry<string> GoldShoresBeacons;
        public static ConfigEntry<string> HoldoutZoneTimeRemaining;

        public static void Init(string configPath)
        {
            ConfigFile config = new ConfigFile(Path.Combine(configPath, Main.PluginGUID + ".cfg"), true);
            BloodShrineMoney = config.Bind("Formatting", "Blood Shrine Money String", "(+<style=cShrine>${amount}</style>)", "{amount} will be replaced. Set to blank to disable.");
            ChanceShrineUses = config.Bind("Formatting", "Chance Shrine Uses String", "({amount} activation{s})", "{amount}, {s} will be replaced. Set to blank to disable.");
            EffectiveHealth = config.Bind("Formatting", "Effective Health String", "<size=18>(Eff. {amount})</size>", "{amount} will be replaced. Set to blank to disable.");
            EffectiveFullHealth = config.Bind("Formatting", "Effective Full Health String", "<size=18>(Eff. {amount})</size>", "{amount}, {relative} will be replaced. Set to blank to disable.");
            EffectivePurchaseCost = config.Bind("Formatting", "Effective Purchase Cost String", "{Eff. {styleOnlyOnTooltip}${amount}{/styleOnlyOnTooltip}}", "{amount}, {relative}, {styleOnlyOnTooltip}, {/styleOnlyOnTooltip} will be replaced. Set to blank to disable.");
            GoldShoresBeacons = config.Bind("Formatting", "Gilded Coast Beacons String", "(<color={color}>${amount}</color>)", "{color}, {amount} will be replaced. Set to blank to disable.");
            HoldoutZoneTimeRemaining = config.Bind("Formatting", "Holdout Zone Time Remaining String", "({second} s)", "{second} will be replaced. Set to blank to disable.");

            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(Constants.RISK_OF_OPTIONS_GUID))
            {
#if DEBUG
                Log.Debug("Risk of Options detected, running compat...");
#endif

                RiskOfOptionsCompatibility.CreateOptions();
            }
#if DEBUG
            else
            {
                Log.Debug("Risk of Options is not installed");
            }
#endif
        }
    }
}
