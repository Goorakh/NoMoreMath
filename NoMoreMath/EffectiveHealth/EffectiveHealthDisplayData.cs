using NoMoreMath.Utils;
using System;

namespace NoMoreMath.EffectiveHealth
{
    public sealed class EffectiveHealthDisplayData : IDisposable
    {
        public readonly TrackedValue<float> LastDisplayedHealthValue = new TrackedValue<float>(float.NaN);

        public readonly TrackedValue<float> LastDisplayValue = new TrackedValue<float>(float.NaN);

        readonly EffectiveHealthDisplayConfig _displayConfig;

        public bool IsDirty;

        public EffectiveHealthDisplayData(EffectiveHealthDisplayConfig displayConfig)
        {
            _displayConfig = displayConfig ?? throw new ArgumentNullException(nameof(displayConfig));

            _displayConfig.EffectiveHealthConfig.Enabled.SettingChanged += onDisplaySettingChanged;
            _displayConfig.EffectiveHealthConfig.EffectiveHealthDisplayFormat.ConfigEntry.SettingChanged += onDisplaySettingChanged;

            _displayConfig.HealthDisplayPosition.SettingChanged += onDisplaySettingChanged;
        }

        void onDisplaySettingChanged(object sender, EventArgs e)
        {
            IsDirty = true;
        }

        public void Dispose()
        {
            IsDirty = false;

            _displayConfig.EffectiveHealthConfig.Enabled.SettingChanged -= onDisplaySettingChanged;
            _displayConfig.EffectiveHealthConfig.EffectiveHealthDisplayFormat.ConfigEntry.SettingChanged -= onDisplaySettingChanged;

            _displayConfig.HealthDisplayPosition.SettingChanged -= onDisplaySettingChanged;
        }

        public bool ShouldDisplay()
        {
            return _displayConfig.ShouldDisplay();
        }

        public string Format(float displayHealth, float displayEffectiveHealth)
        {
            return _displayConfig.Format(displayHealth, displayEffectiveHealth);
        }
    }
}
