namespace LethalNetworkAPI.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Utils;
using HarmonyLib;

#if NETSTANDARD2_1
using OdinSerializer;
using Old.Networking;
#endif

internal class UnnamedMessageHandler : IDisposable
{
    internal static UnnamedMessageHandler? Instance { get; private set; }

    internal static Dictionary<string, INetMessage> LNetworkMessages { get; } = new();
    internal static Dictionary<string, LNetworkEvent> LNetworkEvents { get; } = new();
    internal static Dictionary<string, LNetworkVariableBase> LNetworkVariables { get; } = new();

    internal bool IsServer => this.NetworkManager.IsServer || this.NetworkManager.IsHost;
    private NetworkManager NetworkManager { get; }
    private CustomMessagingManager CustomMessagingManager { get; }

    private const string LibIdentifier = "LethalNetworkAPI";

    internal HashSet<LNetworkVariableBase> DirtyBois { get; } = [];
    internal Dictionary<string, object?> UnInitializedValues { get; } = new();
    internal static event Action? VariableCheck;

    internal UnnamedMessageHandler()
    {
        Instance = this;

        this.NetworkManager = NetworkManager.Singleton;
        this.CustomMessagingManager = this.NetworkManager.CustomMessagingManager;

        this.NetworkManager.NetworkTickSystem.Tick += this.CheckVariablesForChanges;
        this.CustomMessagingManager.OnUnnamedMessage += this.ReceiveMessage;

        if (this.NetworkManager.IsServer || this.NetworkManager.IsHost)
        {
            this.NetworkManager.OnClientConnectedCallback += this.UpdateNewClientVariables;
            this.NetworkManager.OnClientDisconnectCallback += this.UpdateClientList;
        }
    }

    #region Messaging

    #region Variables

    private int _ticksSinceLastCheck;
    private void CheckVariablesForChanges()
    {
        if (this._ticksSinceLastCheck++ > 9)
        {
            VariableCheck?.Invoke();
            this._ticksSinceLastCheck = 0;
        }

        this.UpdateVariables();
    }

    private void UpdateVariables()
    {
        if (this.DirtyBois.Count == 0) return;

        foreach (var variable in this.DirtyBois)
        {
            if (this.IsServer)
                this.SendMessageToClients(
                    new MessageData(
                        variable.Identifier,
                        EMessageType.Variable | EMessageType.DataUpdate,
                        variable.GetValue()),
                    LNetworkUtils.OtherConnectedClients);
            else
                this.SendMessageToServer(
                    new MessageData(
                        variable.Identifier,
                        EMessageType.Variable | EMessageType.DataUpdate,
                        variable.GetValue()));

            variable.ResetDirty();
        }

        this.DirtyBois.Clear();
    }

    private void UpdateNewClientVariables(ulong newClient)
    {
        this.UpdateClientList(newClient);

        foreach (var variable in LNetworkVariables.Values)
        {
            this.SendMessageToClients(
                new MessageData(
                    variable.Identifier,
                    EMessageType.Variable | EMessageType.DataUpdate,
                    variable.OwnerClients),
                [newClient]);
        }
    }

    #endregion

    #region Send

    private void UpdateClientList(ulong changedClient) =>
        this.SendMessageToClients(
            new MessageData(
                "Internal.UpdateClientList",
                EMessageType.UpdateClientList,
                LNetworkUtils.OtherConnectedClients),
            LNetworkUtils.AllConnectedClients);

    internal void SendMessageToClients(MessageData messageData, ulong[] clientGuidArray, bool deprecatedMessage = false)
    {
#if NETSTANDARD2_1
        if (clientGuidArray.Any(client => client == this.NetworkManager.LocalClientId))
        {
            clientGuidArray = clientGuidArray.Where(client => client != this.NetworkManager.LocalClientId).ToArray();

            if (deprecatedMessage)
                NetworkHandler.Instance!.HandleMessage(
                    this.NetworkManager.LocalClientId,
                    messageData.Identifier,
                    messageData.MessageType,
                    (byte[]?)messageData.Data ?? []);
            else
                this.HandleMessage(
                    this.NetworkManager.LocalClientId,
                    messageData.Identifier,
                    messageData.MessageType,
                    messageData.Data,
                    [this.NetworkManager.LocalClientId]);
        }

        if (!clientGuidArray.Any()) return;

        WriteMessageData(out var writer, messageData, deprecatedMessage);

        this.CustomMessagingManager.SendUnnamedMessage(
            clientGuidArray,
            writer,
            NetworkDelivery.ReliableFragmentedSequenced
        );

        writer.Dispose();
#endif
    }

