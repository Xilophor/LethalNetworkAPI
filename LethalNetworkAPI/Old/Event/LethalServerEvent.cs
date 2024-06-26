using System.Collections;
using System.Collections.Generic;
using LethalNetworkAPI.Old.Networking;
using Unity.Netcode;
using UnityEngine;
using System;

namespace LethalNetworkAPI;

using System.Linq;
using Utils;

[Obsolete("Use LNetworkEvent instead.")]
public sealed class LethalServerEvent : LNetworkEventDepricated
{
    #region Public Constructors

    /// <summary>
    /// Create a new network event for the server.
    /// </summary>
    ///
    /// <param name="identifier">(<see cref="string"/>) An identifier for the event.</param>
    ///
    /// <param name="onReceived">Opt. (<see cref="Action{T}">Action&lt;ulong&gt;</see>)
    /// The method to run when an event is received from a client.</param>
    ///
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalServerEvent(string identifier, Action<ulong>? onReceived = null) : base(identifier)
    {
        NetworkHandler.OnServerEvent += ReceiveServerEvent;
        NetworkHandler.OnSyncedServerEvent += ReceiveSyncedServerEvent;

        OnReceived += onReceived;
    }

    #endregion

    #region Public Methods and Event

    /// <summary>
    /// Invoke event to a specific client.
    /// </summary>
    ///
    /// <param name="clientId">(<see cref="ulong">ulong</see>) The client to invoke the event to.</param>
    public void InvokeClient(ulong clientId)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;

        NetworkHandler.Instance!.EventClientRpc(Identifier, [clientId]);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Attempted to invoke Event to Client {clientId}" +
            $" with identifier: {Identifier}");
#endif
    }

    /// <summary>
    /// Invoke event to specific clients.
    /// </summary>
    ///
    /// <param name="clientIds">(<see cref="IEnumerable{UInt64}">IEnumerable&lt;ulong&gt;</see>)
    /// The clients to invoke the event to.</param>
    public void InvokeClients(IEnumerable<ulong> clientIds)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;

        NetworkHandler.Instance!.EventClientRpc(Identifier, clientIds.ToArray());

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Attempted to invoke Event to Clients {clientIds}" +
            $" with identifier: {Identifier}");
#endif
    }

    /// <summary>
    /// Invoke event to all clients.
    /// </summary>
    ///
    /// <param name="receiveOnHost">Opt. (<see cref="bool"/>)
    /// Whether the host client should receive as well.</param>
    public void InvokeAllClients(bool receiveOnHost = true)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;

        if (receiveOnHost)
            NetworkHandler.Instance!.EventClientRpc(Identifier, LNetworkUtils.AllConnectedClients);
        else
            NetworkHandler.Instance!.EventClientRpc(Identifier, LNetworkUtils.OtherConnectedClients);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Attempted to invoke Event to All Clients {receiveOnHost}" +
            $" with identifier: {Identifier}");
#endif
    }

    public void ClearSubscriptions() => OnReceived = delegate { };

    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when an event is received by the server.
    /// </summary>
    ///
    /// <typeparam name="ulong">The origin client ID.</typeparam>
    public event Action<ulong>? OnReceived;

    #endregion

    #region Private Methods and Fields

    private void ReceiveServerEvent(string identifier, ulong originClientId)
    {
        if (identifier != Identifier) return;

        OnReceived?.Invoke(originClientId);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received event with identifier: {Identifier}");
#endif
    }

    private void ReceiveSyncedServerEvent(string identifier, double time, ulong originatorClientId)
    {
        if (identifier != Identifier) return;

        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;

        NetworkManager.Singleton.StartCoroutine(WaitAndInvokeEvent((float)timeToWait, originatorClientId));

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received synced event with identifier: {Identifier}");
#endif
    }

    private IEnumerator WaitAndInvokeEvent(float timeToWait, ulong clientId)
    {
        if (timeToWait > 0)
            yield return new WaitForSeconds(timeToWait);

        OnReceived?.Invoke(clientId);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Invoked event with identifier: {Identifier}");
#endif
    }

    #endregion
}
