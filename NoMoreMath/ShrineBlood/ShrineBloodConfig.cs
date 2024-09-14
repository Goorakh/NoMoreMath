using BepInEx.Configuration;
using NoMoreMath.Config;

namespace NoMoreMath.ShrineBlood
{
    public class ShrineBloodConfig
    {
        public readonly ConfigEntry<bool> EnableGoldGainedDisplay;

        public const string GOLD_GAINED_DISPLAY_AMOUNT_TAG = "{amount}";
        public readonly TagReplacementStringConfig GoldGainedDisplayFormatter;

        public ShrineBloodConfig(in ConfigContext context)
        {
            EnableGoldGainedDisplay = context.Bind("Display Gold Amount", true, new ConfigDescription("If the amount of money that will be granted from a Blood Shrine is displayed"));

            const string GOLD_GAINED_DISPLAY_FORMAT_DEFAULT = $"(+<style=cShrine>${GOLD_GAINED_DISPLAY_AMOUNT_TAG}</style>)";
            ConfigEntry<string> goldGainedDisplayFormatConfig = context.Bind("Gold Amount Display Format", GOLD_GAINED_DISPLAY_FORMAT_DEFAULT, new ConfigDescription($"""
                The format that will be used to display the amount of money gained.
                
                All instances of '{GOLD_GAINED_DISPLAY_AMOUNT_TAG}' (braces included!) will be replaced with the amount of money gained.
                """));
            GoldGainedDisplayFormatter = new TagReplacementStringConfig(goldGainedDisplayFormatConfig, [GOLD_GAINED_DISPLAY_AMOUNT_TAG]);
        }
    }
}
