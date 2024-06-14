namespace LethalNetworkAPI.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;

#if NETSTANDARD2_1
using OdinSerializer;
#endif

internal class UnnamedMessageHandler : IDisposable
{
    internal static UnnamedMessageHandler? Instance { get; private set; }

    internal static Dictionary<string, INetMessage> LNetworkMessages { get; } = new();
    internal static Dictionary<string, LNetworkEvent> LNetworkEvents { get; } = new();
    internal static Dictionary<string, INetVariable> LNetworkVariables { get; } = new();

    private NetworkManager NetworkManager { get; }
    private CustomMessagingManager CustomMessagingManager { get; }

    private const string LibIdentifier = "LethalNetworkAPI";

    internal UnnamedMessageHandler()
    {
        Instance = this;

        this.NetworkManager = NetworkManager.Singleton;
        this.CustomMessagingManager = this.NetworkManager.CustomMessagingManager;

        this.CustomMessagingManager.OnUnnamedMessage += this.ReceiveMessage;
    }

    #region Messaging

    #region Send

    internal void SendMessageToClients(MessageData messageData, ulong[] clientGuidArray, bool deprecatedMessage = false)
    {
#if NETSTANDARD2_1
        WriteMessageData(out var writer, messageData, deprecatedMessage);

        if (clientGuidArray.Any(client => client == NetworkManager.ServerClientId))
        {
            clientGuidArray = clientGuidArray.Where(client => client != NetworkManager.ServerClientId).ToArray();

            var reader = new FastBufferReader(writer, Allocator.None);
            this.ReceiveMessage(NetworkManager.ServerClientId, reader);
        }

        if (!clientGuidArray.Any()) { writer.Dispose(); return; }

        this.CustomMessagingManager.SendUnnamedMessage(
            clientGuidArray,
            writer,
            NetworkDelivery.ReliableFragmentedSequenced
        );

        writer.Dispose();
#endif
    }

    internal void SendMessageToServer(MessageData messageData, bool deprecatedMessage = false)
    {
#if NETSTANDARD2_1
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
        reader.ReadValueSafe(out string identifier);

        if (identifier == $"{LibIdentifier}.Old")
            Old.Networking.NetworkHandler.Instance?.ReceiveMessage(clientId, reader);

        if (identifier != LibIdentifier)
        {
            reader.Dispose();
            return;
        }

        reader.ReadValueSafe(out byte[] serializedMessageData);
        reader.Dispose();

        var (messageID, messageType, messageData) = Deserialize<MessageData>(serializedMessageData);

        switch (messageType)
        {
            case EMessageType.Event | EMessageType.ServerMessage:
                LNetworkEvents[messageID].InvokeOnClientReceived();
                break;
            case EMessageType.Event | EMessageType.ClientMessage:
                LNetworkEvents[messageID].InvokeOnServerReceived(clientId);
                break;
            case EMessageType.Event | EMessageType.ClientMessageToClient:
                LNetworkEvents[messageID].InvokeOnClientReceivedFromClient(clientId);
                break;

            case EMessageType.Message | EMessageType.ServerMessage:
                LNetworkMessages[messageID].InvokeOnClientReceived(messageData);
                break;
            case EMessageType.Message | EMessageType.ClientMessage:
                LNetworkMessages[messageID].InvokeOnServerReceived(messageData, clientId);
                break;
            case EMessageType.Message | EMessageType.ClientMessageToClient:
                LNetworkMessages[messageID].InvokeOnClientReceivedFromClient(messageData, clientId);
                break;

            case EMessageType.Variable:
                throw new NotImplementedException();

            case EMessageType.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
#endif
    }

    #endregion

    #endregion

#if NETSTANDARD2_1

    #region Helper Methods

    private static readonly SerializationContext DefaultSerializationContext = new()
        {
            Config = new SerializationConfig()
            {
                SerializationPolicy = SerializationPolicies.Everything
            }
        };

    private static readonly DeserializationContext DefaultDeserializationContext = new()
    {
        Config = new SerializationConfig()
        {
            SerializationPolicy = SerializationPolicies.Everything
        }
    };

    internal static byte[] Serialize(object? data) =>
        SerializationUtility.SerializeValue(data, DataFormat.Binary, DefaultSerializationContext);

    internal static T Deserialize<T>(byte[] serializedData) =>
        SerializationUtility.DeserializeValue<T>(serializedData, DataFormat.Binary, DefaultDeserializationContext);

    private static void WriteMessageData(out FastBufferWriter writer, MessageData messageData, bool deprecatedMessage = false)
    {
        var (serializedMessage, size) = SerializeDataAndGetSize(messageData);

        writer = new FastBufferWriter(size, Allocator.Temp, size + 8);

        if (!writer.TryBeginWrite(size)) throw new OutOfMemoryException(
            $"The buffer is too small ({writer.MaxCapacity}) " +
            $"to write the message data ({size}) @ {writer.Position}. Shit's hella fucked!");

        writer.WriteValue(deprecatedMessage ? $"{LibIdentifier}.Old" : LibIdentifier);
        writer.WriteValue(serializedMessage);
    }

    private static (byte[], int) SerializeDataAndGetSize(MessageData messageData)
    {
        var size = 0;
        var serializedData = Serialize(messageData);

        size += FastBufferWriter.GetWriteSize(LibIdentifier);
        size += FastBufferWriter.GetWriteSize(serializedData);

        if (size > 65536)
        {
            LethalNetworkAPIPlugin.Logger.LogWarning(
                $"The serialized message size of '{messageData.Identifier}' is {size} bytes. " +
                $"This is larger than the recommended max size of 65536 bytes. " +
                $"The message may be dropped during transit.");
        }

        return (serializedData, size);
    }

    #endregion

#endif

    public void Dispose() => this.CustomMessagingManager.OnUnnamedMessage -= this.ReceiveMessage;
}
