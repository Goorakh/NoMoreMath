using EntityStates.Interactables.GoldBeacon;
using RoR2;
using RoR2.UI;
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

        static string ActivateGoldshoreBeaconTracker_GenerateString(On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.orig_GenerateString orig, ObjectivePanelController.ObjectiveTracker self)
        {
            StringBuilder stringBuilder = HG.StringBuilderPool.RentStringBuilder();
            if (self is ObjectivePanelController.ActivateGoldshoreBeaconTracker beaconTracker)
            {
#pragma warning disable Publicizer001 // Accessing a member that was not originally public
                GoldshoresMissionController missionController = beaconTracker.missionController;
#pragma warning restore Publicizer001 // Accessing a member that was not originally public

                int totalCost = 0;
                foreach (GameObject beaconObject in missionController.beaconInstanceList)
                {
                    if (!beaconObject)
                        continue;

                    PurchaseInteraction purchaseInteraction = beaconObject.GetComponent<PurchaseInteraction>();
                    if (purchaseInteraction && purchaseInteraction.available)
                    {
                        totalCost += purchaseInteraction.Networkcost;
                    }
                }

                stringBuilder.Append(" (<color=#");

                CharacterMaster localPlayerMaster = PlayerUtils.GetLocalUserMaster();
                bool canAfford = localPlayerMaster && localPlayerMaster.money >= totalCost;

                if (canAfford)
                {
                    stringBuilder.Append("00FF00");
                }
                else
                {
                    stringBuilder.Append("FF0000");
                }

                stringBuilder.Append(">$");
                stringBuilder.Append(totalCost);
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
