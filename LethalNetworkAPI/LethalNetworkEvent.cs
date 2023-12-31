using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LethalNetworkAPI.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LethalNetworkAPI;

public class LethalNetworkEvent
{
        #region Public Constructors
    /// <summary>
    /// Create a new network event.
    /// </summary>
    /// <param name="guid">An identifier for the event. GUIDs are specific to a per-mod basis.</param>
    /// <example><code> customEvent = new LethalNetworkEvent(guid: "customStringMessageGuid");</code></example>
    public LethalNetworkEvent(string guid)
    {
        _eventGuid = $"{Assembly.GetCallingAssembly().GetName().Name}.evt.{guid}";
        NetworkHandler.OnEvent += ReceiveEvent;
        NetworkHandler.OnSyncedServerEvent += ReceiveSyncedServerEvent;
        NetworkHandler.OnSyncedClientEvent += ReceiveSyncedClientEvent;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkEvent with guid \"{_eventGuid}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Event
    /// <summary>
    /// Send event to the server/host.
    /// </summary>
    public void SendServer()
    {
        NetworkHandler.Instance.EventServerRpc(_eventGuid);
    }

    /// <summary>
    /// Send event to a specific client.
    /// </summary>
    /// <param name="clientId">The client to send the event to.</param>
    public void SendClient(ulong clientId)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return;
        
        NetworkHandler.Instance.EventClientRpc(_eventGuid, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = new NativeArray<ulong>(new []{clientId}, Allocator.Persistent) } } );       
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to send Event to Client {clientId} with guid: {_eventGuid}");
#endif
    }
    
    /// <summary>
    /// Send event to specific clients.
    /// </summary>
    /// <param name="clientIds">The clients to send the event to.</param>
    public void SendClients(IEnumerable<ulong> clientIds)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;

        var allowedClientIds = new NativeArray<ulong>(clientIds.Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray(), Allocator.Persistent);
        if (!allowedClientIds.Any()) return;
        
        NetworkHandler.Instance.EventClientRpc(_eventGuid, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = allowedClientIds } } );
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to send Event to Clients {clientIds} with guid: {_eventGuid}");
#endif
    }

    /// <summary>
    /// Send event to all clients.
    /// </summary>
    /// <param name="receiveOnHost">Whether the host client should receive as well. Only set to <c>false</c> when absolutely necessary</param>
    public void SendAllClients(bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        
        if (receiveOnHost)
            NetworkHandler.Instance.EventClientRpc(_eventGuid);
        else
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);
            if (!clientIds.Any()) return;
            
            NetworkHandler.Instance.EventClientRpc(_eventGuid, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } } );
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to send Event to All Clients {receiveOnHost} with guid: {_eventGuid}");
#endif
    }
    
    //? Synced Events
    
    /// <summary>
    /// Send synchronized event to other clients.
    /// </summary>
    public void SendOtherClientsSynced()
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;

        var time = NetworkManager.Singleton.LocalTime.Time;
        
        NetworkHandler.Instance.SyncedEventServerRpc(_eventGuid, time);
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent(0, false));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to send Synced Event to Other Clients with guid: {_eventGuid}");
#endif
    }
    
    /// <summary>
    /// Send synchronized event to all clients.
    /// </summary>
    /// <param name="receiveOnHost">Whether the host client should receive as well. Only set to <c>false</c> when absolutely necessary</param>
    public void SendAllClientsSynced(bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        
        if (receiveOnHost)
            NetworkHandler.Instance.EventClientRpc(_eventGuid);
        else
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);
            if (!clientIds.Any()) return;
            
            NetworkHandler.Instance.EventClientRpc(_eventGuid, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } } );
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to send Synced Event to All Clients {receiveOnHost} with guid: {_eventGuid}");
#endif
    }
    
    /// <summary>
    /// The callback to invoke when an event is received by the server.
    /// </summary>
    /// <example><code>customEvent.OnServerReceived += CustomMethod; &#xA; &#xA;private static void CustomMethod()</code></example>
    public event Action OnServerReceived;
    
    
    /// <summary>
    /// The callback to invoke when an event is received by the client.
    /// </summary>
    /// <example><code>customEvent.OnClientReceived += CustomMethod; &#xA; &#xA;private static void CustomMethod()</code></example>
    public event Action OnClientReceived;

    #endregion

    private void ReceiveEvent(string guid, bool isServerEvent)
    {
        if (guid != _eventGuid) return;

        if (isServerEvent)
            OnServerReceived?.Invoke();
        else
            OnClientReceived?.Invoke();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with guid: {_eventGuid}");
#endif
    }
    
    private void ReceiveSyncedServerEvent(string guid, double time, ulong originatorClientId)
    {
        if (guid != _eventGuid) return;
        
        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;
        
        var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != originatorClientId).ToArray(), Allocator.Persistent);
        if (!clientIds.Any()) return;
        
        NetworkHandler.Instance.SyncedEventClientRpc(guid, time, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } } );
        
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent((float)timeToWait, true));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received synced event with guid: {_eventGuid}");
#endif
    }
    
    private void ReceiveSyncedClientEvent(string guid, double time)
    {
        if (guid != _eventGuid) return;
        
        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;
        
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent((float)timeToWait, false));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received synced event with guid: {_eventGuid}");
#endif
    }
    
    private IEnumerator WaitAndInvokeEvent(float timeToWait, bool isServerEvent)
    {
        if (timeToWait > 0)
            yield return new WaitForSeconds(timeToWait);
        
        if (isServerEvent)
            OnServerReceived?.Invoke();
        else
            OnClientReceived?.Invoke();
        
#if DEBUG
        Plugin.Logger.LogDebug($"Invoked event with guid: {_eventGuid}");
#endif
    }
    
    private readonly string _eventGuid;
}