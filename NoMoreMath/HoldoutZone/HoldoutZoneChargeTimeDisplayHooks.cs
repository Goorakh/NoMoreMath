using NoMoreMath.Config;
using RoR2;
using RoR2.UI;
using System.Globalization;
using System.Text;

namespace NoMoreMath.HoldoutZone
{
    internal static class HoldoutZoneChargeTimeDisplayHooks
    {
        [SystemInitializer]
        private static void Init()
        {
            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.GenerateString += ChargeHoldoutZoneObjectiveTracker_GenerateString;

            On.RoR2.UI.WakeMissionObjectiveTracker.GenerateString += WakeMissionObjectiveTracker_GenerateString;
            On.RoR2.UI.WakeMissionObjectiveTracker.IsDirty += WakeMissionObjectiveTracker_IsDirty;
        }

        private static bool WakeMissionObjectiveTracker_IsDirty(On.RoR2.UI.WakeMissionObjectiveTracker.orig_IsDirty orig, WakeMissionObjectiveTracker self)
        {
            return orig(self) || Configs.HoldoutZone.EnableChargeTimeDisplay.Value;
        }

        private static string WakeMissionObjectiveTracker_GenerateString(On.RoR2.UI.WakeMissionObjectiveTracker.orig_GenerateString orig, WakeMissionObjectiveTracker self)
        {
            string objectiveString = orig(self);

            HoldoutZoneConfig holdoutZoneConfig = Configs.HoldoutZone;
            if (holdoutZoneConfig.EnableChargeTimeDisplay.Value)
            {
                HoldoutZoneController holdoutZone = self.wakeController ? self.wakeController.holdoutZoneController : null;
                if (holdoutZone)
                {
                    AppendHoldoutZoneChargeTime(ref objectiveString, holdoutZoneConfig, holdoutZone);
                }
            }

            return objectiveString;
        }

        private static string ChargeHoldoutZoneObjectiveTracker_GenerateString(On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.orig_GenerateString orig, ObjectivePanelController.ObjectiveTracker _self)
        {
            HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker self = (HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker)_self;

            string objectiveString = orig(self);

            HoldoutZoneConfig holdoutZoneConfig = Configs.HoldoutZone;
            if (holdoutZoneConfig.EnableChargeTimeDisplay.Value)
            {
                HoldoutZoneController holdoutZone = self.holdoutZoneController;
                if (holdoutZone)
                {
                    AppendHoldoutZoneChargeTime(ref objectiveString, holdoutZoneConfig, holdoutZone);
                }
            }

            return objectiveString;
        }

        private static void AppendHoldoutZoneChargeTime(ref string objectiveString, HoldoutZoneConfig holdoutZoneConfig, HoldoutZoneController holdoutZone)
        {
            float chargeRate = 0f;
            if (HoldoutZoneChargeRateProvider.ComponentCache.TryGetComponent(holdoutZone, out HoldoutZoneChargeRateProvider chargeRateProvider))
            {
                chargeRate = chargeRateProvider.LastPositiveChargeRate;
            }

            using var _ = HG.StringBuilderPool.RentStringBuilder(out StringBuilder stringBuilder);

            string timeRemainingString = "N/A";
            if (chargeRate > 0f)
            {
                float estimatedTimeRemaining = (1f - holdoutZone.charge) / chargeRate;

                timeRemainingString = estimatedTimeRemaining.ToString("F1", CultureInfo.InvariantCulture);
            }

            stringBuilder.EnsureCapacity(objectiveString.Length + holdoutZoneConfig.TimeRemainingDisplayFormat.StrippedLength + timeRemainingString.Length + 1);

            stringBuilder.Append(objectiveString);

            stringBuilder.Append(' ');

            holdoutZoneConfig.TimeRemainingDisplayFormat.AppendToStringBuilder(stringBuilder, new TagReplacementStringConfig.ReplacementInfo(HoldoutZoneConfig.TIME_REMAINING_DISPLAY_SECONDS_TAG, timeRemainingString));

            objectiveString = stringBuilder.ToString();
        }
    }
}
