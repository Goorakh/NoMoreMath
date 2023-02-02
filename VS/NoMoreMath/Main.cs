using BepInEx;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using System.Diagnostics;
using System.Text;
using UnityEngine;

namespace NoMoreMath
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "NoMoreMath";
        public const string PluginVersion = "1.0.0";

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            On.RoR2.UI.ObjectivePanelController.ActivateGoldshoreBeaconTracker.GenerateString += ActivateGoldshoreBeaconTracker_GenerateString;

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
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
                    PurchaseInteraction purchaseInteraction = beaconObject.GetComponent<PurchaseInteraction>();

                    // TODO: Better check for if it's been purchased
                    if (!purchaseInteraction.lastActivator)
                    {
                        totalCost += purchaseInteraction.Networkcost;
                    }
                }

                CharacterMaster localPlayerMaster = PlayerUtils.GetLocalUserMaster();
                bool canAfford = localPlayerMaster && localPlayerMaster.money >= totalCost;

                stringBuilder.Append(" (<color=#");

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
    }
}
