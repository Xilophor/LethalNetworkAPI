using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LethalNetworkAPI;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class LethalNetworkAPIPlugin : BaseUnityPlugin
{
    private void Awake()
    {
#if NET472
        throw new Exception("The incorrect version of LethalNetworkAPI is installed. Use the netstandard2.1 version provided by the Thunderstore listing or the GitHub release. The currently installed version will *not* work as intended.");
#endif
        Instance = this;
        Logger = base.Logger;
        
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        
        NetcodePatcher();
        
        _harmony.PatchAll(typeof(NetworkObjectManager));

        Logger.LogInfo($"LethalNetworkAPI v{MyPluginInfo.PLUGIN_VERSION} has Loaded.");
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
    
    public static LethalNetworkAPIPlugin Instance = null!;
    
    internal new static ManualLogSource Logger { get; private set; } = null!;
    
    private static Harmony _harmony = null!;
}
