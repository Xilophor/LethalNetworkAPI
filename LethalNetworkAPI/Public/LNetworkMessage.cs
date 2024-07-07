namespace LethalNetworkAPI;

using System;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Internal;
using Unity.Netcode;
using Utils;

internal interface INetMessage
{
    internal void InvokeOnServerReceived(object? data, ulong clientId);
    internal void InvokeOnClientReceived(object? data);
    internal void InvokeOnClientReceivedFromClient(object? data, ulong clientId);
}

/// <summary>
/// Used to send data between clients (and the server/host).
/// </summary>
/// <typeparam name="TData">The type of data to send.</typeparam>
/// <remarks>Will not interact with <see cref="LNetworkEvent"/>, <see cref="LNetworkVariable{TData}"/>,
/// nor with other mods - even if the identifier is not unique.</remarks>
public sealed class LNetworkMessage<TData> : INetMessage
{
    internal string Identifier { get; }

    #region Constructor & Factory

    /// <summary>
    /// Create a new <see cref="LNetworkMessage{TData}"/> if it doesn't already exist,
    /// otherwise return the existing message of the same identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the <see cref="LNetworkMessage{TData}"/>.</param>
    /// <param name="onServerReceived">[Opt.] The method to run when the server receives a message.</param>
    /// <param name="onClientReceived">[Opt.] The method to run when the client receives a message.</param>
    /// <param name="onClientReceivedFromClient">[Opt.] The method to run when the client receives a message from another client.</param>
    /// <returns>The <see cref="LNetworkMessage{TData}"/>.</returns>
    public static LNetworkMessage<TData> Connect(
        string identifier,
        Action<TData, ulong>? onServerReceived = null,
        Action<TData>? onClientReceived = null,
        Action<TData, ulong>? onClientReceivedFromClient = null)
    {
        string actualIdentifier;

        try
        {
            actualIdentifier = $"{LNetworkUtils.GetModGuid(2)}.{identifier}";
        }
        catch (Exception e)
        {
            LethalNetworkAPIPlugin.Logger.LogError($"Unable to find Mod Guid! To still work, this Message will only use the given identifier. " +
                                                   $"Warning: This may cause collision with another mod's NetworkMessage! Stack Trace: {e}");
            actualIdentifier = identifier;
        }

        if (!UnnamedMessageHandler.LNetworkMessages.TryGetValue(actualIdentifier, out var message))
            return new LNetworkMessage<TData>(actualIdentifier, onServerReceived, onClientReceived,
                onClientReceivedFromClient);

        var networkMessage = (LNetworkMessage<TData>)message;

        networkMessage.OnServerReceived += onServerReceived;
        networkMessage.OnClientReceived += onClientReceived;
        networkMessage.OnClientReceivedFromClient += onClientReceivedFromClient;

        return networkMessage;
    }

    /// <summary>
    /// Create a new <see cref="LNetworkMessage{TData}"/>. If it already exists, an exception will be thrown.
    /// </summary>
    /// <param name="identifier">The identifier of the <see cref="LNetworkMessage{TData}"/>.</param>
    /// <param name="onServerReceived">[Opt.] The method to run when the server receives a message.</param>
    /// <param name="onClientReceived">[Opt.] The method to run when the client receives a message.</param>
    /// <param name="onClientReceivedFromClient">[Opt.] The method to run when the client receives a message from another client.</param>
    /// <returns>The <see cref="LNetworkMessage{TData}"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="LNetworkMessage{TData}"/> already exists.</exception>
    public static LNetworkMessage<TData> Create(
        string identifier,
        Action<TData, ulong>? onServerReceived = null,
        Action<TData>? onClientReceived = null,
        Action<TData, ulong>? onClientReceivedFromClient = null)
    {
        string actualIdentifier;

        try
        {
            actualIdentifier = $"{LNetworkUtils.GetModGuid(2)}.{identifier}";
        }
        catch (Exception e)
        {
            LethalNetworkAPIPlugin.Logger.LogError($"Unable to find Mod Guid! To still work, this Message will only use the given identifier. " +
                                                   $"Warning: This may cause collision with another mod's NetworkMessage! Stack Trace: {e}");
            actualIdentifier = identifier;
        }

        return new LNetworkMessage<TData>(actualIdentifier, onServerReceived, onClientReceived,
            onClientReceivedFromClient);
    }

    private LNetworkMessage(
        string identifier,
        Action<TData, ulong>? onServerReceived = null,
        Action<TData>? onClientReceived = null,
        Action<TData, ulong>? onClientReceivedFromClient = null)
    {
        if (UnnamedMessageHandler.LNetworkMessages.TryGetValue(identifier, out _))
        {
            throw new InvalidOperationException
            (
                $"A message with the identifier {identifier} already exists! " +
                "Please use a different identifier."
            );
        }

        this.Identifier = identifier;

        this.OnServerReceived += onServerReceived;
        this.OnClientReceived += onClientReceived;
        this.OnClientReceivedFromClient += onClientReceivedFromClient;

        UnnamedMessageHandler.LNetworkMessages.Add(identifier, this);
    }

    #endregion

    #region Public Varaibles and Events

    /// <summary>
    /// A callback that runs when the server receives a message.
    /// </summary>
    public event Action<TData, ulong>? OnServerReceived = delegate { };

    /// <summary>
    /// A callback that runs when the client receives a message.
    /// </summary>
    public event Action<TData>? OnClientReceived = delegate { };

    /// <summary>
    /// A callback that runs when the client receives a message from another client.
    /// </summary>
    public event Action<TData, ulong>? OnClientReceivedFromClient = delegate { };

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

    /// <summary>
    /// Clear all subscriptions to all callbacks on this <see cref="LNetworkMessage{TData}"/>.
    /// </summary>
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
