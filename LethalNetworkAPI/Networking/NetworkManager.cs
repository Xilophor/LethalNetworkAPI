using HarmonyLib;
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

        var networkHandlerHost = Object.Instantiate(_networkPrefab, Vector3.zero, Quaternion.identity, StartOfRound.Instance.transform);
        networkHandlerHost.GetComponent<NetworkObject>().Spawn();
    }
    
    private static GameObject _networkPrefab;
}