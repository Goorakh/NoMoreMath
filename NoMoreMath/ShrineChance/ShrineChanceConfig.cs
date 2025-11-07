using BepInEx.Configuration;
using NoMoreMath.Config;

namespace NoMoreMath.ShrineChance
{
    public sealed class ShrineChanceConfig
    {
        public readonly ConfigEntry<bool> EnableActivationCountDisplay;

        public const string ACTIVATION_COUNT_DISPLAY_COUNT_TAG = "{activations}";
        public readonly TagReplacementStringConfig ActivationCountDisplayFormatter;

        public ShrineChanceConfig(in ConfigContext context)
        {
            EnableActivationCountDisplay = context.Bind("Display Activation Count", true, new ConfigDescription("Displays how many activations of a shrine you can afford."));

            const string ACTIVATION_COUNT_DISPLAY_FORMAT_DEFAULT = $"<size=80%><nobr>({ACTIVATION_COUNT_DISPLAY_COUNT_TAG} activation(s))</nobr></size>";
            ConfigEntry<string> activationCountDisplayFormatConfig = context.Bind("Shrine Activation Count Display Format", ACTIVATION_COUNT_DISPLAY_FORMAT_DEFAULT, new ConfigDescription($"""
                The format that will be used to display the affordable activations remaining.
                
                All instances of '{ACTIVATION_COUNT_DISPLAY_COUNT_TAG}' (braces included!) will be replaced with the number of times you can purchase the shrine before running out of money.
                """));
            ActivationCountDisplayFormatter = new TagReplacementStringConfig(activationCountDisplayFormatConfig, [ACTIVATION_COUNT_DISPLAY_COUNT_TAG]);
        }
    }
}
