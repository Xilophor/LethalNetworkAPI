namespace LethalNetworkAPI;

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LethalNetworkAPIPlugin : BaseUnityPlugin
{
    public static LethalNetworkAPIPlugin Instance { get; private set; } = null!;

    internal static new ManualLogSource Logger { get; private set; } = null!;

    private static Harmony _harmony = null!;

    private void Awake()
    {
#if NET472
        throw new System.Exception("The incorrect version of LethalNetworkAPI is installed. Use the netstandard2.1 version provided by the Thunderstore listing or the GitHub release. The currently installed version will *not* work as intended.");
#endif
        Instance = this;
        Logger = base.Logger;

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        _harmony.PatchAll();

        Logger.LogInfo($"LethalNetworkAPI v{MyPluginInfo.PLUGIN_VERSION} has Loaded.");
    }
}
