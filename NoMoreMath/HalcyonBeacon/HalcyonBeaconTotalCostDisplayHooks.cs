using NoMoreMath.Config;
using NoMoreMath.EffectiveCost;
using RoR2;
using RoR2.UI;
using System.Collections.ObjectModel;
using UnityEngine;

namespace NoMoreMath.HalcyonBeacon
{
    static class HalcyonBeaconTotalCostDisplayHooks
    {
        [SystemInitializer]
        static void Init()
        {
            On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.IsDirty += ActivateGoldshoreBeaconTracker_IsDirty;
            On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.GenerateString += ActivateGoldshoreBeaconTracker_GenerateString;
        }

        static bool ActivateGoldshoreBeaconTracker_IsDirty(On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.orig_IsDirty orig, ObjectivePanelController.ObjectiveTracker self)
        {
            orig(self);
            return true;
        }

        static string ActivateGoldshoreBeaconTracker_GenerateString(On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.orig_GenerateString orig, ObjectivePanelController.ObjectiveTracker self)
        {
            string objectiveString = orig(self);

            HalcyonBeaconConfig halcyonBeaconConfigs = Configs.HalcyonBeacon;

            CharacterMaster viewerMaster = self.sourceDescriptor.master;
            if (halcyonBeaconConfigs.EnableTotalCostDisplay.Value && GoldshoresMissionControllerBeaconsProvider.Instance && viewerMaster)
            {
                ReadOnlyCollection<PurchaseInteraction> beaconPurchaseInteractions = GoldshoresMissionControllerBeaconsProvider.Instance.BeaconPurchaseInteractions;

                const CostTypeIndex BEACON_COST_TYPE = CostTypeIndex.Money;
                CostTypeDef beaconCostTypeDef = CostTypeCatalog.GetCostTypeDef(BEACON_COST_TYPE);

                int beaconCount = beaconPurchaseInteractions.Count;

                int[] beaconCosts = new int[beaconCount];

                int solitudeItemCount = TeamManager.LongstandingSolitudesInParty();

                for (int i = 0; i < beaconCount; i++)
                {
                    PurchaseInteraction purchaseInteraction = beaconPurchaseInteractions[i];
                    if (purchaseInteraction && purchaseInteraction.available && purchaseInteraction.costType == BEACON_COST_TYPE)
                    {
                        int cost = purchaseInteraction.cost;

                        if (solitudeItemCount > 0)
                        {
                            cost *= 1 + solitudeItemCount;
                        }

                        if (cost > 0)
                        {
                            beaconCosts[i] = cost;
                        }
                    }
                }

                int totalCost = EffectiveCostUtils.GetTotalEffectiveCost(beaconCostTypeDef, beaconCosts, viewerMaster);
                string totalCostString = totalCost.ToString();

                bool canAfford = viewerMaster.money >= totalCost;

                Color32 costColor = canAfford ? halcyonBeaconConfigs.TotalCostAffordableColor.Value : halcyonBeaconConfigs.TotalCostNotAffordableColor.Value;
                string colorString = $"#{costColor.r:X2}{costColor.g:X2}{costColor.b:X2}";

                System.Text.StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();

                stringBuilder.EnsureCapacity(objectiveString.Length + halcyonBeaconConfigs.TotalCostDisplayFormatter.StrippedLength + 15);

                stringBuilder.Append(objectiveString)
                             .Append(' ');

                halcyonBeaconConfigs.TotalCostDisplayFormatter.AppendToStringBuilder(stringBuilder, [
                    new TagReplacementStringConfig.ReplacementInfo(HalcyonBeaconConfig.TOTAL_COST_DISPLAY_COLOR_TAG, colorString),
                    new TagReplacementStringConfig.ReplacementInfo(HalcyonBeaconConfig.TOTAL_COST_DISPLAY_COST_TAG, totalCostString)
                ]);

                objectiveString = stringBuilder.ToString();
                stringBuilder = HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            }

            return objectiveString;
        }
    }
}
