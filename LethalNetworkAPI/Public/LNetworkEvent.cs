namespace LethalNetworkAPI;

using System;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Internal;
using Unity.Netcode;
using Utils;

/// <summary>
/// Used to invoke events between clients (and the server/host).
/// </summary>
/// <remarks>Will not interact with <see cref="LNetworkMessage{TData}"/>, <see cref="LNetworkVariable{TData}"/>,
/// nor with other mods - even if the identifier is not unique.</remarks>
public sealed class LNetworkEvent
{
    internal string Identifier { get; }

    #region Constructor & Factory

    /// <summary>
    /// Creates a new LNetworkEvent if it doesn't already exist,
    /// otherwise returns the existing message of the same identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the NetworkEvent.</param>
    /// <param name="onServerReceived">[Opt.] The method to run when the server receives an event.</param>
    /// <param name="onClientReceived">[Opt.] The method to run when the client receives an event.</param>
    /// <param name="onClientReceivedFromClient">[Opt.] The method to run when the client receives an event from another client.</param>
    /// <returns>The LNetworkEvent.</returns>
    public static LNetworkEvent Connect(
        string identifier,
        Action<ulong>? onServerReceived = null,
        Action? onClientReceived = null,
        Action<ulong>? onClientReceivedFromClient = null)
    {
        string actualIdentifier;

        try
        {
            actualIdentifier = $"{LNetworkUtils.GetModGuid(2)}.{identifier}";
        }
        catch (Exception e)
        {
            LethalNetworkAPIPlugin.Logger.LogError($"Unable to find Mod Guid! To still work, this Event will only use the given identifier. " +
                                                   $"Warning: This may cause collision with another mod's NetworkEvent! Stack Trace: {e}");
            actualIdentifier = identifier;
        }

        if (!UnnamedMessageHandler.LNetworkEvents.TryGetValue(actualIdentifier, out var networkEvent))
            return new LNetworkEvent(actualIdentifier, onServerReceived, onClientReceived,
                onClientReceivedFromClient);

        networkEvent.OnServerReceived += onServerReceived;
        networkEvent.OnClientReceived += onClientReceived;
        networkEvent.OnClientReceivedFromClient += onClientReceivedFromClient;

        return networkEvent;
    }

    /// <summary>
    /// Creates a new LNetworkEvent. If it already exists, an exception will be thrown.
    /// </summary>
    /// <param name="identifier">The identifier of the NetworkEvent.</param>
    /// <param name="onServerReceived">[Opt.] The method to run when the server receives an event.</param>
    /// <param name="onClientReceived">[Opt.] The method to run when the client receives an event.</param>
    /// <param name="onClientReceivedFromClient">[Opt.] The method to run when the client receives an event from another client.</param>
    /// <returns>The LNetworkEvent.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the event already exists.</exception>
    public static LNetworkEvent Create(
        string identifier,
        Action<ulong>? onServerReceived = null,
        Action? onClientReceived = null,
        Action<ulong>? onClientReceivedFromClient = null)
    {
        string actualIdentifier;

        try
        {
            actualIdentifier = $"{LNetworkUtils.GetModGuid(2)}.{identifier}";
        }
        catch (Exception e)
        {
            LethalNetworkAPIPlugin.Logger.LogError($"Unable to find Mod Guid! To still work, this Event will only use the given identifier. " +
                                                   $"Warning: This may cause collision with another mod's NetworkEvent! Stack Trace: {e}");
            actualIdentifier = identifier;
        }

        return new LNetworkEvent(actualIdentifier, onServerReceived, onClientReceived,
            onClientReceivedFromClient);
    }

    private LNetworkEvent(
        string identifier,
        Action<ulong>? onServerReceived = null,
        Action? onClientReceived = null,
        Action<ulong>? onClientReceivedFromClient = null)
    {
        if (UnnamedMessageHandler.LNetworkEvents.TryGetValue(identifier, out _))
        {
            throw new InvalidOperationException
            (
                $"An event with the identifier {identifier} already exists! " +
                "Please use a different identifier."
            );
        }

        this.Identifier = identifier;

        this.OnServerReceived += onServerReceived;
        this.OnClientReceived += onClientReceived;
        this.OnClientReceivedFromClient += onClientReceivedFromClient;

        UnnamedMessageHandler.LNetworkEvents.Add(identifier, this);
    }

    #endregion

    #region Public Varaibles and Events

    /// <summary>
    /// A callback that runs when the server receives an event.
    /// </summary>
    public Action<ulong>? OnServerReceived { get; private set; } = delegate { };

    /// <summary>
    /// A callback that runs when the client receives an event.
    /// </summary>
    public Action? OnClientReceived { get; private set; } = delegate { };

    /// <summary>
    /// A callback that runs when the client receives an event from another client.
    /// </summary>
    public Action<ulong>? OnClientReceivedFromClient { get; private set; } = delegate { };

    #endregion

    #region Internal Methods

    internal void InvokeOnServerReceived(ulong clientId) =>
        this.OnServerReceived?.Invoke(clientId);

