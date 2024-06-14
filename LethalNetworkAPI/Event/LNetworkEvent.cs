namespace LethalNetworkAPI.Event;

using System;
using System.Linq;
using Internal;
using Unity.Netcode;
using Utils;

public sealed class LNetworkEvent
{
    internal string Identifier { get; }

    #region Constructor

    public LNetworkEvent(string identifier)
    {
        this.Identifier = identifier;
        UnnamedMessageHandler.LNetworkEvents.Add(identifier, this);
    }

    #endregion

    #region Public Varaibles and Events

    public Action<ulong>? OnServerReceived { get; set; } = delegate { };
    public Action? OnClientReceived { get; set; } = delegate { };
    public Action<ulong>? OnClientReceivedFromClient { get; set; } = delegate { };

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
