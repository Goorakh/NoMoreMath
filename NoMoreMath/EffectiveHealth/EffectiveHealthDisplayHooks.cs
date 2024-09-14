using NoMoreMath.Config;
using RoR2;
using RoR2.UI;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

namespace NoMoreMath.EffectiveHealth
{
    static class EffectiveHealthDisplayHooks
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.UI.HealthBar.Awake += HealthBar_Awake;
            On.RoR2.UI.HealthBar.UpdateHealthbar += HealthBar_UpdateHealthbar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool isValidForEffectiveHealthDisplay(HealthBar healthBar)
        {
            return healthBar.currentHealthText || healthBar.fullHealthText;
        }

        static void HealthBar_Awake(On.RoR2.UI.HealthBar.orig_Awake orig, HealthBar self)
        {
            orig(self);

            if (!isValidForEffectiveHealthDisplay(self))
                return;

            if (self.spriteAsNumberManager)
            {
                GameObject.Destroy(self.spriteAsNumberManager.gameObject);
            }

            Transform slashTransform = self.transform.Find("Slash");
            if (slashTransform)
            {
                slashTransform.gameObject.SetActive(true);
            }
        }

        static void HealthBar_UpdateHealthbar(On.RoR2.UI.HealthBar.orig_UpdateHealthbar orig, HealthBar self, float deltaTime)
        {
            orig(self, deltaTime);

            if (!isValidForEffectiveHealthDisplay(self))
                return;

            EffectiveHealthDisplayController displayController = EffectiveHealthDisplayController.ComponentCache.GetOrAddComponent(self);
            if (!displayController)
                return;

            EffectiveHealthProvider healthProvider = displayController.HealthProvider;
            if (!healthProvider)
                return;

            updateEffectiveHealthDisplay(self.currentHealthText, self.displayStringCurrentHealth, displayController.HealthDisplayData, healthProvider.EffectiveHealth, Configs.EffectiveHealth.CurrentHealthDisplay);

            updateEffectiveHealthDisplay(self.fullHealthText, self.displayStringFullHealth, displayController.MaxHealthDisplayData, healthProvider.EffectiveMaxHealth, Configs.EffectiveHealth.MaxHealthDisplay);

            void updateEffectiveHealthDisplay(TextMeshProUGUI healthText, float displayHealth, EffectiveHealthDisplayData displayData, float effectiveHealth, EffectiveHealthDisplayConfig displayConfig)
            {
                if (!healthText)
                    return;
                
                float displayEffectiveHealth = Mathf.Ceil(effectiveHealth);
                if (displayEffectiveHealth < 0f || displayEffectiveHealth == displayHealth || !displayData.ShouldDisplay())
                    displayEffectiveHealth = float.NaN;

                bool displayValueDirty = displayData.LastDisplayValue.Push(displayEffectiveHealth);
                bool displayedHealthValueChanged = displayData.LastDisplayedHealthValue.Push(displayHealth);

                if (displayValueDirty || displayedHealthValueChanged || displayData.IsDirty)
                {
                    healthText.text = displayConfig.Format(displayHealth, displayEffectiveHealth);
                    displayData.IsDirty = false;
#if DEBUG
                    Log.Debug($"Refreshing health text '{healthText.name}' for {Util.GetBestBodyName(self.source?.gameObject)}");
#endif
                }
            }
        }
    }
}
