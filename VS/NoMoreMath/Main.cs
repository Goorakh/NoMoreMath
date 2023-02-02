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
        public const string PluginVersion = "1.0.0";

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Log.Init(Logger);

            GoldShoresBeacons.PatchController.Apply();

            stopwatch.Stop();
            Log.Info_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalSeconds:F2} seconds");
        }
    }
}
