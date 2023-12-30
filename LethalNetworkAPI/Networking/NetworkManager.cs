using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LethalNetworkAPI.Networking;

[HarmonyPatch]
internal class NetworkObjectManager
{
    [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))] 
    public static void Init()
    {
        if (_networkPrefab != null)
            return;

        var mainAssetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("LethalNetworkAPI.asset"));
        _networkPrefab = (GameObject)mainAssetBundle.LoadAsset("Assets/LethalNetworkAPI.Handler.prefab");
        
        NetworkManager.Singleton.AddNetworkPrefab(_networkPrefab);
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
    private static void SpawnNetworkHandler()
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;

        var networkHandlerHost = Object.Instantiate(_networkPrefab, Vector3.zero, Quaternion.identity);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }
//         NetworkManager.Singleton.OnClientConnectedCallback += UpdateClientIdCollection;
//         NetworkManager.Singleton.OnClientDisconnectCallback += UpdateClientIdCollection;
//         
//         ClientIdCollections = new List<ulong[]> {new[] {NetworkManager.Singleton.LocalClientId}};
//     }
//
//     [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SetInstanceValuesBackToDefault))]
//     private static void SetInstanceValuesBackToDefault()
//     {
//         NetworkManager.Singleton.OnClientConnectedCallback -= UpdateClientIdCollection;
//         NetworkManager.Singleton.OnClientDisconnectCallback -= UpdateClientIdCollection;
//     }
//
//     private static void UpdateClientIdCollection(ulong clientId)
//     {
//         var result = Combinations(NetworkManager.Singleton.ConnectedClientsIds);
//
//         ClientIdCollections = result;
//
// #if DEBUG
//         foreach (var item in result)
//             Plugin.Instance.Logger.LogInfo($"[{string.Join(", ", item)}]");
// #endif
//     }
//     
//     private static IEnumerable<T[]> Combinations<T>(IEnumerable<T> source) {
//         if (null == source)
//             throw new ArgumentNullException(nameof(source));
//
//         T[] data = source.ToArray();
//
//         return Enumerable
//             .Range(0, 1 << (data.Length))
//             .Select(index => data
//                 .Where((v, i) => (index & (1 << i)) != 0)
//                 .ToArray());
//     }
    
    private static GameObject _networkPrefab;
    // internal static IEnumerable<ulong[]> ClientIdCollections { get; private set; }
}