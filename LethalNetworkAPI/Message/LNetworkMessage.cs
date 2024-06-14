namespace LethalNetworkAPI.Message;

using System;
using System.Linq;
using Internal;
using Unity.Netcode;
using Utils;

internal interface INetMessage
{
    internal void InvokeOnServerReceived(object? data, ulong clientId);
    internal void InvokeOnClientReceived(object? data);
    internal void InvokeOnClientReceivedFromClient(object? data, ulong clientId);
}

public sealed class LNetworkMessage<TData> : INetMessage
{
    internal string Identifier { get; }

    #region Constructor

    public LNetworkMessage(
        string identifier,
        Action<TData, ulong>? onServerReceived = null,
        Action<TData>? onClientReceived = null,
        Action<TData, ulong>? onClientReceivedFromClient = null)
    {
        this.Identifier = identifier;

        this.OnServerReceived += onServerReceived;
        this.OnClientReceived += onClientReceived;
        this.OnClientReceivedFromClient += onClientReceivedFromClient;

        UnnamedMessageHandler.LNetworkMessages.Add(identifier, this);
    }

    #endregion

    #region Public Varaibles and Events

    public Action<TData, ulong>? OnServerReceived { get; set; } = delegate { };
    public Action<TData>? OnClientReceived { get; set; } = delegate { };
    public Action<TData, ulong>? OnClientReceivedFromClient { get; set; } = delegate { };

    #endregion

    #region Internal Methods

    void INetMessage.InvokeOnServerReceived(object? data, ulong clientId) =>
        this.OnServerReceived?.Invoke((TData)data!, clientId);

    void INetMessage.InvokeOnClientReceived(object? data) =>
        this.OnClientReceived?.Invoke((TData)data!);

    void INetMessage.InvokeOnClientReceivedFromClient(object? data, ulong clientId) =>
        this.OnClientReceivedFromClient?.Invoke((TData)data!, clientId);

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
    /// Server-only method to send data to a specific client.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="clientGuid">The NGO guid of the client to send the data to.</param>
    public void SendClient(TData data, ulong clientGuid) =>
        this.SendClients(data, [clientGuid]);

    /// <summary>
    /// Server-only method to send data to a specific client.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="playerId">The in-game ids of the client to send the data to.</param>
    public void SendClient(TData data, int playerId) =>
        this.SendClients(data, [LNetworkUtils.GetClientGuid(playerId)]);

    /// <summary>
    /// Server-only method to send data to the specified clients.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="clientGuidArray">[Opt.] The NGO guids of the clients to send the data to.</param>
    public void SendClients(TData data, ulong[] clientGuidArray)
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
                EMessageType.Message | EMessageType.ServerMessage,
                data
            ),
            clientGuidArray);
    }

    /// <summary>
    /// Server-only method to send data to the specified clients.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="playerIdArray">[Opt.] The in-game ids of the clients to send the data to.</param>
    public void SendClients(TData data, int[] playerIdArray) =>
        this.SendClients(data, playerIdArray.Select(LNetworkUtils.GetClientGuid).ToArray());

    /// <summary>
    /// Server-only method to send data to all clients.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <remarks>This method will not send data if the client is not connected to a server.</remarks>
    public void SendClients(TData data) =>
        this.SendClients(data, LNetworkUtils.AllConnectedClients);

    #endregion

    #region Client Methods

    /// <summary>
    /// Client method to send data to the server/host.
    /// </summary>
    /// <param name="data">The data to send.</param>
    public void SendServer(TData data)
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
                EMessageType.Message | EMessageType.ClientMessage,
                data
            ));
    }

    /// <summary>
    /// Client method to send data to the specified clients.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="clientGuidArray">[Opt.] The NGO guids of the clients to send the data to.</param>
    public void SendOtherClients(TData data, ulong[] clientGuidArray)
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
                EMessageType.Message | EMessageType.ClientMessageToClient,
                data
            ),
            clientGuidArray);
    }

    /// <summary>
    /// Client method to send data to the specified clients.
    /// </summary>
    /// <param name="data">The data to send.</param>
    /// <param name="playerIdArray">[Opt.] The in-game ids of the clients to send the data to.</param>
    public void SendOtherClients(TData data, int[] playerIdArray) =>
        this.SendOtherClients(data, playerIdArray.Select(LNetworkUtils.GetClientGuid).ToArray());

    /// <summary>
    /// Client method to send data to all clients.
    /// </summary>
    /// <param name="data">The data to send.</param>
    public void SendOtherClients(TData data) =>
        this.SendOtherClients(data, LNetworkUtils.OtherConnectedClients);

    #endregion

    #endregion
}
