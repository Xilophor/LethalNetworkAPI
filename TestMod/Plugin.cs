﻿using BepInEx;
using BepInEx.Logging;
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
            Logger = base.Logger;
            
            // Plugin startup logic
            Logger.LogDebug($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Instance = this;

            Harmony.CreateAndPatchAll(typeof(Test));

            Message = new LethalNetworkMessage<Vector3>("customMessage");

            Message.OnClientReceived += Receive;

            CustomIntVariable.Value = 7;
            
            Logger.LogDebug(new ulong());
        }

        private static void Receive(Vector3 data)
        {
            Logger.LogInfo($"Player position: {data}");

            CustomIntVariable.Value = CustomIntVariable.Value switch
            {
                > 5 => 5,
                < -5 => -5,
                _ => CustomIntVariable.Value
            };
        } 

        public static Plugin Instance;
        public new static ManualLogSource Logger;
        
        [LethalNetworkProtected]
        public static LethalNetworkMessage<Vector3> Message;

        private static readonly LethalNetworkVariable<int> CustomIntVariable = new("customGuid");
    }

    [HarmonyPatch]
    public class Test
    {
        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.StartGame))]
        private static void TestPrint()
        {
            Plugin.Logger.LogDebug(NetworkManager.Singleton.ConnectedClientsIds.Join());
            
            if (NetworkManager.Singleton.IsHost)
                Plugin.Message.SendAllClients(GameNetworkManager.Instance.localPlayerController.transform.position, false);
        }
    }
}