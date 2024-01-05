using BepInEx;
using HarmonyLib;

namespace LethalNetworkAPI;


[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
internal class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Instance = this;
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        Logger = base.Logger;
        
        NetcodePatcher();
        
        _harmony.PatchAll(typeof(NetworkObjectManager));

        Logger.LogDebug("LethalNetworkAPI Patches Applied");

        var message = new LethalNetworkMessage<string>("");
        message.SendClient("", 0);

        var variable = new LethalNetworkVariable<string>("") { Value = "" };

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

    internal static Plugin Instance = null!;
    internal new static BepInEx.Logging.ManualLogSource Logger { get; private set; } = null!;
    
    private static Harmony _harmony = null!;
}

[Serializable]
internal class ValueWrapper<T>(T? value)
{
    public T? var = value;
}
