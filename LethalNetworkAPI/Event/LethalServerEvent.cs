using System.Collections;
using System.Collections.Generic;
using Unity.Collections;

namespace LethalNetworkAPI;

public class LethalServerEvent : NetworkEvent
{
    #region Public Constructors
    /// <summary>
    /// Create a new network event for the server.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the event.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalServerEvent(string identifier) : base(identifier)
    {
        NetworkHandler.OnServerEvent += ReceiveServerEvent;
        NetworkHandler.OnSyncedServerEvent += ReceiveSyncedServerEvent;
    }
    
    #endregion

    #region Public Methods and Event

    /// <summary>
    /// Invoke event to a specific client.
    /// </summary>
    /// <param name="clientId">(<see cref="UInt64">ulong</see>) The client to invoke the event to.</param>
    public void InvokeClient(ulong clientId)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;
        
        NetworkHandler.Instance!.EventClientRpc(Identifier, 
            clientRpcParams: GenerateClientParams(clientId));       
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Event to Client {clientId} with identifier: {Identifier}");
#endif
    }
    
    /// <summary>
    /// Invoke event to specific clients.
    /// </summary>
    /// <param name="clientIds">(<see cref="IEnumerable{UInt64}">IEnumerable&lt;ulong&gt;</see>) The clients to invoke the event to.</param>
    public void InvokeClients(IEnumerable<ulong> clientIds)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;
        
        NetworkHandler.Instance!.EventClientRpc(Identifier,
            clientRpcParams: GenerateClientParams(clientIds));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Event to Clients {clientIds} with identifier: {Identifier}");
#endif
    }

    /// <summary>
    /// Invoke event to all clients.
    /// </summary>
    /// <param name="receiveOnHost">(<see cref="bool"/>) Whether the host client should receive as well.</param>
    public void InvokeAllClients(bool receiveOnHost = true)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;
        
        if (receiveOnHost)
            NetworkHandler.Instance!.EventClientRpc(Identifier);
        else
            NetworkHandler.Instance!.EventClientRpc(Identifier,
                clientRpcParams: GenerateClientParamsExceptHost());
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to invoke Event to All Clients {receiveOnHost} with identifier: {Identifier}");
#endif
    }
    
    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when an event is received by the server.
    /// </summary>
    /// <typeparam name="ulong">The origin client ID.</typeparam>
    public event Action<ulong>? OnReceived;

    #endregion

    #region Private Methods and Fields

    private void ReceiveServerEvent(string identifier, ulong originClientId)
    {
        if (identifier != Identifier) return;

        OnReceived?.Invoke(originClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with identifier: {Identifier}");
#endif
    }
    
    private void ReceiveSyncedServerEvent(string identifier, double time, ulong originatorClientId)
    {
        if (identifier != Identifier || IsNetworkHandlerNull()) return;
        
        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;
        
        NetworkHandler.Instance!.SyncedEventClientRpc(identifier, time, originatorClientId,
            clientRpcParams: GenerateClientParamsExcept(originatorClientId));
        
        NetworkHandler.Instance.StartCoroutine(WaitAndInvokeEvent((float)timeToWait, originatorClientId));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received synced event with identifier: {Identifier}");
#endif
    }
    
    private IEnumerator WaitAndInvokeEvent(float timeToWait, ulong clientId)
    {
        if (timeToWait > 0)
            yield return new WaitForSeconds(timeToWait);
        
        OnReceived?.Invoke(clientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Invoked event with identifier: {Identifier}");
#endif
    }

    #endregion
}