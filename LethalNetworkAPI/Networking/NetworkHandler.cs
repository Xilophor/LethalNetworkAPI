// ReSharper disable MemberCanBeMadeStatic.Global

using System.Collections.Generic;
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
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoin;
    }

    public override void OnNetworkDespawn()
    {
        NetworkDespawn?.Invoke();
    }
    
    #region Messages
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string identifier, string data, bool toOtherClients = false, bool sendToOriginator = false, ServerRpcParams serverRpcParams = default)
    {
        if (!toOtherClients)
            OnServerMessage?.Invoke(identifier, data, serverRpcParams.Receive.SenderClientId);
        else if (!sendToOriginator)
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(), Allocator.Persistent);
            if (!clientIds.Any()) return;
            
            MessageClientRpc(identifier, data, serverRpcParams.Receive.SenderClientId, 
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
        }
        else
            MessageClientRpc(identifier, data, serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    internal void MessageClientRpc(string identifier, string data, ulong originatorClient = 99999, ClientRpcParams clientRpcParams = default)
    {
        OnClientMessage?.Invoke(identifier, data, originatorClient);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
    }

    #endregion Messages

    #region Events

    [ServerRpc(RequireOwnership = false)]
    internal void EventServerRpc(string identifier, bool toOtherClients = false, bool sendToOriginator = false, ServerRpcParams serverRpcParams = default)
    {
        if (!toOtherClients)
            OnServerEvent?.Invoke(identifier, serverRpcParams.Receive.SenderClientId);
        else if (!sendToOriginator)
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(), Allocator.Persistent);
            if (!clientIds.Any()) return;
            
            EventClientRpc(identifier, serverRpcParams.Receive.SenderClientId, 
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
        }
        else
            EventClientRpc(identifier, serverRpcParams.Receive.SenderClientId);
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {identifier}");
#endif
    }

    [ClientRpc]
    internal void EventClientRpc(string identifier, ulong originatorId = 99999, ClientRpcParams clientRpcParams = default)
    {
        OnClientEvent?.Invoke(identifier, originatorId);
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
    internal void SyncedEventClientRpc(string identifier, double time, ulong originatorClient, ClientRpcParams clientRpcParams = default)
    {
        OnSyncedClientEvent?.Invoke(identifier, time, originatorClient);
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
    internal void UpdateVariableClientRpc(string identifier, string data, ClientRpcParams clientRpcParams = default)
    {
        OnVariableUpdate?.Invoke(identifier, data);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received variable with identifier: {identifier}");
#endif
    }
    
    

    [ServerRpc(RequireOwnership = false)]
    internal void GetVariableValueServerRpc(string identifier, ServerRpcParams serverRpcParams = default)
    {
        GetVariableValue?.Invoke(identifier, serverRpcParams.Receive.SenderClientId);
    }

    #endregion

    internal static NetworkHandler? Instance { get; private set; }

    internal readonly List<ILethalNetVar> ObjectNetworkVariableList = [];

    #region Internal Events
    
    internal static event Action? NetworkSpawn;
    internal static event Action? NetworkDespawn;
    internal static event Action? NetworkTick;
    internal static event Action<ulong>? OnPlayerJoin;
    
    internal static event Action<string, string, ulong>? OnServerMessage; // identifier, data, originatorClientId
    internal static event Action<string, string, ulong>? OnClientMessage; // identifier, data, originatorClientId
    
    internal static event Action<string, string>? OnVariableUpdate; // identifier, data
    internal static event Action<string, ulong>? GetVariableValue; // identifier, connectedClientId
    
    internal static event Action<string, ulong>? OnServerEvent; // identifier, originatorClientId
    internal static event Action<string, ulong>? OnClientEvent; // identifier, originatorClientId
    internal static event Action<string, double, ulong>? OnSyncedServerEvent; // identifier, time, originatorClientId
    internal static event Action<string, double, ulong>? OnSyncedClientEvent; // identifier, time, originatorClientId

    #endregion
}