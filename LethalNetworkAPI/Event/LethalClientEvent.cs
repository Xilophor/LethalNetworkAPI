using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

// ReSharper disable InvalidXmlDocComment

namespace LethalNetworkAPI;

public class LethalClientEvent
{
    #region Public Constructors
    /// <summary>
    /// Create a new network event for clients.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the event.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalClientEvent(string identifier)
    {
        _eventIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.evt.{identifier}";
        NetworkHandler.OnClientEvent += ReceiveClientEvent;
        NetworkHandler.OnSyncedClientEvent += ReceiveSyncedClientEvent;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkEvent with identifier \"{_eventIdentifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Events
    /// <summary>
    /// Invoke event to the server/host.
    /// </summary>
    public void InvokeServer()
    {
        if (NetworkHandler.Instance != null) 
            NetworkHandler.Instance.EventServerRpc(_eventIdentifier);
    }

    /// <summary>
    /// Invoke event to all clients.
    /// </summary>
    /// <param name="includeLocalClient">Opt. (<see cref="bool"/>) If the local client event should be invoked.</param>
    /// <param name="waitForServerResponse">Opt. (<see cref="bool"/>) If the local client should wait for a server response before invoking the <see cref="OnReceivedFromClient"/> event.</param>
    /// <remarks><paramref name="waitForServerResponse"/> will only be considered if <paramref name="includeLocalClient"/> is set to true.</remarks>
    public void InvokeAllClients(bool includeLocalClient = true, bool waitForServerResponse = false)
    {
        if (NetworkHandler.Instance == null) return;
        
        NetworkHandler.Instance.EventServerRpc(_eventIdentifier, toOtherClients: true, 
            sendToOriginator: (includeLocalClient && waitForServerResponse));
        
        if(includeLocalClient && !waitForServerResponse)
            OnReceivedFromClient?.Invoke(NetworkManager.Singleton.LocalClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Event to All Clients {includeLocalClient} with identifier: {_eventIdentifier}");
#endif
    }
    
    //? Synced Events
    
    /// <summary>
    /// Invoke synchronized event to all clients.
    /// </summary>
    public void InvokeAllClientsSynced()
    {
        var time = NetworkManager.Singleton.LocalTime.Time;

        if (NetworkHandler.Instance == null) return;
        
        NetworkHandler.Instance.SyncedEventServerRpc(_eventIdentifier, time);
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent(0, NetworkManager.Singleton.LocalClientId));

#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Synced Event to Other Clients with identifier: {_eventIdentifier}");
#endif
    }
    
    /// <summary>
    /// The callback to invoke when an event is received from the server.
    /// </summary>
    public event Action? OnReceived;
    
    /// <summary>
    /// The callback to invoke when an event is received from another client.
    /// </summary>
    /// <typeparam name="clientId">(<see cref="UInt64">ulong</see>) The origin client.</typeparam>
    public event Action<ulong>? OnReceivedFromClient;

    #endregion
    
    private void ReceiveClientEvent(string identifier, ulong originatorClientId)
    {
        if (identifier != _eventIdentifier) return;
        
        if (originatorClientId == 99999)
            OnReceived?.Invoke();
        else
            OnReceivedFromClient?.Invoke(originatorClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with identifier: {_eventIdentifier}");
#endif
    }
    
    private void ReceiveSyncedClientEvent(string identifier, double time, ulong originatorClientId)
    {
        if (identifier != _eventIdentifier || NetworkHandler.Instance == null) return;
        
        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;
        
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent((float)timeToWait, originatorClientId));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received synced event with identifier: {_eventIdentifier}");
#endif
    }
    
    private IEnumerator WaitAndInvokeEvent(float timeToWait, ulong originatorClientId)
    {
        if (timeToWait > 0)
            yield return new WaitForSeconds(timeToWait);
        
        if (originatorClientId == 99999)
            OnReceived?.Invoke();
        else
            OnReceivedFromClient?.Invoke(originatorClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Invoked event with identifier: {_eventIdentifier}");
#endif
    }
    
    private readonly string _eventIdentifier;
}