

// ReSharper disable MemberCanBeMadeStatic.Global

using Unity.Collections;

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
        NetworkManager.NetworkTickSystem.Tick += NetworkTick;
    }

    #region Messages
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, string data, ServerRpcParams serverRpcParams = default)
    {
        OnServerMessage?.Invoke(guid, data, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    internal void MessageClientRpc(string guid, string data, ClientRpcParams clientRpcParams = default)
    {
        OnClientMessage?.Invoke(guid, data);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
    }

    #endregion Messages

    #region Events

    [ServerRpc(RequireOwnership = false)]
    internal void EventServerRpc(string guid, ServerRpcParams serverRpcParams = default)
    {
        OnServerEvent?.Invoke(guid, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {guid}");
#endif
    }

    [ClientRpc]
    internal void EventClientRpc(string guid, ClientRpcParams clientRpcParams = default)
    {
        OnClientEvent?.Invoke(guid);
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
    internal void UpdateVariableServerRpc(string guid, string data, ServerRpcParams serverRpcParams = default)
    {
        if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) OnVariableUpdate?.Invoke(guid, data);
        
        var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(), Allocator.Persistent);
        if (!clientIds.Any()) return;
        
        UpdateVariableClientRpc(guid, data, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
#if DEBUG
        Plugin.Logger.LogDebug($"Received variable with guid: {guid}; data: {data}");
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



    [ServerRpc(RequireOwnership = false)]
    internal void UpdateOwnershipServerRpc(string guid, ulong newClientId, ServerRpcParams serverRpcParams = default)
    {
        UpdateOwnershipClientRpc(guid, [serverRpcParams.Receive.SenderClientId, newClientId]);
    }
    
    [ClientRpc]
    internal void UpdateOwnershipClientRpc(string guid, ulong[] clientIds)
    {
        OnOwnershipChange?.Invoke(guid, clientIds);
    }

    #endregion

    internal static NetworkHandler? Instance { get; private set; }
    
    internal static event Action? NetworkSpawn;
    internal static event Action? NetworkTick;
    
    internal static event Action<string, string, ulong>? OnServerMessage; // guid, data, originatorClientId
    internal static event Action<string, string>? OnClientMessage; // guid, data
    
    internal static event Action<string, string>? OnVariableUpdate; // guid, data
    internal static event Action<string, ulong[]>? OnOwnershipChange; // guid, clientIds
    
    internal static event Action<string, ulong>? OnServerEvent; // guid, originatorClientId
    internal static event Action<string>? OnClientEvent; // guid
    internal static event Action<string, double, ulong>? OnSyncedServerEvent; // guid, time, originatorClientId
    internal static event Action<string, double>? OnSyncedClientEvent; // guid, time
}