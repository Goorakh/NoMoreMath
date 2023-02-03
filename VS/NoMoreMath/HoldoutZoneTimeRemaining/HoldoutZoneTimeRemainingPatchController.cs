using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.UI;
using System;
using System.Text;
using UnityEngine;

namespace NoMoreMath.HoldoutZoneTimeRemaining
{
    static class HoldoutZoneTimeRemainingPatchController
    {
        static string getChargeTimeRemainingString(HoldoutZoneController holdoutZoneController)
        {
            if (holdoutZoneController)
            {
                HoldoutZoneChargeRateTracker chargeRateTracker = holdoutZoneController.gameObject.GetOrAddComponent<HoldoutZoneChargeRateTracker>();

                float chargeRate = chargeRateTracker.PositiveChargeRate;
                if (chargeRate > 0f)
                {
                    float remainingCharge = 1f - holdoutZoneController.charge;
                    float remainingTime = remainingCharge / chargeRate;

                    StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();

                    stringBuilder.Append(" (").Append(remainingTime.ToString("F1")).Append(" s)");

                    string result = stringBuilder.ToString();
                    HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
                    return result;
                }
                else
                {
                    return " (N/A s)";
                }
            }

            return string.Empty;
        }

        public static void Apply()
        {
            On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;
            IL.RoR2.HoldoutZoneController.FixedUpdate += HoldoutZoneController_FixedUpdate;

            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.GenerateString += ChargeHoldoutZoneObjectiveTracker_GenerateString;
            //On.RoR2.UI.ChargeIndicatorController.Update += ChargeIndicatorController_Update;
        }

        public static void Cleanup()
        {
            On.RoR2.HoldoutZoneController.Awake -= HoldoutZoneController_Awake;
            IL.RoR2.HoldoutZoneController.FixedUpdate -= HoldoutZoneController_FixedUpdate;

            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.GenerateString -= ChargeHoldoutZoneObjectiveTracker_GenerateString;
            //On.RoR2.UI.ChargeIndicatorController.Update -= ChargeIndicatorController_Update;
        }

        static void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, HoldoutZoneController self)
        {
            orig(self);
            self.gameObject.GetOrAddComponent<HoldoutZoneChargeRateTracker>();
        }

        static void HoldoutZoneController_FixedUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            int chargeRateLocalIndex = -1;

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.calcChargeRate)),
                              x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(HoldoutZoneController), nameof(HoldoutZoneController.charge))),
                              x => x.MatchLdloc(out chargeRateLocalIndex),
                              x => x.MatchCallOrCallvirt(AccessTools.DeclaredPropertyGetter(typeof(Time), nameof(Time.fixedDeltaTime))),
                              x => x.MatchMul(),
                              x => x.MatchAdd()))
            {
                foundCursors[1].Emit(OpCodes.Ldarg_0);
                foundCursors[1].Emit(OpCodes.Ldloc, chargeRateLocalIndex);
                foundCursors[1].EmitDelegate((HoldoutZoneController holdoutZoneController, float chargeRate) =>
                {
                    if (holdoutZoneController.TryGetComponent(out HoldoutZoneChargeRateTracker chargeRateTracker))
                    {
                        chargeRateTracker.RecordChargeRate(chargeRate);
                    }
                });

#if DEBUG
                Log.Debug(il.ToString());
#endif
            }
            else
            {
                Log.Warning("unable to find patch location");
            }
        }

        static string ChargeHoldoutZoneObjectiveTracker_GenerateString(On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.orig_GenerateString orig, ObjectivePanelController.ObjectiveTracker self)
        {
            HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker chargeHoldoutObjective = (HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker)self;
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
            HoldoutZoneController holdoutZoneController = chargeHoldoutObjective.holdoutZoneController;
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

            return orig(self) + getChargeTimeRemainingString(holdoutZoneController);
        }

        static void ChargeIndicatorController_Update(On.RoR2.UI.ChargeIndicatorController.orig_Update orig, ChargeIndicatorController self)
        {
            orig(self);

            if (self && self.chargingText && self.chargingText.enabled)
            {
                string timeRemainingString = getChargeTimeRemainingString(self.holdoutZoneController);
                if (!string.IsNullOrEmpty(timeRemainingString))
                {
                    self.chargingText.text += timeRemainingString;
                }
            }
        }
    }
}
