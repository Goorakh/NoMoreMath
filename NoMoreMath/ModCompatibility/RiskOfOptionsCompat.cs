using BepInEx.Bootstrap;
using NoMoreMath.Config;
using NoMoreMath.EffectiveCost;
using NoMoreMath.EffectiveHealth;
using NoMoreMath.HalcyonBeacon;
using NoMoreMath.HalcyonShrine;
using NoMoreMath.HoldoutZone;
using NoMoreMath.ShrineBlood;
using NoMoreMath.ShrineChance;
using RiskOfOptions;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NoMoreMath.ModCompatibility
{
    static class RiskOfOptionsCompat
    {
        public static bool Enabled => Chainloader.PluginInfos.ContainsKey(RiskOfOptions.PluginInfo.PLUGIN_GUID);

        const string MOD_GUID = NoMoreMathPlugin.PluginGUID;
        const string MOD_NAME = NoMoreMathPlugin.PluginName;

        static Sprite _iconSprite;

        static void findIconSprite()
        {
            Sprite iconSprite = null;

            FileInfo iconFile = null;

            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(NoMoreMathPlugin.Instance.Info.Location));
            do
            {
                FileInfo[] files = dir.GetFiles("icon.png", SearchOption.TopDirectoryOnly);
                if (files != null && files.Length > 0)
                {
                    iconFile = files[0];
                    break;
                }

                dir = dir.Parent;
            } while (dir != null && !string.Equals(dir.Name, "plugins", StringComparison.OrdinalIgnoreCase));

            if (iconFile != null)
            {
                Texture2D iconTexture = new Texture2D(256, 256);
                iconTexture.name = $"tex{NoMoreMathPlugin.PluginName}Icon";
                if (iconTexture.LoadImage(File.ReadAllBytes(iconFile.FullName)))
                {
                    iconSprite = Sprite.Create(iconTexture, new Rect(0f, 0f, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
                    iconSprite.name = $"{NoMoreMathPlugin.PluginName}Icon";
                }
            }

            _iconSprite = iconSprite;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void Init()
        {
            if (!_iconSprite)
                findIconSprite();

            addEffectiveHealthOptions(Configs.EffectiveHealth);

            addEffectiveCostOptions(Configs.EffectiveCost);

            addHoldoutZoneOptions(Configs.HoldoutZone);

            addHalcyonShrineOptions(Configs.HalcyonShrine);

            addHalcyonBeaconOptions(Configs.HalcyonBeacon);

            addShrineChanceOptions(Configs.ShrineChance);

            addShrineBloodOptions(Configs.ShrineBlood);

            ModSettingsManager.SetModDescription($"Options for {NoMoreMathPlugin.PluginName}", MOD_GUID, MOD_NAME);

            if (_iconSprite)
            {
                ModSettingsManager.SetModIcon(_iconSprite, MOD_GUID, MOD_NAME);
            }
        }

        static void addOption(BaseOption option)
        {
            ModSettingsManager.AddOption(option, MOD_GUID, MOD_NAME);
        }

        static void addEffectiveHealthOptions(EffectiveHealthConfig effectiveHealthConfig)
        {
            addOption(new CheckBoxOption(effectiveHealthConfig.Enabled));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool isDisabledImpl()
            {
                return !effectiveHealthConfig.Enabled.Value;
            }

            BaseOptionConfig.IsDisabledDelegate isDisabled = isDisabledImpl;

            addOption(new StringInputFieldOption(effectiveHealthConfig.EffectiveHealthDisplayFormat.ConfigEntry, new InputFieldConfig
            {
                lineType = TMPro.TMP_InputField.LineType.SingleLine,
                richText = false,
                submitOn = InputFieldConfig.SubmitEnum.OnExit,
                checkIfDisabled = isDisabled
            }));

            addDisplayOptions(effectiveHealthConfig.CurrentHealthDisplay);
            addDisplayOptions(effectiveHealthConfig.MaxHealthDisplay);

            void addDisplayOptions(EffectiveHealthDisplayConfig displayConfig)
            {
                addOption(new ChoiceOption(displayConfig.HealthDisplayPosition, new ChoiceConfig
                {
                    checkIfDisabled = isDisabled
                }));
            }
        }

        static void addEffectiveCostOptions(EffectiveCostConfig effectiveCostConfig)
        {
            addOption(new CheckBoxOption(effectiveCostConfig.Enabled));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool isDisabledImpl()
            {
                return !effectiveCostConfig.Enabled.Value;
            }

            BaseOptionConfig.IsDisabledDelegate isDisabled = isDisabledImpl;

            addOption(new StringInputFieldOption(effectiveCostConfig.CostDisplayFormat.ConfigEntry, new InputFieldConfig
            {
                lineType = TMPro.TMP_InputField.LineType.SingleLine,
                richText = false,
                submitOn = InputFieldConfig.SubmitEnum.OnExit,
                checkIfDisabled = isDisabled
            }));
        }

        static void addHoldoutZoneOptions(HoldoutZoneConfig holdoutZoneConfig)
        {
            addOption(new CheckBoxOption(holdoutZoneConfig.EnableChargeTimeDisplay));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool chargeTimeIsDisabledImpl()
            {
                return !holdoutZoneConfig.EnableChargeTimeDisplay.Value;
            }

            BaseOptionConfig.IsDisabledDelegate chargeTimeIsDisabled = chargeTimeIsDisabledImpl;

            addOption(new StringInputFieldOption(holdoutZoneConfig.TimeRemainingDisplayFormat.ConfigEntry, new InputFieldConfig
            {
                lineType = TMPro.TMP_InputField.LineType.SingleLine,
                richText = false,
                submitOn = InputFieldConfig.SubmitEnum.OnExit,
                checkIfDisabled = chargeTimeIsDisabled
            }));
        }

        static void addHalcyonShrineOptions(HalcyonShrineConfig halcyonShrineConfig)
        {
            addOption(new CheckBoxOption(halcyonShrineConfig.EnableProgressObjective));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool progressObjectiveIsDisabledImpl()
            {
                return !halcyonShrineConfig.EnableProgressObjective.Value;
            }

            BaseOptionConfig.IsDisabledDelegate progressObjectiveIsDisabled = progressObjectiveIsDisabledImpl;

            addOption(new StringInputFieldOption(halcyonShrineConfig.ChargeProgressDisplayFormat.ConfigEntry, new InputFieldConfig
            {
                lineType = TMPro.TMP_InputField.LineType.SingleLine,
                richText = false,
                submitOn = InputFieldConfig.SubmitEnum.OnExit,
                checkIfDisabled = progressObjectiveIsDisabled
            }));
        }

        static void addHalcyonBeaconOptions(HalcyonBeaconConfig halcyonBeaconConfig)
        {
            addOption(new CheckBoxOption(halcyonBeaconConfig.EnableTotalCostDisplay));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool totalCostDisplayIsDisabledImpl()
            {
                return !halcyonBeaconConfig.EnableTotalCostDisplay.Value;
            }

            BaseOptionConfig.IsDisabledDelegate totalCostDisplayIsDisabled = totalCostDisplayIsDisabledImpl;

            addOption(new StringInputFieldOption(halcyonBeaconConfig.TotalCostDisplayFormatter.ConfigEntry, new InputFieldConfig
            {
                lineType = TMPro.TMP_InputField.LineType.SingleLine,
                richText = false,
                submitOn = InputFieldConfig.SubmitEnum.OnExit,
                checkIfDisabled = totalCostDisplayIsDisabled
            }));

            addOption(new ColorOption(halcyonBeaconConfig.TotalCostAffordableColor, new ColorOptionConfig
            {
                checkIfDisabled = totalCostDisplayIsDisabled
            }));

            addOption(new ColorOption(halcyonBeaconConfig.TotalCostNotAffordableColor, new ColorOptionConfig
            {
                checkIfDisabled = totalCostDisplayIsDisabled
            }));
        }

        static void addShrineChanceOptions(ShrineChanceConfig shrineChanceConfig)
        {
            addOption(new CheckBoxOption(shrineChanceConfig.EnableActivationCountDisplay));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool activationCountDisplayIsDisabledImpl()
            {
                return !shrineChanceConfig.EnableActivationCountDisplay.Value;
            }

            BaseOptionConfig.IsDisabledDelegate activationCountDisplayIsDisabled = activationCountDisplayIsDisabledImpl;

            addOption(new StringInputFieldOption(shrineChanceConfig.ActivationCountDisplayFormatter.ConfigEntry, new InputFieldConfig
            {
                lineType = TMPro.TMP_InputField.LineType.SingleLine,
                richText = false,
                submitOn = InputFieldConfig.SubmitEnum.OnExit,
                checkIfDisabled = activationCountDisplayIsDisabled
            }));
        }

        static void addShrineBloodOptions(ShrineBloodConfig shrineBloodConfig)
        {
            addOption(new CheckBoxOption(shrineBloodConfig.EnableGoldGainedDisplay));

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            bool goldGainedDisplayIsDisabledImpl()
            {
                return !shrineBloodConfig.EnableGoldGainedDisplay.Value;
            }

            BaseOptionConfig.IsDisabledDelegate goldGainedDisplayIsDisabled = goldGainedDisplayIsDisabledImpl;

            addOption(new StringInputFieldOption(shrineBloodConfig.GoldGainedDisplayFormatter.ConfigEntry, new InputFieldConfig
            {
                lineType = TMPro.TMP_InputField.LineType.SingleLine,
                richText = false,
                submitOn = InputFieldConfig.SubmitEnum.OnExit,
                checkIfDisabled = goldGainedDisplayIsDisabled
            }));
        }
    }
}
