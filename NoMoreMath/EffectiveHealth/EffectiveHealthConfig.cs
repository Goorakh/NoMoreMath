using BepInEx.Configuration;
using NoMoreMath.Config;

namespace NoMoreMath.EffectiveHealth
{
    public class EffectiveHealthConfig
    {
        public readonly ConfigEntry<bool> Enabled;

        public const string EFFECTIVE_HEALTH_FORMAT_VALUE_TAG = "{value}";
        public readonly TagReplacementStringConfig EffectiveHealthDisplayFormat;

        public readonly EffectiveHealthDisplayConfig CurrentHealthDisplay;

        public readonly EffectiveHealthDisplayConfig MaxHealthDisplay;

        public EffectiveHealthConfig(in ConfigContext context)
        {
            Enabled = context.Bind("Enabled", true, new ConfigDescription("If effective health should be displayed."));

            const string EFFECTIVE_HEALTH_FORMAT_DEFAULT = $"<size=65%>(Eff. {EFFECTIVE_HEALTH_FORMAT_VALUE_TAG})</size>";
            ConfigEntry<string> effectiveHealthDisplayFormatConfig = context.Bind("Display Format", EFFECTIVE_HEALTH_FORMAT_DEFAULT, new ConfigDescription($"""
                The format that will be used to display effective health.

                All instances of '{EFFECTIVE_HEALTH_FORMAT_VALUE_TAG}' (braces included!) will be replaced with the effective health value
                """));
            EffectiveHealthDisplayFormat = new TagReplacementStringConfig(effectiveHealthDisplayFormatConfig, [EFFECTIVE_HEALTH_FORMAT_VALUE_TAG]);

            ConfigContext currentHealthDisplayContext = context;
            currentHealthDisplayContext.NameFormat = "Current Health {0}";
            CurrentHealthDisplay = new EffectiveHealthDisplayConfig(currentHealthDisplayContext, this, EffectiveHealthDisplayConfig.DisplayPosition.BeforeHealth);

            ConfigContext maxHealthDisplayContext = context;
            maxHealthDisplayContext.NameFormat = "Max Health {0}";
            MaxHealthDisplay = new EffectiveHealthDisplayConfig(maxHealthDisplayContext, this, EffectiveHealthDisplayConfig.DisplayPosition.AfterHealth);
        }
    }
}
