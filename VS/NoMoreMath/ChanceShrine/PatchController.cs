using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace NoMoreMath.ChanceShrine
{
    static class PatchController
    {
        static string getChanceShrineActivationCountString(PurchaseInteraction purchaseInteraction)
        {
            if (purchaseInteraction.TryGetComponent<ShrineChanceBehavior>(out ShrineChanceBehavior shrineChanceBehavior))
            {
                CharacterMaster master = PlayerUtils.GetLocalUserMaster();
                if (master)
                {
                    uint playerMoney = master.money;

                    uint affordableActivations = 0;
                    int currentCost = purchaseInteraction.Networkcost;

                    if (currentCost > 0)
                    {
                        while (playerMoney >= currentCost)
                        {
                            uint effectiveCost = (uint)CostUtils.GetEffectiveCost(currentCost, master);
                            if (effectiveCost == 0)
                            {
                                affordableActivations = uint.MaxValue;
                                break;
                            }

                            playerMoney -= effectiveCost;
                            currentCost = (int)(currentCost * shrineChanceBehavior.costMultiplierPerPurchase);
                            affordableActivations++;
                        }
                    }
                    else
                    {
                        affordableActivations = uint.MaxValue;
                    }

                    StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
                    stringBuilder.Append(" (");

                    if (affordableActivations == uint.MaxValue)
                    {
                        stringBuilder.Append("INF.");
                    }
                    else
                    {
                        stringBuilder.Append(affordableActivations);
                    }

                    stringBuilder.Append(" activation");
                    
                    if (affordableActivations != 1)
                    {
                        stringBuilder.Append('s');
                    }

                    stringBuilder.Append(')');

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
            return orig(self, activator) + getChanceShrineActivationCountString(self);
        }

        static string PurchaseInteraction_GetDisplayName(On.RoR2.PurchaseInteraction.orig_GetDisplayName orig, PurchaseInteraction self)
        {
            return orig(self) + getChanceShrineActivationCountString(self);
        }
    }
}
