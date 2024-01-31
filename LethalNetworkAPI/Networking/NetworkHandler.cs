using System.Collections.Generic;
using Unity.Collections;
// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBeMadeStatic.Local

namespace LethalNetworkAPI.Networking;

internal class NetworkHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            "Attempting to set Network Handler instance...");
#endif
        if (Instance != null)
        {
#if DEBUG
            LethalNetworkAPIPlugin.Logger.LogDebug(
                "Instance already exists! Destroying current game object!");
#endif
            Destroy(this);
            return;
        }
        
        Instance = this;
        
        NetworkSpawn?.Invoke();
        NetworkManager.NetworkTickSystem.Tick += NetworkTick;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            "Created new Network Handler Instance.");
#endif
    }

    internal void Clean()
    {
        NetworkDespawn?.Invoke();
        OnPlayerJoin = null;
        NetworkDespawn = null;
        Instance = null;
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Cleaned the Network Handler instance. Is Instance Null? {Instance == null}");
#endif
    }

    private void OnClientConnectedCallback(ulong client) =>
        OnPlayerJoin?.Invoke(client);

    #region Messages
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string identifier,
        byte[] data,
        bool toOtherClients = false,
        bool sendToOriginator = false,
        ServerRpcParams serverRpcParams = default)
    {
        if (!toOtherClients)
            OnServerMessage?.Invoke(identifier, data, serverRpcParams.Receive.SenderClientId);
        else if (!sendToOriginator)
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                    .Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(), 
                Allocator.Persistent);
            if (!clientIds.Any()) return;
            
            MessageClientRpc(identifier, data, serverRpcParams.Receive.SenderClientId, 
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
        }
        else
            MessageClientRpc(identifier, data, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received event \"{identifier}\" from a client.");
#endif
    }

    [ClientRpc]
    internal void MessageClientRpc(string identifier,
        byte[] data,
        ulong originatorClient = 99999,
        ClientRpcParams clientRpcParams = default)
    {
        OnClientMessage?.Invoke(identifier, data, originatorClient);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received message \"{identifier}\" from server with originator: " +
            $"{(originatorClient == 99999 ? "server" : originatorClient)}");
#endif
    }

    #endregion Messages

    #region Events

    [ServerRpc(RequireOwnership = false)]
    internal void EventServerRpc(string identifier,
        bool toOtherClients = false,
        bool sendToOriginator = false,
        ServerRpcParams serverRpcParams = default)
    {
        if (!toOtherClients)
            OnServerEvent?.Invoke(identifier, serverRpcParams.Receive.SenderClientId);
        else if (!sendToOriginator)
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                    .Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(), 
                Allocator.Persistent);
            if (!clientIds.Any()) return;
            
            EventClientRpc(identifier, serverRpcParams.Receive.SenderClientId, 
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
        }
        else
            EventClientRpc(identifier, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received event \"{identifier}\" from a client.");
#endif
    }

    [ClientRpc]
    internal void EventClientRpc(string identifier,
        ulong originatorClient = 99999,
        ClientRpcParams clientRpcParams = default)
    {
        OnClientEvent?.Invoke(identifier, originatorClient);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received event \"{identifier}\" from server with originator: " +
            $"{(originatorClient == 99999 ? "server" : originatorClient)}");
#endif
    }
    
    [ServerRpc(RequireOwnership = false)]
    internal void SyncedEventServerRpc(string identifier,
        double time,
        ServerRpcParams serverRpcParams = default)
    {
        OnSyncedServerEvent?.Invoke(identifier, time, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received synced event \"{identifier}\" from a client.");
#endif
    }

    [ClientRpc]
    internal void SyncedEventClientRpc(string identifier,
        double time,
        ulong originatorClient,
        ClientRpcParams clientRpcParams = default)
    {
        OnSyncedClientEvent?.Invoke(identifier, time, originatorClient);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received synced event \"{identifier}\" from server with originator: " +
            $"{(originatorClient == 99999 ? "server" : originatorClient)}");
#endif
    }

    #endregion Events

    #region Variables
    
    [ServerRpc(RequireOwnership = false)]
    internal void UpdateVariableServerRpc(string identifier, 
        byte[] data,
        ServerRpcParams serverRpcParams = default)
    {
        if (serverRpcParams.Receive.SenderClientId != NetworkManager.ServerClientId)
            OnVariableUpdate?.Invoke(identifier, data);
        
        var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != serverRpcParams.Receive.SenderClientId).ToArray(),
            Allocator.Persistent);
        if (!clientIds.Any()) return;
        
        UpdateVariableClientRpc(identifier, data, 
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received variable with identifier \"{identifier}\" from a client.");
#endif
    }

    [ClientRpc]
    internal void UpdateVariableClientRpc(string identifier, 
        byte[] data, 
        ClientRpcParams clientRpcParams = default)
    {
        OnVariableUpdate?.Invoke(identifier, data);
        clientRpcParams.Send.TargetClientIdsNativeArray?.Dispose();
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received variable with identifier \"{identifier}\" from the server.");
#endif
    }
    
    

    [ServerRpc(RequireOwnership = false)]
    internal void GetVariableValueServerRpc(string identifier, 
        ServerRpcParams serverRpcParams = default)
    {
        GetVariableValue?.Invoke(identifier, serverRpcParams.Receive.SenderClientId);
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Requesting variable data with identifier \"{identifier}\" from the server.");
#endif
    }

    #endregion

    internal static NetworkHandler? Instance { get; private set; }

    internal readonly List<ILethalNetVar> ObjectNetworkVariableList = [];

    #region Internal Events
    
    internal static event Action? NetworkSpawn;
    internal static event Action? NetworkDespawn;
    internal static event Action? NetworkTick;
    internal static event Action<ulong>? OnPlayerJoin;
    
    internal static event Action<string, byte[], ulong>? OnServerMessage; // identifier, data, originatorClientId
    internal static event Action<string, byte[], ulong>? OnClientMessage; // identifier, data, originatorClientId
    
    internal static event Action<string, byte[]>? OnVariableUpdate; // identifier, data
    internal static event Action<string, ulong>? GetVariableValue; // identifier, connectedClientId
    
    internal static event Action<string, ulong>? OnServerEvent; // identifier, originatorClientId
    internal static event Action<string, ulong>? OnClientEvent; // identifier, originatorClientId
    internal static event Action<string, double, ulong>? OnSyncedServerEvent; // identifier, time, originatorClientId
    internal static event Action<string, double, ulong>? OnSyncedClientEvent; // identifier, time, originatorClientId

    #endregion
}