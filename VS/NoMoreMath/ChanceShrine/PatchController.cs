using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoMoreMath.ChanceShrine
{
    static class PatchController
    {
        static CharacterMaster getMasterFromInteractor(Interactor interactor)
        {
            if (interactor.TryGetComponent(out CharacterBody body))
            {
                return body.master;
            }

            return null;
        }

        static string getChanceShrineActivationCountString(PurchaseInteraction purchaseInteraction, Interactor interactor)
        {
            if (purchaseInteraction.TryGetComponent<ShrineChanceBehavior>(out ShrineChanceBehavior shrineChanceBehavior))
            {
                CharacterMaster master = interactor ? getMasterFromInteractor(interactor) : PlayerUtils.GetLocalUserMaster();
                if (master)
                {
                    StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
                    stringBuilder.Append(" (");

                    uint playerMoney = master.money;

                    int affordableActivations = 0;
                    int currentCost = purchaseInteraction.Networkcost;

                    while (playerMoney >= currentCost)
                    {
                        playerMoney -= (uint)currentCost;
                        currentCost = (int)(currentCost * shrineChanceBehavior.costMultiplierPerPurchase);
                        affordableActivations++;
                    }

                    stringBuilder.Append(affordableActivations);

                    stringBuilder.Append(" activations)");

                    string result = stringBuilder.ToString();
                    HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
                    return result;
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
            return orig(self, activator) + getChanceShrineActivationCountString(self, activator);
        }

        static string PurchaseInteraction_GetDisplayName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        {
            return orig(self) + getChanceShrineActivationCountString(self, null);
        }
    }
}
