using System.Security.Cryptography;
using System.Text;
using HarmonyLib;
using Object = UnityEngine.Object;

namespace LethalNetworkAPI.Networking;

[HarmonyPatch]
[HarmonyPriority(Priority.First)]
[HarmonyWrapSafe]
internal class NetworkObjectManager
{
    [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.Start))] 
    private static void Init()
    {
        if (_networkPrefab != null)
            return;
        
        var disabledPrefab = new GameObject("NetworkAPIContainer") { hideFlags = HideFlags.HideAndDontSave };
        disabledPrefab.SetActive(false);

        _networkPrefab = MakePrefab<NetworkHandler>("LethalNetworkAPI.Handler", disabledPrefab, 889887688); // Ensure compatibility with old method
    }

    private static GameObject MakePrefab<T>(string name, GameObject parent, uint overrideGuid = 0) where T : NetworkBehaviour
    {
        var prefab = new GameObject(name);
        prefab.transform.SetParent(parent.transform);
        
        prefab.AddComponent<NetworkObject>();
        prefab.AddComponent<T>();
        prefab.hideFlags = HideFlags.HideAndDontSave;
        
        if (overrideGuid != 0)
            prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = overrideGuid;
        else
        {
            var newId = BitConverter.ToUInt32(MD5.Create()
                .ComputeHash(Encoding.UTF8.GetBytes(Assembly.GetExecutingAssembly().GetName().Name + name)), 0);

            prefab.GetComponent<NetworkObject>().GlobalObjectIdHash = newId;
        }

        NetworkManager.Singleton.AddNetworkPrefab(prefab);
        return prefab;
    }

    [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    private static void SpawnNetworkHandler()
    {
        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;

        var networkHandlerHost = Object.Instantiate(_networkPrefab, Vector3.zero, Quaternion.identity, StartOfRound.Instance.transform);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }

    private static GameObject _networkPrefab = null!;
}