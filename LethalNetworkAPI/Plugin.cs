using System.Reflection;
using BepInEx;
using HarmonyLib;
using LethalNetworkAPI.Networking;
using UnityEngine;

namespace LethalNetworkAPI;


[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
internal class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Instance = this;
        _harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        Logger = base.Logger;
        
        NetcodePatcher();
        
        _harmony.PatchAll(typeof(NetworkObjectManager));

        Logger.LogDebug("LethalNetworkAPI Patches Applied");
    }

    private static void NetcodePatcher()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    internal static Plugin Instance;
    internal new static BepInEx.Logging.ManualLogSource Logger { get; private set; }
    private static Harmony _harmony;
}
