using BepInEx.Configuration;
using NoMoreMath.Config;

namespace NoMoreMath.HalcyonShrine
{
    public class HalcyonShrineConfig
    {
        public readonly ConfigEntry<bool> EnableProgressObjective;

        public const string CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG = "{percent_complete}";
        public readonly TagReplacementStringConfig ChargeProgressDisplayFormat;

        public HalcyonShrineConfig(in ConfigContext context)
        {
            EnableProgressObjective = context.Bind("Display Charge Progress", true, new ConfigDescription("If the charge progress of Halcyon Shrines should be displayed in the Objectives panel"));

            const string CHARGE_PROGRESS_DISPLAY_DEFAULT = $"Charge the <style=cShrine>Halcyon Shrine</style> ({CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG}%)";
            ConfigEntry<string> chargeProgressDisplayFormatConfig = context.Bind("Charge Progress Display Format", CHARGE_PROGRESS_DISPLAY_DEFAULT, new ConfigDescription($"""
                The format that will be used to display the charge progress.
                
                All instances of '{CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG}' (braces included!) will be replaced with the charge percentage (0-100)
                """));
            ChargeProgressDisplayFormat = new TagReplacementStringConfig(chargeProgressDisplayFormatConfig, [CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG]);
        }
    }
}
