using System.Collections.Generic;
using Unity.Collections;

// ReSharper disable InvalidXmlDocComment

namespace LethalNetworkAPI;

/// <typeparam name="T">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class LethalServerMessage<T>
{
    #region Constructor
    
    /// <summary>
    /// Create a new network message for the server.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalServerMessage(string identifier)
    {
        _messageIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.msg.{identifier}";
        NetworkHandler.OnServerMessage += ReceiveServerMessage;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkMessage with identifier \"{_messageIdentifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Event

    /// <summary>
    /// Send data to a specified client.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    /// <param name="clientId">(<see cref="UInt64">ulong</see>) The client to send the data to.</param>
    public void SendClient(T data, ulong clientId)
    {
        if (NetworkHandler.Instance == null)
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NotInLobbyMessage, _messageIdentifier, data));
            return;
        }
        
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NotServerInfo, NetworkManager.Singleton.LocalClientId, _messageIdentifier));
            return;
        }
        
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return;
        

        NetworkHandler.Instance.MessageClientRpc(_messageIdentifier, Serializer.Serialize(data),
            clientRpcParams: new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = new NativeArray<ulong>(new [] {clientId}, Allocator.Persistent) } } );
    }
    
    /// <summary>
    /// Send data to the specified clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    /// <param name="clientIds">(<see cref="IEnumerable{UInt64}">IEnumerable&lt;ulong&gt;</see>) The clients to send the data to.</param>
    public void SendClients(T data, IEnumerable<ulong> clientIds)
    {
        if (NetworkHandler.Instance == null)
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NotInLobbyMessage, _messageIdentifier, data));
            return;
        }
        
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NotServerInfo, NetworkManager.Singleton.LocalClientId, _messageIdentifier));
            return;
        }

        var allowedClientIds = new NativeArray<ulong>(clientIds
            .Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray(), Allocator.Persistent);
        
        if (!allowedClientIds.Any()) return;
        
        NetworkHandler.Instance.MessageClientRpc(_messageIdentifier, Serializer.Serialize(data), 
            clientRpcParams: new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = allowedClientIds } } );
    }
    
    /// <summary>
    /// Send data to all clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    public void SendAllClients(T data)
    {
        if (NetworkHandler.Instance == null)
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NotInLobbyMessage, _messageIdentifier, data));
            return;
        }
        
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NotServerInfo, NetworkManager.Singleton.LocalClientId, _messageIdentifier));
            return;
        }
        
        NetworkHandler.Instance.MessageClientRpc(_messageIdentifier, Serializer.Serialize(data));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to All Clients {receiveOnHost} with data: {data}; {Serializer.Serialize(data)}");
#endif
    }
    
    /// <summary>
    /// The callback to invoke when a message is received.
    /// </summary>
    /// <typeparam name="data">(<typeparamref name="T"/>) The received data.</typeparam>
    /// <typeparam name="clientId">(<see cref="UInt64">ulong</see>) The origin client.</typeparam>
    public event Action<T, ulong>? OnReceived;

    #endregion

    private void ReceiveServerMessage(string identifier, string data, ulong originClientId)
    {
        if (identifier != _messageIdentifier) return;
        
        OnReceived?.Invoke(Serializer.Deserialize<T>(data), originClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }

    private readonly string _messageIdentifier;
}