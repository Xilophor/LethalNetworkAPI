using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

// ReSharper disable InvalidXmlDocComment

namespace LethalNetworkAPI;

public class LethalNetworkEvent
{
        #region Public Constructors
    /// <summary>
    /// Create a new network event.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the event.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalNetworkEvent(string identifier)
    {
        _eventIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.evt.{identifier}";
        NetworkHandler.OnServerEvent += ReceiveServerEvent;
        NetworkHandler.OnClientEvent += ReceiveClientEvent;
        NetworkHandler.OnSyncedServerEvent += ReceiveSyncedServerEvent;
        NetworkHandler.OnSyncedClientEvent += ReceiveSyncedClientEvent;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkEvent with identifier \"{_eventIdentifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Event
    /// <summary>
    /// Invoke event to the server/host.
    /// </summary>
    public void InvokeServer()
    {
        if (NetworkHandler.Instance != null) 
            NetworkHandler.Instance.EventServerRpc(_eventIdentifier);
    }

    /// <summary>
    /// Invoke event to a specific client.
    /// </summary>
    /// <param name="clientId">(<see cref="UInt64">ulong</see>) The client to invoke the event to.</param>
    public void InvokeClient(ulong clientId)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) || NetworkHandler.Instance == null) return;
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return;
        
        NetworkHandler.Instance.EventClientRpc(_eventIdentifier,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = new NativeArray<ulong>(new []{clientId}, Allocator.Persistent) } } );       
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Event to Client {clientId} with identifier: {_eventIdentifier}");
#endif
    }
    
    /// <summary>
    /// Invoke event to specific clients.
    /// </summary>
    /// <param name="clientIds">(<see cref="IEnumerable{UInt64}">IEnumerable&lt;ulong&gt;</see>) The clients to invoke the event to.</param>
    public void InvokeClients(IEnumerable<ulong> clientIds)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) || NetworkHandler.Instance == null) return;

        var allowedClientIds = new NativeArray<ulong>(clientIds
            .Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray(), Allocator.Persistent);
        if (!allowedClientIds.Any()) return;
        
        NetworkHandler.Instance.EventClientRpc(_eventIdentifier,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = allowedClientIds } } );
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Event to Clients {clientIds} with identifier: {_eventIdentifier}");
#endif
    }

    /// <summary>
    /// Invoke event to all clients.
    /// </summary>
    /// <param name="receiveOnHost">(<see cref="bool"/>) Whether the host client should receive as well.</param>
    public void InvokeAllClients(bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) || NetworkHandler.Instance == null) return;
        
        if (receiveOnHost)
            NetworkHandler.Instance.EventClientRpc(_eventIdentifier);
        else
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);
            if (!clientIds.Any()) return;
            
            NetworkHandler.Instance.EventClientRpc(_eventIdentifier, 
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } } );
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Event to All Clients {receiveOnHost} with identifier: {_eventIdentifier}");
#endif
    }
    
    //? Synced Events
    
    /// <summary>
    /// Invoke synchronized event to other clients.
    /// </summary>
    public void InvokeOtherClientsSynced()
    {
        var time = NetworkManager.Singleton.LocalTime.Time;

        if (NetworkHandler.Instance == null) return;
        
        NetworkHandler.Instance.SyncedEventServerRpc(_eventIdentifier, time);
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent(0));

#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Synced Event to Other Clients with identifier: {_eventIdentifier}");
#endif
    }
    
    /// <summary>
    /// Invoke synchronized event to all clients.
    /// </summary>
    /// <param name="receiveOnHost">(<see cref="bool"/>) Whether the host client should receive as well.</param>
    public void InvokeAllClientsSynced(bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) || NetworkHandler.Instance == null) return;
        
        if (receiveOnHost)
        {
            NetworkHandler.Instance.SyncedEventClientRpc(_eventIdentifier, NetworkManager.Singleton.ServerTime.Time);
        }
        else
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);
            if (!clientIds.Any()) return;

            NetworkHandler.Instance.SyncedEventClientRpc(_eventIdentifier, NetworkManager.Singleton.ServerTime.Time, 
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } });
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Synced Event to All Clients {receiveOnHost} with identifier: {_eventIdentifier}");
#endif
    }
    
    /// <summary>
    /// The callback to invoke when an event is received by the server.
    /// </summary>
    public event Action? OnServerReceived;
    
    /// <summary>
    /// The callback to invoke when an event is received by the server.
    /// </summary>
    /// <typeparam name="clientId">(<see cref="UInt64">ulong</see>) The origin client.</typeparam>
    public event Action<ulong>? OnServerReceivedFrom;
    
    
    /// <summary>
    /// The callback to invoke when an event is received by the client.
    /// </summary>
    public event Action? OnClientReceived;

    #endregion

    private void ReceiveServerEvent(string identifier, ulong originClientId)
    {
        if (identifier != _eventIdentifier) return;

        OnServerReceived?.Invoke();
        OnServerReceivedFrom?.Invoke(originClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with identifier: {_eventIdentifier}");
#endif
    }
    
    private void ReceiveClientEvent(string identifier)
    {
        if (identifier != _eventIdentifier) return;
        
        OnClientReceived?.Invoke();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with identifier: {_eventIdentifier}");
#endif
    }
    
    private void ReceiveSyncedServerEvent(string identifier, double time, ulong originatorClientId)
    {
        if (identifier != _eventIdentifier || NetworkHandler.Instance == null) return;
        
        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;
        
        var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
            .Where(i => i != originatorClientId).ToArray(), Allocator.Persistent);
        if (!clientIds.Any()) return;
        
        NetworkHandler.Instance.SyncedEventClientRpc(identifier, time,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } } );
        
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent((float)timeToWait, originatorClientId));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received synced event with identifier: {_eventIdentifier}");
#endif
    }
    
    private void ReceiveSyncedClientEvent(string identifier, double time)
    {
        if (identifier != _eventIdentifier || NetworkHandler.Instance == null) return;
        
        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;
        
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent((float)timeToWait));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received synced event with identifier: {_eventIdentifier}");
#endif
    }
    
    private IEnumerator WaitAndInvokeEvent(float timeToWait, ulong clientId = 99999)
    {
        if (timeToWait > 0)
            yield return new WaitForSeconds(timeToWait);
        
        if (clientId != 99999)
        {
            OnServerReceived?.Invoke();
            OnServerReceivedFrom?.Invoke(clientId);
        }
        else
            OnClientReceived?.Invoke();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Invoked event with identifier: {_eventIdentifier}");
#endif
    }
    
    private readonly string _eventIdentifier;
}