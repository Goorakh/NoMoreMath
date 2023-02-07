using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoMoreMath.Utility.Extensions;
using RoR2;
using RoR2.UI;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
                    
                    int precision;
                    if (NetworkServer.active)
                    {
                        precision = 1;
                    }
                    else
                    {
                        remainingTime = Mathf.Round(remainingTime);
                        precision = 0;
                    }

                    stringBuilder.Append(" (").Append(remainingTime.ToString($"F{precision}")).Append(" s)");

                    return stringBuilder.GetAndReturnToPool();
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
            On.RoR2.HoldoutZoneController.OnEnable += HoldoutZoneController_OnEnable;

            IL.RoR2.HoldoutZoneController.FixedUpdate += HoldoutZoneController_FixedUpdate;

            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.GenerateString += ChargeHoldoutZoneObjectiveTracker_GenerateString;
            //On.RoR2.UI.ChargeIndicatorController.Update += ChargeIndicatorController_Update;

            IL.RoR2.HoldoutZoneController.OnDeserialize += HoldoutZoneController_OnDeserialize;
        }

        public static void Cleanup()
        {
            On.RoR2.HoldoutZoneController.Awake -= HoldoutZoneController_Awake;
            On.RoR2.HoldoutZoneController.OnEnable -= HoldoutZoneController_OnEnable;

            IL.RoR2.HoldoutZoneController.FixedUpdate -= HoldoutZoneController_FixedUpdate;

            On.RoR2.HoldoutZoneController.ChargeHoldoutZoneObjectiveTracker.GenerateString -= ChargeHoldoutZoneObjectiveTracker_GenerateString;
            //On.RoR2.UI.ChargeIndicatorController.Update -= ChargeIndicatorController_Update;

            IL.RoR2.HoldoutZoneController.OnDeserialize -= HoldoutZoneController_OnDeserialize;
        }

        static void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, HoldoutZoneController self)
        {
            orig(self);
            self.gameObject.GetOrAddComponent<HoldoutZoneChargeRateTracker>();
        }

        static void HoldoutZoneController_OnEnable(On.RoR2.HoldoutZoneController.orig_OnEnable orig, HoldoutZoneController self)
        {
            orig(self);

            if (self.TryGetComponent(out HoldoutZoneChargeRateTracker chargeRateTracker))
            {
                chargeRateTracker.OnHoldoutZoneStarted();
            }
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

        static void HoldoutZoneController_OnDeserialize(ILContext il)
        {
            ILCursor c = new ILCursor(il);

#pragma warning disable Publicizer001 // Accessing a member that was not originally public
            const string CHARGE_FIELD_NAME = nameof(HoldoutZoneController._charge);
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

            while (c.TryGotoNext(MoveType.Before, x => x.MatchStfld<HoldoutZoneController>(CHARGE_FIELD_NAME)))
            {
                c.Emit(OpCodes.Dup);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((float charge, HoldoutZoneController instance) =>
                {
                    if (instance.TryGetComponent(out HoldoutZoneChargeRateTracker chargeRateTracker))
                    {
                        chargeRateTracker.OnChargeReceived(charge);
                    }
                });

                c.Index++;
            }
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
