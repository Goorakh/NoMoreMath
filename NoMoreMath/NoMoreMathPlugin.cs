using BepInEx;
using NoMoreMath.Config;
using NoMoreMath.ModCompatibility;
using System.Diagnostics;

namespace NoMoreMath
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(RiskOfOptions.PluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    public class NoMoreMathPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Gorakh";
        public const string PluginName = "NoMoreMath";
        public const string PluginVersion = "1.3.0";

        internal static NoMoreMathPlugin Instance { get; private set; }

        void Awake()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            Instance = SingletonHelper.Assign(Instance, this);

            Log.Init(Logger);

            Configs.Init(Config);

            if (RiskOfOptionsCompat.Enabled)
                RiskOfOptionsCompat.Init();

            stopwatch.Stop();
            Log.Message_NoCallerPrefix($"Initialized in {stopwatch.Elapsed.TotalMilliseconds:F0}ms");
        }

        void OnDestroy()
        {
            Instance = SingletonHelper.Unassign(Instance, this);
        }
    }
}
