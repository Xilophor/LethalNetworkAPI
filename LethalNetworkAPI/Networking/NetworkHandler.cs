using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;

namespace LethalNetworkAPI.Networking;

internal class NetworkHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) && Instance != null)
            Instance.gameObject.GetComponent<NetworkObject>().Despawn(); 
        Instance = this;
    }

    #region Messages
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, string message)
    {
        OnMessage?.Invoke(guid, message, true);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {JsonParser.Parse(message)}");
#endif
    }

    [ClientRpc]
    internal void MessageClientRpc(string guid, string message, ClientRpcParams clientRpcParams = default)
    {
        OnMessage?.Invoke(guid, message, false);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received client data: {JsonParser.Parse(message)}");
#endif
    }

    #endregion Messages

    #region Events

    [ServerRpc(RequireOwnership = false)]
    internal void EventServerRpc(string guid)
    {
        OnEvent?.Invoke(guid, true);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {guid}");
#endif
    }

    [ClientRpc]
    internal void EventClientRpc(string guid, ClientRpcParams clientRpcParams = default)
    {
        OnEvent?.Invoke(guid, false);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with guid: {guid}");
#endif
    }

    #endregion Events

    internal static NetworkHandler Instance { get; private set; }
    
    internal static event Action<string, string, bool> OnMessage; // guid, message, isServerMessage
    
    internal static event Action<string, bool> OnEvent; // guid, isServerEvent
}