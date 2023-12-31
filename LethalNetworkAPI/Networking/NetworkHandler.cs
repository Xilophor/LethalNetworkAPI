using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
// ReSharper disable MemberCanBeMadeStatic.Global

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
    internal void MessageServerRpc(string guid, string data) =>
        OnMessage?.Invoke(guid, data, true);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, string data, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, data, false);

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