    /// <remarks>This method will also not send to the server.</remarks>
    internal void SendMessageToClientsExcept(MessageData messageData, ulong clientId, bool deprecatedMessage = false)
    {
#if NETSTANDARD2_1
        WriteMessageData(out var writer, messageData, deprecatedMessage);

        var clientIds = LNetworkUtils.AllConnectedClientsExcept(clientId, NetworkManager.ServerClientId);
        if (clientIds.Length == 0) return;

        this.CustomMessagingManager.SendUnnamedMessage(
            LNetworkUtils.AllConnectedClientsExcept(clientId, NetworkManager.ServerClientId),
            writer,
            NetworkDelivery.ReliableFragmentedSequenced
        );

        writer.Dispose();
#endif
    }

    internal void SendMessageToServer(MessageData messageData, bool deprecatedMessage = false)
    {
#if NETSTANDARD2_1
        if (this.IsServer)
        {
            if (deprecatedMessage)
                NetworkHandler.Instance!.HandleMessage(
                    NetworkManager.ServerClientId,
                    messageData.Identifier,
                    messageData.MessageType,
                    (byte[]?)messageData.Data ?? []);
            else
                this.HandleMessage(
                    NetworkManager.ServerClientId,
                    messageData.Identifier,
                    messageData.MessageType,
                    messageData.Data,
                    [NetworkManager.ServerClientId]);

            return;
        }

        WriteMessageData(out var writer, messageData, deprecatedMessage);

        this.CustomMessagingManager.SendUnnamedMessage(
            NetworkManager.ServerClientId,
            writer,
            NetworkDelivery.ReliableFragmentedSequenced
        );

        writer.Dispose();
#endif
    }

    #endregion

    #region Receive

    private void ReceiveMessage(ulong clientId, FastBufferReader reader)
    {
#if NETSTANDARD2_1
        string identifier;
        try
        {
            reader.ReadValueSafe(out identifier);
        }
        catch
        {
            return;
        }

        if (identifier == $"{LibIdentifier}.Old") NetworkHandler.Instance?.ReadMessage(clientId, reader);

        if (identifier != LibIdentifier) return;

        reader.ReadValueSafe(out string messageID);
        reader.ReadValueSafe(out EMessageType messageType);
        reader.ReadValueSafe(out ulong[] targetClients);

        reader.ReadValueSafe(out byte[] serializedMessageData);
        reader.ReadValueSafe(out byte[] serializedType);

        var messageDataType = Deserialize<Type?>(serializedType);
        var messageData = messageDataType != null ? Deserialize<object>(serializedMessageData) : null;

        #if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received message: ({messageType}) {messageID} of type {messageDataType} with data {messageData} from {clientId}.");
        #endif

        this.HandleMessage(clientId, messageID, messageType, messageData, targetClients);
#endif
    }

