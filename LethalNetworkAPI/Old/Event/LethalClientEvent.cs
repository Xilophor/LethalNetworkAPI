using System;
using System.Collections;
using LethalNetworkAPI.Old.Networking;
using Unity.Netcode;
using UnityEngine;

namespace LethalNetworkAPI;

[Obsolete("Use LNetworkEvent instead.")]
public sealed class LethalClientEvent : LNetworkEventDepricated
{
    #region Public Constructors

    /// <summary>
    /// Create a new network event for clients.
    /// </summary>
    ///
    /// <param name="identifier">(<see cref="string"/>) An identifier for the event.</param>
    ///
    /// <param name="onReceived">Opt. (<see cref="Action">Action</see>)
    /// The method to run when an event is received from the server.</param>
    ///
    /// <param name="onReceivedFromClient">Opt. (<see cref="Action{T}">Action&lt;ulong&gt;</see>)
    /// The method to run when an event is received from another client.</param>
    ///
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalClientEvent(string identifier,
        Action? onReceived = null,
        Action<ulong>? onReceivedFromClient = null)
        : base(identifier)
    {
        NetworkHandler.OnClientEvent += ReceiveClientEvent;
        NetworkHandler.OnSyncedClientEvent += ReceiveSyncedClientEvent;

        OnReceived += onReceived;
        OnReceivedFromClient += onReceivedFromClient;

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"NetworkEvent with identifier \"{Identifier}\" has been created.");
#endif
    }

    #endregion

    #region Public Methods and Events
    /// <summary>
    /// Invoke event to the server/host.
    /// </summary>
    public void InvokeServer()
    {
        if (IsNetworkHandlerNull()) return;

        NetworkHandler.Instance!.EventServerRpc(Identifier);
    }

    /// <summary>
    /// Invoke event to all clients.
    /// </summary>
    ///
    /// <param name="includeLocalClient">Opt. (<see cref="bool"/>)
    /// If the local client event should be invoked.</param>
    ///
    /// <param name="waitForServerResponse">Opt. (<see cref="bool"/>)
    /// If the local client should wait for a server response before
    /// invoking the <see cref="OnReceivedFromClient"/> event.</param>
    ///
    /// <remarks><paramref name="waitForServerResponse"/> will only be considered
    /// if <paramref name="includeLocalClient"/> is set to true.</remarks>
    public void InvokeAllClients(bool includeLocalClient = true, bool waitForServerResponse = false)
    {
        if (IsNetworkHandlerNull()) return;

        NetworkHandler.Instance!.EventServerRpc(Identifier, toOtherClients: true);

        if(includeLocalClient)
            OnReceivedFromClient?.Invoke(NetworkManager.Singleton.LocalClientId);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Attempted to invoke Event to All Clients {includeLocalClient}" +
            $" with identifier: {Identifier}");
#endif
    }

    //? Synced Events

    /// <summary>
    /// Invoke synchronized event to all clients.
    /// </summary>
    public void InvokeAllClientsSynced()
    {
        if (IsNetworkHandlerNull()) return;

        var time = NetworkManager.Singleton.LocalTime.Time;

        NetworkHandler.Instance!.SyncedEventServerRpc(Identifier, time);
        ReceiveClientEvent(Identifier, NetworkManager.Singleton.LocalClientId);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Attempted to invoke Synced Event to Other Clients with identifier: {Identifier}");
#endif
    }

    public void ClearSubscriptions()
    {
        OnReceived = delegate { };
        OnReceivedFromClient = delegate { };
    }

    /// <summary>
    /// The callback to invoke when an event is received from the server.
    /// </summary>
    public event Action? OnReceived;

    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when an event is received from another client.
    /// </summary>
    ///
    /// <typeparam name="ulong">The origin client ID.</typeparam>
    public event Action<ulong>? OnReceivedFromClient;

    #endregion

    #region Private Methods and Fields

    private void ReceiveClientEvent(string identifier, ulong originatorClientId)
    {
        if (identifier != Identifier) return;

        if (originatorClientId == 99999)
            OnReceived?.Invoke();
        else
            OnReceivedFromClient?.Invoke(originatorClientId);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received event with identifier: {Identifier}");
#endif
    }

    private void ReceiveSyncedClientEvent(string identifier, double time, ulong originatorClientId)
    {
        if (identifier != Identifier || NetworkHandler.Instance == null) return;

        var timeToWait = time - NetworkManager.Singleton.ServerTime.Time;

        NetworkManager.Singleton.StartCoroutine(
            WaitAndInvokeEvent((float)timeToWait, originatorClientId));

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received synced event with identifier: {Identifier}");
#endif
    }

    private IEnumerator WaitAndInvokeEvent(float timeToWait, ulong originatorClientId)
    {
        if (timeToWait > 0)
            yield return new WaitForSeconds(timeToWait);

        ReceiveClientEvent(Identifier, originatorClientId);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Invoked event with identifier: {Identifier}");
#endif
    }

    #endregion
}
