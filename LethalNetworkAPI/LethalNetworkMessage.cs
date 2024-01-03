using System.Collections.Generic;
using Unity.Collections;

// ReSharper disable InvalidXmlDocComment

namespace LethalNetworkAPI;

/// <typeparam name="T">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class LethalNetworkMessage<T>
{
    #region Constructor
    
    /// <summary>
    /// Create a new network message.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalNetworkMessage(string identifier)
    {
        _messageIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.msg.{identifier}";
        NetworkHandler.OnServerMessage += ReceiveServerMessage;
        NetworkHandler.OnClientMessage += ReceiveClientMessage;
        NetworkHandler.OnClientMessageFrom += ReceiveFromClientMessage;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkMessage with identifier \"{_messageIdentifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Event
    
    /// <summary>
    /// Send data to the server/host.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    public void SendServer(T data)
    {
        if (NetworkHandler.Instance != null)
            NetworkHandler.Instance.MessageServerRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)));

#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Server with data: {data}");
#endif
    }
    
    /// <summary>
    /// Send data to other clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    public void SendOtherClients(T data)
    {
        if (NetworkHandler.Instance != null)
            NetworkHandler.Instance.MessageOthersServerRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)));

#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Other Clients with data: {data}");
#endif
    }

    /// <summary>
    /// Send data to a specified client.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    /// <param name="clientId">(<see cref="UInt64">ulong</see>) The client to send the data to.</param>
    public void SendClient(T data, ulong clientId)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) || NetworkHandler.Instance == null) return;
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return;

        NetworkHandler.Instance.MessageClientRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)),
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = new NativeArray<ulong>(new [] {clientId}, Allocator.Persistent) } } );
    }
    
    /// <summary>
    /// Send data to the specified clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    /// <param name="clientIds">(<see cref="IEnumerable{UInt64}">IEnumerable&lt;ulong&gt;</see>) The clients to send the data to.</param>
    public void SendClients(T data, IEnumerable<ulong> clientIds)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) || NetworkHandler.Instance == null) return;

        var allowedClientIds = new NativeArray<ulong>(clientIds
            .Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray(), Allocator.Persistent);
        
        if (!allowedClientIds.Any()) return;
        
        NetworkHandler.Instance.MessageClientRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)), 
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = allowedClientIds } } );
    }
    
    /// <summary>
    /// Send data to all clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    /// <param name="receiveOnHost">(<see cref="bool"/>) Whether the host client should receive as well.</param>
    public void SendAllClients(T data, bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) || NetworkHandler.Instance == null) return;
        
        if (receiveOnHost)
            NetworkHandler.Instance.MessageClientRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)));
        else
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);

            if (!clientIds.Any()) return;
            
            NetworkHandler.Instance.MessageClientRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)), 
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } } );
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to All Clients {receiveOnHost} with data: {data}; {Serializer.Serialize(new ValueWrapper<T>(data))}");
#endif
    }
    
    /// <summary>
    /// The callback to invoke when a message is received by the server.
    /// </summary>
    public event Action<T>? OnServerReceived;
    
    /// <summary>
    /// The callback to invoke when a message is received by the server.
    /// </summary>
    /// <typeparam name="clientId">(<see cref="UInt64">ulong</see>) The origin client.</typeparam>
    public event Action<T, ulong>? OnServerReceivedFrom;
    
    /// <summary>
    /// The callback to invoke when a message is received by the client.
    /// </summary>
    public event Action<T>? OnClientReceived;

    /// <summary>
    /// The callback to invoke when a message is received by the client from another client;
    /// </summary>
    public event Action<T, ulong>? OnClientReceivedFrom;

    #endregion

    private void ReceiveServerMessage(string identifier, string data, ulong originClientId)
    {
        if (identifier != _messageIdentifier) return;

        OnServerReceived?.Invoke(Serializer.Deserialize<ValueWrapper<T>>(data)!.var!);
        OnServerReceivedFrom?.Invoke(Serializer.Deserialize<ValueWrapper<T>>(data)!.var!, originClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }
    
    private void ReceiveClientMessage(string identifier, string data)
    {
        if (identifier != _messageIdentifier) return;

        OnClientReceived?.Invoke(Serializer.Deserialize<ValueWrapper<T>>(data)!.var!);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }
    
    private void ReceiveFromClientMessage(string identifier, string data, ulong originatorClient)
    {
        if (identifier != _messageIdentifier) return;

        OnClientReceivedFrom?.Invoke(Serializer.Deserialize<ValueWrapper<T>>(data)!.var!, originatorClient);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }

    private readonly string _messageIdentifier;
}