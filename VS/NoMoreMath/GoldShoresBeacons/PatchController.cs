using NoMoreMath.Utility;
using NoMoreMath.Utility.Extensions;
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
            if (Config.GoldShoresBeacons.Value == "") return orig(self);
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

                bool canAfford = localPlayerMaster && localPlayerMaster.money >= totalEffectiveCost;

                int cost = costs.Sum();
                string costString = cost.ToString();
                if (Config.EffectivePurchaseCost.Value != "" && cost != totalEffectiveCost)
                    costString += " " + Config.EffectivePurchaseCost.Value
                        .Replace("{amount}", totalEffectiveCost.ToString())
                        .Replace("{relative}", (totalEffectiveCost - cost).ToString())
                        .Replace("{styleOnlyOnTooltip}", "")
                        .Replace("{/styleOnlyOnTooltip}", "");

                return orig(self) + " " + Config.GoldShoresBeacons.Value
                    .Replace("{color}", canAfford ? "#00FF00" : "#FF0000")
                    .Replace("{amount}", costString);
            }
            else
            {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                Log.Error($"{nameof(self)} is not {nameof(ObjectivePanelController.ActivateGoldshoreBeaconTracker)}");
#pragma warning restore Publicizer001 // Accessing a member that was not originally public
                return orig(self);
            }
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
