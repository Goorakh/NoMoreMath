using RoR2;
using RoR2.UI;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NoMoreMath.GoldShoresBeacons
{
    static class PatchController
    {
        public static void Apply()
        {
            On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.GenerateString += ActivateGoldshoreBeaconTracker_GenerateString;

            On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.IsDirty += ActivateGoldshoreBeaconTracker_IsDirty;
        }

        public static void Cleanup()
        {
            On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.GenerateString -= ActivateGoldshoreBeaconTracker_GenerateString;

            On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.IsDirty -= ActivateGoldshoreBeaconTracker_IsDirty;
        }

        static string ActivateGoldshoreBeaconTracker_GenerateString(On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.orig_GenerateString orig, ObjectivePanelController.ObjectiveTracker self)
        {
            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            if (self is ObjectivePanelController.ActivateGoldshoreBeaconTracker beaconTracker)
            {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                GoldshoresMissionController missionController = beaconTracker.missionController;
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

                int[] costs = new int[missionController.beaconCount];

                for (int i = 0; i < missionController.beaconInstanceList.Count; i++)
                {
                    GameObject beaconObject = missionController.beaconInstanceList[i];
                    if (!beaconObject)
                        continue;

                    PurchaseInteraction purchaseInteraction = beaconObject.GetComponent<PurchaseInteraction>();
                    if (purchaseInteraction && purchaseInteraction.available)
                    {
                        costs[i] = purchaseInteraction.Networkcost;
                    }
                }

                CharacterMaster localPlayerMaster = PlayerUtils.GetLocalUserMaster();
                int totalEffectiveCost = CostUtils.GetTotalEffectiveCost(localPlayerMaster, costs);

                stringBuilder.Append(" (<color=#");

                bool canAfford = localPlayerMaster && localPlayerMaster.money >= totalEffectiveCost;

                if (canAfford)
                {
                    stringBuilder.Append("00FF00");
                }
                else
                {
                    stringBuilder.Append("FF0000");
                }

                stringBuilder.Append('>');

                CostTypeDef costType = CostTypeCatalog.GetCostTypeDef(CostTypeIndex.Money);

                EffectivePurchaseCost.EffectivePurchaseCostPatchController.DisableBuildCostStringPatch = true;
                int cost = costs.Sum();
                costType.BuildCostStringStyled(cost, stringBuilder, false, false);

                if (cost != totalEffectiveCost)
                {
                    CostUtils.FormatEffectiveCost(costType, totalEffectiveCost, stringBuilder, false, false);
                }

                EffectivePurchaseCost.EffectivePurchaseCostPatchController.DisableBuildCostStringPatch = false;

                stringBuilder.Append("</color>)");
            }
            else
            {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                Log.Error($"{nameof(self)} is not {nameof(ObjectivePanelController.ActivateGoldshoreBeaconTracker)}");
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
            }

            string result = orig(self) + stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);
            return result;
        }

        static bool ActivateGoldshoreBeaconTracker_IsDirty(On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.orig_IsDirty orig, ObjectivePanelController.ObjectiveTracker self)
        {
            // Still call orig to avoid possible compat issues
            orig(self);

            // Fuck it, performance be damned
            return true;
        }
    }
}
