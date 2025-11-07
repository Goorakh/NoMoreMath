using BepInEx.Configuration;
using NoMoreMath.Config;

namespace NoMoreMath.HoldoutZone
{
    public sealed class HoldoutZoneConfig
    {
        public readonly ConfigEntry<bool> EnableChargeTimeDisplay;

        public const string TIME_REMAINING_DISPLAY_SECONDS_TAG = "{sec}";
        public readonly TagReplacementStringConfig TimeRemainingDisplayFormat;

        public HoldoutZoneConfig(in ConfigContext context)
        {
            EnableChargeTimeDisplay = context.Bind("Display Estimated Charge Time", true, new ConfigDescription("If the estimated time remaining to fully charge a holdout zone should be displayed in the Objective panel"));

            const string TIME_REMAINING_DISPLAY_DEFAULT_VALUE = $"<nobr>({TIME_REMAINING_DISPLAY_SECONDS_TAG} s)</nobr>";
            ConfigEntry<string> timeRemainingDisplayConfig = context.Bind("Charge Time Display Format", TIME_REMAINING_DISPLAY_DEFAULT_VALUE, new ConfigDescription($"""
                The format that will be used to display the time remaining.
                
                All instances of '{TIME_REMAINING_DISPLAY_SECONDS_TAG}' (braces included!) will be replaced with the time remaining (in seconds)
                """));
            TimeRemainingDisplayFormat = new TagReplacementStringConfig(timeRemainingDisplayConfig, [TIME_REMAINING_DISPLAY_SECONDS_TAG]);
        }
    }
}
