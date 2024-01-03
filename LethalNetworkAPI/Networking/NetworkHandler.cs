

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

    public override void OnNetworkDespawn()
    {
        NetworkDespawn?.Invoke();
    }
    
    #region Messages
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string identifier, string data, ServerRpcParams serverRpcParams = default)
    {
        OnServerMessage?.Invoke(identifier, data, serverRpcParams.Receive.SenderClientId);
    }
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageOthersServerRpc(string identifier, string data, ServerRpcParams serverRpcParams = default)
    {
        var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
            .Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(), Allocator.Persistent);
        if (!clientIds.Any()) return;
        
        MessageOthersClientRpc(identifier, data, serverRpcParams.Receive.SenderClientId, 
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
    }

    [ClientRpc]
    internal void MessageClientRpc(string identifier, string data, ClientRpcParams clientRpcParams = default)
    {
        OnClientMessage?.Invoke(identifier, data);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
    }
    
    [ClientRpc]
    private void MessageOthersClientRpc(string identifier, string data, ulong originatorClient, ClientRpcParams clientRpcParams = default)
    {
        OnClientMessageFrom?.Invoke(identifier, data, originatorClient);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
    }

    #endregion Messages

    #region Events

    [ServerRpc(RequireOwnership = false)]
    internal void EventServerRpc(string identifier, ServerRpcParams serverRpcParams = default)
    {
        OnServerEvent?.Invoke(identifier, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {identifier}");
#endif
    }

    [ClientRpc]
    internal void EventClientRpc(string identifier, ClientRpcParams clientRpcParams = default)
    {
        OnClientEvent?.Invoke(identifier);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with identifier: {identifier}");
#endif
    }
    
    [ServerRpc(RequireOwnership = false)]
    internal void SyncedEventServerRpc(string identifier, double time, ServerRpcParams serverRpcParams = default)
    {
        OnSyncedServerEvent?.Invoke(identifier, time, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {identifier}");
#endif
    }

    [ClientRpc]
    internal void SyncedEventClientRpc(string identifier, double time, ClientRpcParams clientRpcParams = default)
    {
        OnSyncedClientEvent?.Invoke(identifier, time);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with identifier: {identifier}");
#endif
    }

    #endregion Events

    #region Variables
    
    [ServerRpc(RequireOwnership = false)]
    internal void UpdateVariableServerRpc(string identifier, string data, ServerRpcParams serverRpcParams = default)
    {
        if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId) OnVariableUpdate?.Invoke(identifier, data);
        
        var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(), Allocator.Persistent);
        if (!clientIds.Any()) return;
        
        UpdateVariableClientRpc(identifier, data, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
#if DEBUG
        Plugin.Logger.LogDebug($"Received variable with identifier: {identifier}; data: {data}");
#endif
    }

    [ClientRpc]
    private void UpdateVariableClientRpc(string identifier, string data, ClientRpcParams clientRpcParams = default)
    {
        OnVariableUpdate?.Invoke(identifier, data);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received variable with identifier: {identifier}");
#endif
    }



    [ServerRpc(RequireOwnership = false)]
    internal void UpdateOwnershipServerRpc(string identifier, ulong newClientId, ServerRpcParams serverRpcParams = default)
    {
        UpdateOwnershipClientRpc(identifier, [serverRpcParams.Receive.SenderClientId, newClientId]);
    }
    
    [ClientRpc]
    internal void UpdateOwnershipClientRpc(string identifier, ulong[] clientIds)
    {
        OnOwnershipChange?.Invoke(identifier, clientIds);
    }

    #endregion

    internal static NetworkHandler? Instance { get; private set; }
    
    internal static event Action? NetworkSpawn;
    internal static event Action? NetworkDespawn;
    internal static event Action? NetworkTick;
    
    internal static event Action<string, string, ulong>? OnServerMessage; // identifier, data, originatorClientId
    internal static event Action<string, string>? OnClientMessage; // identifier, data
    internal static event Action<string, string, ulong>? OnClientMessageFrom; // identifier, data, originatorClientId
    
    internal static event Action<string, string>? OnVariableUpdate; // identifier, data
    internal static event Action<string, ulong[]>? OnOwnershipChange; // identifier, clientIds
    
    internal static event Action<string, ulong>? OnServerEvent; // identifier, originatorClientId
    internal static event Action<string>? OnClientEvent; // identifier
    internal static event Action<string, double, ulong>? OnSyncedServerEvent; // identifier, time, originatorClientId
    internal static event Action<string, double>? OnSyncedClientEvent; // identifier, time
}