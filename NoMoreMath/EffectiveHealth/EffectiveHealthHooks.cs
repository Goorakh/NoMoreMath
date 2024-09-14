using RoR2;

namespace NoMoreMath.EffectiveHealth
{
    static class EffectiveHealthHooks
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.HealthComponent.Awake += HealthComponent_Awake;
        }

        static void HealthComponent_Awake(On.RoR2.HealthComponent.orig_Awake orig, HealthComponent self)
        {
            orig(self);

            self.gameObject.AddComponent<EffectiveHealthProvider>();
        }
    }
}
