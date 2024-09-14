using RoR2;

namespace NoMoreMath.HalcyonBeacon
{
    static class GoldshoresMissionControllerBeaconsProviderHooks
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.GoldshoresMissionController.Awake += GoldshoresMissionController_Awake;
        }

        static void GoldshoresMissionController_Awake(On.RoR2.GoldshoresMissionController.orig_Awake orig, GoldshoresMissionController self)
        {
            orig(self);

            GoldshoresMissionControllerBeaconsProvider.TryAddComponent(self);
        }
    }
}
