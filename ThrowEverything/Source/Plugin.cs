using BepInEx;
using HarmonyLib;
using ThrowEverything.Patches;

namespace ThrowEverything
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(LethalCompanyInputUtils.LethalCompanyInputUtilsPlugin.ModId, BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);

        internal static new BepInEx.Logging.ManualLogSource Logger;

        private void Awake()
        {
            Logger = base.Logger;

            harmony.PatchAll(typeof(GrabbableObject_Patch));
            harmony.PatchAll(typeof(HUDManager_Patch));
            harmony.PatchAll(typeof(PlayerControllerB_Patch));

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