    internal void InvokeOnClientReceived() =>
        this.OnClientReceived?.Invoke();

    internal void InvokeOnClientReceivedFromClient(ulong clientId) =>
        this.OnClientReceivedFromClient?.Invoke(clientId);

    #endregion

    #region Public Methods

    /// <summary>
    /// Clear all subscriptions to all callbacks on this <see cref="LNetworkEvent"/>.
    /// </summary>
    public void ClearSubscriptions()
    {
        this.OnServerReceived = delegate { };
        this.OnClientReceived = delegate { };
        this.OnClientReceivedFromClient = delegate { };
    }

    #region Server Methods

    /// <summary>
    /// Server-only method to invoke the event on a specific client.
    /// </summary>
    /// <param name="clientGuid">The NGO guid of the client to invoke the event on.</param>
    public void SendClient(ulong clientGuid) =>
        this.SendClients([clientGuid]);

    /// <summary>
    /// Server-only method to invoke the event on a specific client.
    /// </summary>
    /// <param name="playerId">The in-game ids of the client to invoke the event on.</param>
    public void SendClient(int playerId) =>
        this.SendClients([LNetworkUtils.GetClientGuid(playerId)]);

    /// <summary>
    /// Server-only method to invoke the event on the specified clients.
    /// </summary>
    /// <param name="clientGuidArray">[Opt.] The NGO guids of the clients to invoke the event on.</param>
    public void SendClients(ulong[] clientGuidArray)
    {
        if (!LNetworkUtils.IsConnected) throw new NetworkConfigurationException
        (
            "Attempting to use LNetworkMessage method while not connected to a server."
        );

        if (!LNetworkUtils.IsHostOrServer) throw new Exception
        (
            "Attempting to use LNetworkMessage Server-Only method while not the host."
        );

        if (!clientGuidArray.Any()) return;

        if (UnnamedMessageHandler.Instance == null) throw new NetworkConfigurationException
        (
            "The NamedMessageHandler is null. Shit's fucked! " +
            "Please send this log to the LethalNetworkAPI developer."
        );

        UnnamedMessageHandler.Instance.SendMessageToClients(
            new MessageData
            (
                this.Identifier,
                EMessageType.Event | EMessageType.ClientMessage
            ),
            clientGuidArray);
    }

    /// <summary>
    /// Server-only method to invoke the event on the specified clients.
    /// </summary>
    /// <param name="playerIdArray">[Opt.] The in-game ids of the clients to invoke the event on.</param>
    public void SendClients(int[] playerIdArray) =>
        this.SendClients(playerIdArray.Select(LNetworkUtils.GetClientGuid).ToArray());

    /// <summary>
    /// Server-only method to invoke the event on all clients.
    /// </summary>
    public void SendClients() =>
        this.SendClients(LNetworkUtils.AllConnectedClients);

    #endregion

    #region Client Methods

    /// <summary>
    /// Client method to invoke the event on the server/host.
    /// </summary>
    public void SendServer()
    {
        if (!LNetworkUtils.IsConnected) throw new NetworkConfigurationException
        (
            "Attempting to use LNetworkMessage method while not connected to a server."
        );

        if (UnnamedMessageHandler.Instance == null) throw new NetworkConfigurationException
        (
            "The NamedMessageHandler is null. Shit's fucked! " +
            "Please send this log to the LethalNetworkAPI developer."
        );

        UnnamedMessageHandler.Instance.SendMessageToServer(
            new MessageData
            (
                this.Identifier,
                EMessageType.Event | EMessageType.ClientMessage
            ));
    }

    /// <summary>
    /// Client method to invoke the event on the specified clients.
    /// </summary>
    /// <param name="clientGuidArray">[Opt.] The NGO guids of the clients to invoke the event on.</param>
    public void SendOtherClients(ulong[] clientGuidArray)
    {
        if (!LNetworkUtils.IsConnected) throw new NetworkConfigurationException
        (
            "Attempting to use LNetworkMessage method while not connected to a server."
        );

        if (!clientGuidArray.Any()) return;

        if (UnnamedMessageHandler.Instance == null) throw new NetworkConfigurationException
        (
            "The NamedMessageHandler is null. Shit's fucked! " +
            "Please send this log to the LethalNetworkAPI developer."
        );

        UnnamedMessageHandler.Instance.SendMessageToClients(
            new MessageData
            (
                this.Identifier,
                EMessageType.Event | EMessageType.ClientMessageToClient
            ),
            clientGuidArray);
    }

    /// <summary>
    /// Client method to invoke the event on the specified clients.
    /// </summary>
    /// <param name="playerIdArray">[Opt.] The in-game ids of the clients to invoke the event on.</param>
    public void SendOtherClients(int[] playerIdArray) =>
        this.SendOtherClients(playerIdArray.Select(LNetworkUtils.GetClientGuid).ToArray());

    /// <summary>
    /// Client method to invoke the event on all other clients.
    /// </summary>
    public void SendOtherClients() =>
        this.SendOtherClients(LNetworkUtils.OtherConnectedClients);

    #endregion

    #endregion
}
