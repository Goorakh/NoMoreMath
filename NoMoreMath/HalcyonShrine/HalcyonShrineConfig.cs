using BepInEx.Configuration;
using NoMoreMath.Config;

namespace NoMoreMath.HalcyonShrine
{
    public sealed class HalcyonShrineConfig
    {
        public readonly ConfigEntry<bool> EnableProgressObjective;

        public const string CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG = "{percent_complete}";
        public const string CHARGE_PROGRESS_DISPLAY_GOLD_DRAINED_TAG = "{money_spent}";
        public const string CHARGE_PROGRESS_DISPLAY_GOLD_REQUIRED_TAG = "{money_required}";
        public readonly TagReplacementStringConfig ChargeProgressDisplayFormat;

        public HalcyonShrineConfig(in ConfigContext context)
        {
            EnableProgressObjective = context.Bind("Display Charge Progress", true, new ConfigDescription("If the charge progress of Halcyon Shrines should be displayed in the Objectives panel"));

            const string CHARGE_PROGRESS_DISPLAY_DEFAULT = $"Charge the <style=cHumanObjective>Halcyon Shrine</style> (<style=cShrine>${CHARGE_PROGRESS_DISPLAY_GOLD_DRAINED_TAG}/{CHARGE_PROGRESS_DISPLAY_GOLD_REQUIRED_TAG}</style>)";
            ConfigEntry<string> chargeProgressDisplayFormatConfig = context.Bind("Charge Progress Display Format", CHARGE_PROGRESS_DISPLAY_DEFAULT, new ConfigDescription($"""
                The format that will be used to display the charge progress.

                All instances of '{CHARGE_PROGRESS_DISPLAY_GOLD_DRAINED_TAG}' (braces included!) will be replaced with the amount of money spent on the shrine
                
                All instances of '{CHARGE_PROGRESS_DISPLAY_GOLD_REQUIRED_TAG}' (braces included!) will be replaced with the amount of money required to complete the shrine
                
                All instances of '{CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG}' (braces included!) will be replaced with the charge percentage (0-100)
                """));
            ChargeProgressDisplayFormat = new TagReplacementStringConfig(chargeProgressDisplayFormatConfig, [CHARGE_PROGRESS_DISPLAY_PROGRESS_TAG, CHARGE_PROGRESS_DISPLAY_GOLD_DRAINED_TAG, CHARGE_PROGRESS_DISPLAY_GOLD_REQUIRED_TAG]);
        }
    }
}
