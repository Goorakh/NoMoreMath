using BepInEx;
using R2API.Utils;
using System.Diagnostics;

namespace NoMoreMath
{
    [NetworkCompatibility(CompatibilityLevel.NoNeedForSync, VersionStrictness.DifferentModVersionsAreOk)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Main : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "NoMoreMath";
        public const string PluginVersion = "1.3.0";

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);
            NoMoreMath.Config.Init(Paths.ConfigPath);

            GoldShoresBeacons.PatchController.Apply();
            ChanceShrine.PatchController.Apply();
            EffectiveHealth.EffectiveHealthPatchController.Apply();
            EffectivePurchaseCost.EffectivePurchaseCostPatchController.Apply();
            BloodShrine.BloodShrineAmountGainedPatchController.Apply();
            HoldoutZoneTimeRemaining.HoldoutZoneTimeRemainingPatchController.Apply();

#if DEBUG
            // This is so we can connect to ourselves.
            // Instructions:
            // Step One: Start two instances of RoR2 (do this through the .exe directly)
            // Step Two: Host a game with one instance of RoR2.
            // Step Three: On the instance that isn't hosting, open up the console (ctrl + alt + tilde) and enter the command "connect localhost:7777"
            // DO NOT MAKE A MISTAKE SPELLING THE COMMAND OR YOU WILL HAVE TO RESTART THE CLIENT INSTANCE!!
            // Step Four: Test whatever you were going to test.
            On.RoR2.Networking.NetworkManagerSystem.ClientSendAuth += (orig, self, conn) => { };

            // The client player does not have any entitlements when connected for some reason, so just force them enabled in that case
            On.RoR2.PlayerCharacterMasterControllerEntitlementTracker.HasEntitlement += (orig, self, entitlementDef) => true;
#endif

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }

        void OnDestroy()
        {
            GoldShoresBeacons.PatchController.Cleanup();
            ChanceShrine.PatchController.Cleanup();
            EffectiveHealth.EffectiveHealthPatchController.Cleanup();
            EffectivePurchaseCost.EffectivePurchaseCostPatchController.Cleanup();
            BloodShrine.BloodShrineAmountGainedPatchController.Cleanup();
            HoldoutZoneTimeRemaining.HoldoutZoneTimeRemainingPatchController.Cleanup();
        }
    }
}
