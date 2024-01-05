using System.Collections.Generic;
using Unity.Collections;

// ReSharper disable InvalidXmlDocComment

namespace LethalNetworkAPI;

/// <typeparam name="T">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class ClientMessage<T>
{
    #region Constructor
    
    /// <summary>
    /// Create a new network message for clients.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public ClientMessage(string identifier)
    {
        _messageIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.msg.{identifier}";
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
    /// <param name="includeLocalClient">Opt. (<see cref="bool"/>) If the local client event should be invoked.</param>
    /// <param name="waitForServerResponse">Opt. (<see cref="bool"/>) If the local client should wait for a server response before invoking the <see cref="OnReceivedFromClient"/> event.</param>
    /// <remarks><paramref name="waitForServerResponse"/> will only be considered if <paramref name="includeLocalClient"/> is set to true.</remarks>
    public void SendServer(T data, bool includeLocalClient = true, bool waitForServerResponse = false)
    {
        if (NetworkHandler.Instance == null) return;
        
        NetworkHandler.Instance.MessageServerRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)),
            toOtherClients: true, sendToOriginator: (includeLocalClient && waitForServerResponse));
        
        if(includeLocalClient && !waitForServerResponse)
            OnReceivedFromClient?.Invoke(NetworkManager.Singleton.LocalClientId);
        NetworkHandler.Instance.MessageServerRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)));

#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Server with data: {data}");
#endif
    }
    
    /// <summary>
    /// Send data to all clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="T"/>) The data to send.</param>
    public void SendOtherClients(T data, bool)
    {
        if (NetworkHandler.Instance != null)
            NetworkHandler.Instance.MessageOthersServerRpc(_messageIdentifier, Serializer.Serialize(new ValueWrapper<T>(data)));

#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Other Clients with data: {data}");
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