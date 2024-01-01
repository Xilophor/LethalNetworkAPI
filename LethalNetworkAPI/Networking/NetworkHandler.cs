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
        
        NetworkSpawn?.Invoke();
    }

    #region Messages
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, string data)
    {
        OnMessage?.Invoke(guid, data, true);
    }

    [ClientRpc]
    internal void MessageClientRpc(string guid, string data, ClientRpcParams clientRpcParams = default)
    {
        OnMessage?.Invoke(guid, data, false);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
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
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with guid: {guid}");
#endif
    }
    
    [ServerRpc(RequireOwnership = false)]
    internal void SyncedEventServerRpc(string guid, double time, ServerRpcParams serverRpcParams = default)
    {
        OnSyncedServerEvent?.Invoke(guid, time, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {guid}");
#endif
    }

    [ClientRpc]
    internal void SyncedEventClientRpc(string guid, double time, ClientRpcParams clientRpcParams = default)
    {
        OnSyncedClientEvent?.Invoke(guid, time);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with guid: {guid}");
#endif
    }

    #endregion Events

    #region Variables
    
    [ServerRpc(RequireOwnership = false)]
    internal void UpdateVariableServerRpc(string guid, string data)
    {
        OnVariableUpdate?.Invoke(guid, data);
        UpdateVariableClientRpc(guid, data);
#if DEBUG
        Plugin.Logger.LogDebug($"Received variable with guid: {guid}");
#endif
    }

    [ClientRpc]
    private void UpdateVariableClientRpc(string guid, string data, ClientRpcParams clientRpcParams = default)
    {
        OnVariableUpdate?.Invoke(guid, data);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received variable with guid: {guid}");
#endif
    }

    #endregion

    internal static NetworkHandler Instance { get; private set; }
    
    internal static event Action NetworkSpawn;
    
    internal static event Action<string, string, bool> OnMessage; // guid, data, isServerMessage
    
    internal static event Action<string, string> OnVariableUpdate; // guid, data
    
    internal static event Action<string, bool> OnEvent; // guid, isServerEvent
    internal static event Action<string, double, ulong> OnSyncedServerEvent; // guid, time, originatorClientId
    internal static event Action<string, double> OnSyncedClientEvent; // guid, time
}