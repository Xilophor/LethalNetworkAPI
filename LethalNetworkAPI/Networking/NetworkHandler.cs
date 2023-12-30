using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace LethalNetworkAPI.Networking;

internal class NetworkHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) 
            Instance?.gameObject.GetComponent<NetworkObject>().Despawn(); 
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, string message)
    {
        string[] guidSections = guid.Split("|");
        
        switch (guidSections[2])
        {
            case "message":
                OnMessage?.Invoke(guid, message);
                break;
        }
    }

    [ClientRpc]
    internal void MessageClientRpc(string guid, string message, ClientRpcParams clientRpcParams = default)
    {
        
    }

    internal static NetworkHandler Instance { get; private set; }
    internal static event Action<string, string> OnMessage;
}