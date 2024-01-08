using HarmonyLib;
using Object = UnityEngine.Object;

namespace LethalNetworkAPI.Networking;

[HarmonyPatch]
internal class NetworkObjectManager
{
    [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))] 
    private static void Init()
    {
        if (_networkPrefab != null)
            return;

        _networkPrefab = MakePrefab<NetworkHandler>("LethalNetworkAPI.Handler");
    }

    private static GameObject MakePrefab<T>(string name) where T : NetworkBehaviour
    {
        var prefab = new GameObject(name);
        prefab.AddComponent<NetworkObject>();
        prefab.AddComponent<T>();
        prefab.hideFlags = HideFlags.HideAndDontSave;
        
        var newId = NetworkManager.Singleton.NetworkConfig.Prefabs.m_Prefabs
            .First(i => NetworkManager.Singleton.NetworkConfig.Prefabs.m_Prefabs
                .Any(x => x.SourcePrefabGlobalObjectIdHash != i.SourcePrefabGlobalObjectIdHash + 1))
            .SourcePrefabGlobalObjectIdHash + 1;

        prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = newId;
        
        NetworkManager.Singleton.AddNetworkPrefab(prefab);
        return prefab;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Awake))]
    private static void SpawnNetworkHandler()
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;

        var networkHandlerHost = Object.Instantiate(_networkPrefab, Vector3.zero, Quaternion.identity, StartOfRound.Instance.transform);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }
    
    private static GameObject _networkPrefab = null!;
}