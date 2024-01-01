using BepInEx;
using GameNetcodeStuff;
using HarmonyLib;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;

namespace TestMod
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("LethalNetworkAPI")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogDebug($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Instance = this;

            Harmony.CreateAndPatchAll(typeof(Test));

            Message = new ("customMessage");

            Message.OnClientReceived += Receive;
        }

        private void Receive(Vector3 data)
        {
            Logger.LogInfo($"Player position: {data}");
        } 

        public static Plugin Instance;
        
        [LethalNetworkProtected]
        public static LethalNetworkMessage<Vector3> Message;
    }

    [HarmonyPatch]
    public class Test
    {
        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
        private static void TestPrint()
        {
            if (NetworkManager.Singleton.IsHost)
                Plugin.Message.SendAllClients(GameNetworkManager.Instance.localPlayerController.transform.position, false);
        }
    }
}