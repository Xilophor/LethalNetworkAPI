using BepInEx;
using HarmonyLib;
using LethalNetworkAPI;
using Unity.Netcode;

namespace TestMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogDebug($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!!!!!!!!!!!!!!!!!!!!!!!!!!");
            Instance = this;

            Harmony.CreateAndPatchAll(typeof(Test));

            StringMessage.OnReceived += Receive;
        }

        private void Receive(string data)
        {
            Logger.LogInfo(data);
        } 

        public static Plugin Instance;
        public static readonly LethalNetworkMessage<string> StringMessage = new("customStringMessage");
    }

    [HarmonyPatch]
    public class Test
    {
        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
        private static void TestPrint()
        {
            if (NetworkManager.Singleton.IsHost)
                Plugin.StringMessage.SendAllClients("poo-poo", false);
        }
    }
}