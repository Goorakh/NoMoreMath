using BepInEx.Configuration;
using NoMoreMath.Config;
using System.Runtime.CompilerServices;
using System.Text;

namespace NoMoreMath.EffectiveHealth
{
    public class EffectiveHealthDisplayConfig
    {
        public enum DisplayPosition
        {
            None,
            BeforeHealth,
            AfterHealth
        }

        public readonly EffectiveHealthConfig EffectiveHealthConfig;

        public readonly ConfigEntry<DisplayPosition> HealthDisplayPosition;

        public EffectiveHealthDisplayConfig(in ConfigContext context, EffectiveHealthConfig effectiveHealthConfig, DisplayPosition defaultDisplayPosition)
        {
            EffectiveHealthConfig = effectiveHealthConfig;

            HealthDisplayPosition = context.Bind("Display Position", defaultDisplayPosition, new ConfigDescription($"""
                How this effective health should be displayed.

                {nameof(DisplayPosition.None)}: Effective health is not displayed.
                
                {nameof(DisplayPosition.BeforeHealth)}: Effective health is displayed to the left of the health value.
                
                {nameof(DisplayPosition.AfterHealth)}: Effective health is displayed to the right of the health value.
                """));
        }

        public bool ShouldDisplay()
        {
            return EffectiveHealthConfig.Enabled.Value && HealthDisplayPosition.Value != DisplayPosition.None;
        }

        public string Format(float displayHealth, float displayEffectiveHealth)
        {
            string displayHealthString = displayHealth.ToString();
            if (float.IsNaN(displayEffectiveHealth) || !ShouldDisplay())
                return displayHealthString;

            string effectiveHealthString;
            if (float.IsInfinity(displayEffectiveHealth))
            {
                effectiveHealthString = "<color=#555555>INF.</color>";
            }
            else
            {
                effectiveHealthString = displayEffectiveHealth.ToString();
            }

            TagReplacementStringConfig effectiveHealthFormatter = EffectiveHealthConfig.EffectiveHealthDisplayFormat;

            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            stringBuilder.EnsureCapacity(effectiveHealthFormatter.StrippedLength + effectiveHealthString.Length + displayHealthString.Length + 1);

            DisplayPosition position = HealthDisplayPosition.Value;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void appendEffectiveHealth()
            {
                effectiveHealthFormatter.AppendToStringBuilder(stringBuilder, new TagReplacementStringConfig.ReplacementInfo(EffectiveHealthConfig.EFFECTIVE_HEALTH_FORMAT_VALUE_TAG, effectiveHealthString));
            }

            if (position == DisplayPosition.BeforeHealth)
            {
                appendEffectiveHealth();
                stringBuilder.Append(' ');
            }

            stringBuilder.Append(displayHealthString);

            if (position == DisplayPosition.AfterHealth)
            {
                stringBuilder.Append(' ');
                appendEffectiveHealth();
            }

            string formatted = stringBuilder.ToString();
            stringBuilder = HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return formatted;
        }
    }
}
