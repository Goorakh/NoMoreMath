using RoR2;

namespace NoMoreMath.HalcyonShrine
{
    static class HalcyonShrineObjectiveProviderHooks
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.HalcyoniteShrineInteractable.Awake += HalcyoniteShrineInteractable_Awake;
        }

        static void HalcyoniteShrineInteractable_Awake(On.RoR2.HalcyoniteShrineInteractable.orig_Awake orig, HalcyoniteShrineInteractable self)
        {
            orig(self);

            self.gameObject.AddComponent<HalcyonShrineObjectiveProvider>();
        }
    }
}