    private void HandleMessage(ulong clientId, string messageID, EMessageType messageType, object? messageData, ulong[] targetClients)
    {
        LNetworkVariableBase variable;

        var nonServerTargets = targetClients.Where(i => i != NetworkManager.ServerClientId && i != clientId).ToArray();

        switch (messageType)
        {
            case EMessageType.Event | EMessageType.ServerMessage:
                LNetworkEvents[messageID].InvokeOnClientReceived();
                break;
            case EMessageType.Event | EMessageType.ClientMessage:
                LNetworkEvents[messageID].InvokeOnServerReceived(clientId);
                break;
            case EMessageType.Event | EMessageType.ClientMessageToClient:
                if (!this.IsServer)
                {
                    LNetworkMessages[messageID].InvokeOnClientReceivedFromClient(messageData, clientId);
                    break;
                }

                this.SendMessageToClients(new MessageData(messageID, messageType, messageData), nonServerTargets);
                if (targetClients.Any(i => i == NetworkManager.ServerClientId))
                    LNetworkEvents[messageID].InvokeOnClientReceivedFromClient(clientId);
                break;

            case EMessageType.Message | EMessageType.ServerMessage:
                LNetworkMessages[messageID].InvokeOnClientReceived(messageData);
                break;
            case EMessageType.Message | EMessageType.ClientMessage:
                LNetworkMessages[messageID].InvokeOnServerReceived(messageData, clientId);
                break;
            case EMessageType.Message | EMessageType.ClientMessageToClient:
                if (!this.IsServer)
                {
                    LNetworkMessages[messageID].InvokeOnClientReceivedFromClient(messageData, clientId);
                    break;
                }

                this.SendMessageToClients(new MessageData(messageID, messageType, messageData), nonServerTargets);
                if (targetClients.Any(i => i == NetworkManager.ServerClientId))
                    LNetworkMessages[messageID].InvokeOnClientReceivedFromClient(messageData, clientId);
                break;

            case EMessageType.Variable | EMessageType.DataUpdate:
                if (!LNetworkVariables.TryGetValue(messageID, out variable))
                {
                    this.UnInitializedValues[messageID] = messageData;
                    break;
                }

                if (this.IsServer)
                {
                    if (!variable.CanWrite()) break;

                    this.SendMessageToClients(new MessageData(messageID, messageType, messageData), nonServerTargets);
                }

                variable.ReceiveUpdate(messageData);
                break;
            case EMessageType.Variable | EMessageType.OwnershipUpdate:
                variable = LNetworkVariables[messageID];

                if (clientId != NetworkManager.ServerClientId ||
                    variable.WritePerms != LNetworkVariableWritePerms.Owner) break;

                variable.OwnerClients = (ulong[]?)messageData;
                break;

            case EMessageType.UpdateClientList:
                if (clientId != NetworkManager.ServerClientId ||
                    this.NetworkManager.LocalClientId == NetworkManager.ServerClientId) break;

                LNetworkUtils.AllConnectedClients = ((ulong[]?)messageData ?? [this.NetworkManager.LocalClientId]).AddToArray<ulong>(0);
                break;

            case EMessageType.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    #endregion

    #endregion

#if NETSTANDARD2_1

    #region Helper Methods

    internal static byte[] Serialize(object? data) =>
        SerializationUtility.SerializeValue(data, DataFormat.Binary);

    internal static T Deserialize<T>(byte[] serializedData) =>
        SerializationUtility.DeserializeValue<T>(serializedData, DataFormat.Binary);

    private static void WriteMessageData(out FastBufferWriter writer, MessageData messageData, bool deprecatedMessage)
    {
        var (serializedData, serializedType, size) = SerializeDataAndGetSize(messageData, deprecatedMessage);

        writer = new FastBufferWriter(size, Allocator.Temp, size + 8);

        if (!writer.TryBeginWrite(size)) throw new OutOfMemoryException(
            $"The buffer is too small ({writer.MaxCapacity}) " +
            $"to write the message data ({size}) @ {writer.Position}. Shit's hella fucked!");

        writer.WriteValue(deprecatedMessage ? $"{LibIdentifier}.Old" : LibIdentifier);

        writer.WriteValue(messageData.Identifier);
        writer.WriteValue(messageData.MessageType);
        if (!deprecatedMessage) writer.WriteValue(messageData.TargetClients ?? []);

        writer.WriteValue(serializedData);
        writer.WriteValue(serializedType);
    }

    private static (byte[], byte[], int) SerializeDataAndGetSize(MessageData messageData, bool deprecatedMessage)
    {
        var serializedData = messageData.Data?.GetType() == typeof(byte[]) ?
            (byte[])messageData.Data : Serialize(messageData.Data);
        var serializedType = Serialize(messageData.Data?.GetType());

        var size = FastBufferWriter.GetWriteSize(deprecatedMessage ? $"{LibIdentifier}.Old" : LibIdentifier);

        size += FastBufferWriter.GetWriteSize(messageData.Identifier);
        size += FastBufferWriter.GetWriteSize(messageData.MessageType);
        if (!deprecatedMessage) size += FastBufferWriter.GetWriteSize(messageData.TargetClients ?? []);

        size += FastBufferWriter.GetWriteSize(serializedData);
        size += FastBufferWriter.GetWriteSize(serializedType);

        if (size > 65536)
        {
            LethalNetworkAPIPlugin.Logger.LogWarning(
                $"The serialized message size of '{messageData.Identifier}' is {size} bytes. " +
                $"This is larger than the recommended max size of 65536 bytes. " +
                $"The message may be dropped during transit.");
        }

        return (serializedData, serializedType, size);
    }

    #endregion

#endif

    public void Dispose()
    {
        this.CustomMessagingManager.OnUnnamedMessage -= this.ReceiveMessage;

        foreach (var variable in LNetworkVariables.Values)
        {
            variable.ResetValue();
        }
    }
}
