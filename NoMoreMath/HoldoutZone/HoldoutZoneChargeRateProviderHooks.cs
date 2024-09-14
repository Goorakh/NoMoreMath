using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using UnityEngine.Networking;

namespace NoMoreMath.HoldoutZone
{
    static class HoldoutZoneChargeRateProviderHooks
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.HoldoutZoneController.Awake += HoldoutZoneController_Awake;
            IL.RoR2.HoldoutZoneController.DoUpdate += HoldoutZoneController_DoUpdate;
        }

        static void HoldoutZoneController_Awake(On.RoR2.HoldoutZoneController.orig_Awake orig, HoldoutZoneController self)
        {
            orig(self);

            HoldoutZoneChargeRateProvider.ComponentCache.GetOrAddComponent(self);
        }

        static void HoldoutZoneController_DoUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors = null;
            if (!c.TryFindNext(out foundCursors,
                               x => x.MatchLdfld<HoldoutZoneController>(nameof(HoldoutZoneController.calcChargeRate)),
                               x => x.MatchCallOrCallvirt<HoldoutZoneController.CalcChargeRateDelegate>(nameof(HoldoutZoneController.CalcChargeRateDelegate.Invoke))))
            {
                Log.Error("Failed to find calcChargeRate invoke");
                return;
            }

            c.Goto(foundCursors[1].Next);

            int chargeRateLocalIndex = -1;
            if (!c.Clone().TryGotoPrev(x => x.MatchLdloca(out chargeRateLocalIndex)))
            {
                Log.Error("Failed to find chargeRate local");
                return;
            }

            if (!c.TryGotoNext(MoveType.After,
                               x => x.MatchCallOrCallvirt<HoldoutZoneController>("set_" + nameof(HoldoutZoneController.charge))))
            {
                c.Index++;
                Log.Warning("Failed to find set_charge call, using current position");
            }

            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc, chargeRateLocalIndex);
            c.EmitDelegate(recordChargeRate);
            static void recordChargeRate(HoldoutZoneController holdoutZoneController, float chargeRate)
            {
                if (!NetworkServer.active)
                    return;

                HoldoutZoneChargeRateProvider chargeRateProvider = HoldoutZoneChargeRateProvider.ComponentCache.GetOrAddComponent(holdoutZoneController);
                chargeRateProvider.ChargeRate = chargeRate;
            }
        }
    }
}
