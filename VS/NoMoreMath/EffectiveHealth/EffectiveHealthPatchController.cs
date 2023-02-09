using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using NoMoreMath.Utility.Extensions;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace NoMoreMath.EffectiveHealth
{
    static class EffectiveHealthPatchController
    {
        static string formatEffectiveHealthString(float effectiveHealth, bool maxHealth = false)
        {
            string effectiveHealthString;
            if (!float.IsPositiveInfinity(effectiveHealth)) effectiveHealthString = Mathf.CeilToInt(effectiveHealth).ToString();
            else effectiveHealthString = "<color=#555555>INF.</color>";
            return (maxHealth ? Config.EffectiveFullHealth.Value : Config.EffectiveHealth.Value).Replace("{amount}", effectiveHealthString);
        }

        public static void Apply()
        {
            IL.RoR2.UI.HealthBar.UpdateHealthbar += HealthBar_UpdateHealthbar;
            On.RoR2.UI.HealthBar.Awake += HealthBar_Awake;
        }

        public static void Cleanup()
        {
            IL.RoR2.UI.HealthBar.UpdateHealthbar -= HealthBar_UpdateHealthbar;
            On.RoR2.UI.HealthBar.Awake += HealthBar_Awake;
        }

        static void HealthBar_Awake(On.RoR2.UI.HealthBar.orig_Awake orig, HealthBar self)
        {
            orig(self);
            self.gameObject.GetOrAddComponent<EffectiveHealthTracker>();
        }

        static void HealthBar_UpdateHealthbar(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            while (c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt(AccessTools.PropertySetter(typeof(TMP_Text), nameof(TMP_Text.text)))))
            {
                ILCursor[] foundCursors;
                if (!c.TryFindPrev(out foundCursors,
                                   x => x.MatchLdfld(out FieldReference field) && field.FieldType.FullName == typeof(TextMeshProUGUI).FullName))
                {
                    Log.Error($"Unable to find ldfld for {nameof(TextMeshProUGUI)} at {c.Index}");
                    continue;
                }

                FieldReference displayTextField;
                foundCursors[0].Next.MatchLdfld(out displayTextField);

                bool isFullHealth;
                switch (displayTextField.Name)
                {
                    case nameof(HealthBar.fullHealthText):
                        isFullHealth = true;
                        break;
                    case nameof(HealthBar.currentHealthText):
                        isFullHealth = false;
                        break;
                    default:
                        Log.Warning($"Unexpected displayText field: {displayTextField.Name}");
                        continue;
                }

                if (!c.TryFindPrev(out foundCursors,
                                   x => x.MatchLdloc(out _),
                                   x => x.MatchStfld(out FieldReference field) && field.FieldType.FullName == typeof(float).FullName))
                {
                    Log.Error($"Unable to find stfld for currentHealthValue at {c.Index}");
                    continue;
                }

                int newDisplayedHealthValueLocalIndex;
                foundCursors[0].Next.MatchLdloc(out newDisplayedHealthValueLocalIndex);

                FieldReference displayedHealthValueField;
                foundCursors[1].Next.MatchStfld(out displayedHealthValueField);

                if (foundCursors[1].TryFindPrev(out foundCursors,
                                                x => x.MatchLdloc(newDisplayedHealthValueLocalIndex),
                                                x => x.MatchLdfld(displayedHealthValueField),
                                                x => x.MatchBeq(out _)))
                {
                    ILCursor labelCursor = foundCursors[2]; // Beq
                    labelCursor.Index++;
                    ILLabel insideIfLabel = labelCursor.MarkLabel();

                    ILCursor cursor = foundCursors[0];

                    cursor.Emit(OpCodes.Ldarg_0);

                    if (isFullHealth)
                    {
                        cursor.EmitDelegate((HealthBar instance) =>
                        {
                            return instance.TryGetComponent(out EffectiveHealthTracker effectiveHealthTracker) && effectiveHealthTracker.FullHealth.IsDirty;
                        });
                    }
                    else
                    {
                        cursor.EmitDelegate((HealthBar instance) =>
                        {
                            return instance.TryGetComponent(out EffectiveHealthTracker effectiveHealthTracker) && effectiveHealthTracker.CurrentHealth.IsDirty;
                        });
                    }

                    cursor.Emit(OpCodes.Brtrue_S, insideIfLabel);
                }
                else
                {
                    Log.Warning($"Unable to find IsDirty hook location for {displayedHealthValueField.Name}");
                }

                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldfld, displayedHealthValueField);

                c.Emit(OpCodes.Ldarg_0);

                static bool ShouldDisplayEffectiveHealth(HealthBar instance, float displayedHealthValue, bool useFullHealth, out EffectiveHealthTracker.Tracker healthTracker)
                {
                    healthTracker = null;

                    if (!instance)
                        return false;

                    HealthComponent source = instance.source;
                    if (!source)
                        return false;

                    if (!instance.TryGetComponent(out EffectiveHealthTracker effectiveHealthTracker))
                        return false;

                    healthTracker = useFullHealth ? effectiveHealthTracker.FullHealth : effectiveHealthTracker.CurrentHealth;
                    float effectiveHealth = healthTracker.CurrentValue;
                    if (Mathf.Ceil(effectiveHealth) == displayedHealthValue)
                        return false;

                    return true;
                }

                if (isFullHealth)
                {
                    c.EmitDelegate((string text, float displayedHealthValue, HealthBar instance) =>
                    {
                        if (Config.EffectiveFullHealth.Value == null || !ShouldDisplayEffectiveHealth(instance, displayedHealthValue, true, out EffectiveHealthTracker.Tracker effectiveHealthTracker))
                            return text;

                        string effectiveHealthString = formatEffectiveHealthString(effectiveHealthTracker.CurrentValue, true);
                        effectiveHealthTracker.OnRefreshed();
                        return text + " " + effectiveHealthString;
                    });
                }
                else
                {
                    c.EmitDelegate((string text, float displayedHealthValue, HealthBar instance) =>
                    {
                        if (Config.EffectiveHealth.Value == null || !ShouldDisplayEffectiveHealth(instance, displayedHealthValue, false, out EffectiveHealthTracker.Tracker effectiveHealthTracker))
                            return text;

                        string effectiveHealthString = formatEffectiveHealthString(effectiveHealthTracker.CurrentValue);

                        effectiveHealthTracker.OnRefreshed();
                        return effectiveHealthString + " " + text;
                    });
                }

                // Prevent infinite loop
                c.Index++;
            }

#if DEBUG
            Log.Debug(il.ToString());
#endif
        }
    }
}
