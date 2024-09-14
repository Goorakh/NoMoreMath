using BepInEx.Configuration;
using NoMoreMath.Config;

namespace NoMoreMath.EffectiveCost
{
    public class EffectiveCostConfig
    {
        public readonly ConfigEntry<bool> Enabled;

        public const string EFFECTIVE_COST_FORMAT_VALUE_TAG = "{value}";
        public readonly TagReplacementStringConfig CostDisplayFormat;

        public EffectiveCostConfig(in ConfigContext context)
        {
            Enabled = context.Bind("Enabled", true, new ConfigDescription("If effective cost should be shown on interactables"));

            const string COST_DISPLAY_FORMAT_DEFAULT = $"<size=75%>(Eff. {EFFECTIVE_COST_FORMAT_VALUE_TAG})</size>";
            ConfigEntry<string> costDisplayFormatConfig = context.Bind("Display Format", COST_DISPLAY_FORMAT_DEFAULT, new ConfigDescription($"""
                The format that will be used to display the effective cost.

                All instances of '{EFFECTIVE_COST_FORMAT_VALUE_TAG}' (braces included!) will be replaced with the effective cost value
                """));

            CostDisplayFormat = new TagReplacementStringConfig(costDisplayFormatConfig, [EFFECTIVE_COST_FORMAT_VALUE_TAG]);
        }
    }
}
