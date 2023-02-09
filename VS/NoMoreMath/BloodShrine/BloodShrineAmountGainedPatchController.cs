using NoMoreMath.EffectivePurchaseCost;
using NoMoreMath.Utility;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoMoreMath.BloodShrine
{
    static class BloodShrineAmountGainedPatchController
    {
        static string getShrineAmountGivenString(PurchaseInteraction purchaseInteraction)
        {
            if (purchaseInteraction.TryGetComponent(out ShrineBloodBehavior shrineBloodBehavior))
            {
                CharacterMaster playerMaster = PlayerUtils.GetLocalUserMaster();
                if (playerMaster)
                {
                    CharacterBody body = playerMaster.GetBody();
                    if (body && Config.BloodShrineMoney.Value != "")
                    {
                        int amountGained = (int)(body.healthComponent.fullCombinedHealth * (purchaseInteraction.cost / 100f * shrineBloodBehavior.goldToPaidHpRatio));
                        return " " + Config.BloodShrineMoney.Value
                            .Replace("{amount}", amountGained.ToString());
                    }
                }
            }

            return string.Empty;
        }

        public static void Apply()
        {
            On.RoR2.PurchaseInteraction.GetContextString += PurchaseInteraction_GetContextString;
            On.RoR2.PurchaseInteraction.GetDisplayName += PurchaseInteraction_GetDisplayName;
        }

        public static void Cleanup()
        {
            On.RoR2.PurchaseInteraction.GetContextString -= PurchaseInteraction_GetContextString;
            On.RoR2.PurchaseInteraction.GetDisplayName -= PurchaseInteraction_GetDisplayName;
        }

        static string PurchaseInteraction_GetContextString(On.RoR2.PurchaseInteraction.orig_GetContextString orig, PurchaseInteraction self, Interactor activator)
        {
            return orig(self, activator) + getShrineAmountGivenString(self);
        }

        static string PurchaseInteraction_GetDisplayName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        {
            return orig(self) + getShrineAmountGivenString(self);
        }
    }
}
