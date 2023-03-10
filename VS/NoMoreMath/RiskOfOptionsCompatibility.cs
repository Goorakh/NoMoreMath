using MonoMod.RuntimeDetour;
using System;
using UnityEngine;

namespace NoMoreMath
{
    internal static class RiskOfOptionsCompatibility
    {
        static Sprite createIconSprite(byte[] imageBytes)
        {
            Texture2D texture = new Texture2D(1, 1);
            if (!texture.LoadImage(imageBytes))
            {
                Log.Warning("Could not load icon image data");
                return null;
            }

            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
        }

        internal static void CreateOptions()
        {
            RiskOfOptions.ModSettingsManager.SetModDescription("Configuration for No More Math", Main.PluginGUID, "No More Math");

            Sprite iconSprite = createIconSprite(Properties.Resources.icon);
            if (iconSprite)
            {
                RiskOfOptions.ModSettingsManager.SetModIcon(iconSprite);
            }

            RiskOfOptions.OptionConfigs.InputFieldConfig inputFieldConfig = new RiskOfOptions.OptionConfigs.InputFieldConfig
            {
                submitOn = RiskOfOptions.OptionConfigs.InputFieldConfig.SubmitEnum.OnSubmit
            };

            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Config.BloodShrineMoney, inputFieldConfig));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Config.ChanceShrineUses, inputFieldConfig));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Config.EffectiveHealth, inputFieldConfig));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Config.EffectiveFullHealth, inputFieldConfig));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Config.EffectivePurchaseCost, inputFieldConfig));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Config.GoldShoresBeacons, inputFieldConfig));
            RiskOfOptions.ModSettingsManager.AddOption(new RiskOfOptions.Options.StringInputFieldOption(Config.HoldoutZoneTimeRemaining, inputFieldConfig));

            new Hook(typeof(RiskOfOptions.Options.StringInputFieldOption).GetMethod(nameof(RiskOfOptions.Options.StringInputFieldOption.CreateOptionGameObject)), StringInputFieldRichTextFix);
        }

        static GameObject StringInputFieldRichTextFix(Func<RiskOfOptions.Options.StringInputFieldOption, GameObject, Transform, GameObject> orig, RiskOfOptions.Options.StringInputFieldOption self, GameObject prefab, Transform parent)
        {
            GameObject result = orig(self, prefab, parent);

            RiskOfOptions.Components.Options.InputFieldController inputFieldController = result.GetComponentInChildren<RiskOfOptions.Components.Options.InputFieldController>();
            if (inputFieldController)
            {
                RiskOfOptions.Components.Options.RooInputField inputField = inputFieldController.inputField;
                if (inputField)
                {
                    inputField.richText = false;
                    inputField.isRichTextEditingAllowed = false;
                }
            }

            return result;
        }
    }
}
