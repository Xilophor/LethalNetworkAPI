using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable MemberCanBeMadeStatic.Local

namespace LethalNetworkAPI.Old.Networking;

using Internal;
using Utils;

internal class NetworkHandler : IDisposable
{
    private void SendMessageToServer(MessageData messageData) => UnnamedMessageHandler.Instance!.SendMessageToServer(messageData, true);
    private void SendMessageToClients(MessageData messageData, ulong[] clientGuidArray) => UnnamedMessageHandler.Instance!.SendMessageToClients(messageData, clientGuidArray, true);

    public NetworkHandler()
    {
        Instance = this;

        NetworkSpawn?.Invoke();
        NetworkManager.Singleton.NetworkTickSystem.Tick += NetworkTick;
        NetworkManager.Singleton.OnClientConnectedCallback += this.OnClientConnectedCallback;
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            "Created new Network Handler Instance.");
#endif
    }

    public void Dispose()
    {
        NetworkDespawn?.Invoke();
        OnPlayerJoin = delegate { };
        NetworkDespawn = delegate { };
        Instance = null;

        NetworkManager.Singleton.NetworkTickSystem.Tick -= NetworkTick;
        NetworkManager.Singleton.OnClientConnectedCallback -= this.OnClientConnectedCallback;
    }

    private void OnClientConnectedCallback(ulong client) =>
        OnPlayerJoin?.Invoke(client);

    public void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
#if NETSTANDARD2_1
        reader.ReadValueSafe(out byte[] serializedMessageData);
        reader.Dispose();

        var (messageID, messageType, originalMessageData) = UnnamedMessageHandler.Deserialize<MessageData>(serializedMessageData);

        var messageData = (byte[])originalMessageData!;

        switch (messageType)
        {
            case EMessageType.Event | EMessageType.ServerMessage:
                OnClientEvent?.Invoke(messageID, 99999);
                break;
            case EMessageType.Event | EMessageType.ClientMessage:
                OnServerEvent?.Invoke(messageID, clientId);
                break;
            case EMessageType.Event | EMessageType.ClientMessageToClient:
                OnClientEvent?.Invoke(messageID, clientId);
                break;

#pragma warning disable CS0612 // Type or member is obsolete
            case EMessageType.SyncedEvent:
                OnSyncedServerEvent?.Invoke(messageID, UnnamedMessageHandler.Deserialize<double>(messageData), clientId);
                break;
#pragma warning restore CS0612 // Type or member is obsolete

            case EMessageType.Message | EMessageType.ServerMessage:
                OnClientMessage?.Invoke(messageID, messageData, 99999);
                break;
            case EMessageType.Message | EMessageType.ClientMessage:
                OnServerMessage?.Invoke(messageID, messageData, clientId);
                break;
            case EMessageType.Message | EMessageType.ClientMessageToClient:
                OnClientMessage?.Invoke(messageID, messageData, clientId);
                break;

            case EMessageType.Variable | EMessageType.Request:
                GetVariableValue?.Invoke(messageID, clientId);
                break;
            case EMessageType.Variable:
                OnVariableUpdate?.Invoke(messageID, messageData);
                break;

            case EMessageType.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
#endif
    }

    #region Messages

    internal void MessageServerRpc(string identifier,
        byte[] data,
        bool toOtherClients = false,
        bool sendToOriginator = false
        )
    {
        if (!toOtherClients)
        {
            this.SendMessageToServer(
                new MessageData(
                    identifier,
                    EMessageType.Message | EMessageType.ClientMessage,
                    data)
            );
        }
        else
        {
            this.SendMessageToClients(
                new MessageData(
                    identifier,
                    EMessageType.Message | EMessageType.ClientMessageToClient,
                    data),
                sendToOriginator ? LNetworkUtils.AllConnectedClients : LNetworkUtils.OtherConnectedClients
            );
        }
    }

    internal void MessageClientRpc(string identifier,
        byte[] data,
        ulong[] clientGuidArray)
    {
        this.SendMessageToClients(
                new MessageData(
                    identifier,
                    EMessageType.Message | EMessageType.ServerMessage,
                    data),
                clientGuidArray
            );
    }

    #endregion Messages

    #region Events

    internal void EventServerRpc(string identifier,
        bool toOtherClients = false,
        bool sendToOriginator = false
    )
    {
        if (!toOtherClients)
        {
            this.SendMessageToServer(
                new MessageData(
                    identifier,
                    EMessageType.Event | EMessageType.ClientMessage)
            );
        }
        else
        {
            this.SendMessageToClients(
                new MessageData(
                    identifier,
                    EMessageType.Event | EMessageType.ClientMessageToClient),
                sendToOriginator ? LNetworkUtils.AllConnectedClients : LNetworkUtils.OtherConnectedClients
            );
        }
    }

    internal void EventClientRpc(string identifier,
        ulong[] clientGuidArray)
    {
        this.SendMessageToClients(
            new MessageData(
                identifier,
                EMessageType.Event | EMessageType.ServerMessage),
            clientGuidArray
        );
    }

    internal void SyncedEventServerRpc(string identifier,
        double time)
    {
#if NETSTANDARD2_1
        this.SendMessageToClients(
            new MessageData(
                identifier,
                EMessageType.SyncedEvent,
                UnnamedMessageHandler.Serialize(time)),
            LNetworkUtils.OtherConnectedClients
        );
#endif
    }

    #endregion Events

    #region Variables

    [ServerRpc(RequireOwnership = false)]
    internal void UpdateVariableServerRpc(string identifier,
        byte[] data)
    {
        this.SendMessageToClients(
            new MessageData(
                identifier,
                EMessageType.Variable,
                data),
            LNetworkUtils.OtherConnectedClients
        );
    }

    [ClientRpc]
    internal void UpdateVariableClientRpc(string identifier,
        byte[] data,
        ulong[] clientGuidArray)
    {
        this.SendMessageToClients(
            new MessageData(
                identifier,
                EMessageType.Variable,
                data),
            clientGuidArray
        );
    }

    [ServerRpc(RequireOwnership = false)]
    internal void GetVariableValueServerRpc(string identifier,
        ServerRpcParams serverRpcParams = default)
    {
        this.SendMessageToServer(
            new MessageData(
                identifier,
                EMessageType.Variable | EMessageType.Request)
        );
    }

    #endregion

    internal static NetworkHandler? Instance { get; private set; }

    internal readonly List<ILethalNetVar> ObjectNetworkVariableList = [];

    #region Internal Events

    internal static event Action? NetworkSpawn;
    internal static event Action? NetworkDespawn;
    internal static event Action? NetworkTick;
    internal static event Action<ulong>? OnPlayerJoin;

    internal static event Action<string, byte[], ulong>? OnServerMessage; // identifier, data, originatorClientId
    internal static event Action<string, byte[], ulong>? OnClientMessage; // identifier, data, originatorClientId

    internal static event Action<string, byte[]>? OnVariableUpdate; // identifier, data
    internal static event Action<string, ulong>? GetVariableValue; // identifier, connectedClientId

    internal static event Action<string, ulong>? OnServerEvent; // identifier, originatorClientId
    internal static event Action<string, ulong>? OnClientEvent; // identifier, originatorClientId
    internal static event Action<string, double, ulong>? OnSyncedServerEvent; // identifier, time, originatorClientId
    internal static event Action<string, double, ulong>? OnSyncedClientEvent; // identifier, time, originatorClientId

    #endregion
}
