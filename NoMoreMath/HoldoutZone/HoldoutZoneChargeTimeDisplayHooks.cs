using NoMoreMath.Config;
using RoR2;
using RoR2.UI;
using System;
using System.Globalization;
using System.Text;

namespace NoMoreMath.HoldoutZone
{
    static class HoldoutZoneChargeTimeDisplayHooks
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.GenerateString += ChargeHoldoutZoneObjectiveTracker_GenerateString;
        }

        static string ChargeHoldoutZoneObjectiveTracker_GenerateString(On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.orig_GenerateString orig, ObjectivePanelController.ObjectiveTracker _self)
        {
            HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker self = (HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker)_self;

            string objectiveString = orig(self);

            HoldoutZoneConfig holdoutZoneConfig = Configs.HoldoutZone;
            if (holdoutZoneConfig.EnableChargeTimeDisplay.Value)
            {
                HoldoutZoneController holdoutZone = self.holdoutZoneController;

                float chargeRate = 0f;
                if (HoldoutZoneChargeRateProvider.ComponentCache.TryGetComponent(holdoutZone, out HoldoutZoneChargeRateProvider chargeRateProvider))
                {
                    chargeRate = chargeRateProvider.LastPositiveChargeRate;
                }

                StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();

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
                stringBuilder = HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            }

            return objectiveString;
        }
    }
}
