using LethalNetworkAPI.Serializable;

namespace LethalNetworkAPI;

/// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class LethalClientMessage<TData>
{
    #region Constructor
    
    /// <summary>
    /// Create a new network message for clients.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalClientMessage(string identifier)
    {
        _messageIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.msg.{identifier}";
        NetworkHandler.OnClientMessage += ReceiveMessage;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkMessage with identifier \"{_messageIdentifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Event
    
    /// <summary>
    /// Invoke event to the server/host.
    /// </summary>
    public void SendServer(TData data)
    {
        if (NetworkHandler.Instance == null)
        {
            Plugin.Logger.LogError(string.Format(
                TextDefinitions.NotInLobbyMessage, _messageIdentifier, data));
            return;
        }
        
        NetworkHandler.Instance.MessageServerRpc(_messageIdentifier, LethalNetworkSerializer.Serialize(data));
    }
    
    /// <summary>
    /// Send data to the server/host.
    /// </summary>
    /// <param name="data">(<typeparamref name="TData"/>) The data to send.</param>
    /// <param name="includeLocalClient">Opt. (<see cref="bool"/>) If the local client event should be invoked.</param>
    /// <param name="waitForServerResponse">Opt. (<see cref="bool"/>) If the local client should wait for a server response before invoking the <see cref="OnReceivedFromClient"/> event.</param>
    /// <remarks><paramref name="waitForServerResponse"/> will only be considered if <paramref name="includeLocalClient"/> is set to true.</remarks>
    public void SendAllClients(TData data, bool includeLocalClient = true, bool waitForServerResponse = false)
    {
        if (NetworkHandler.Instance == null)
        {
            Plugin.Logger.LogError(string.Format(
                TextDefinitions.NotInLobbyMessage, _messageIdentifier, data));
            return;
        }
        
        NetworkHandler.Instance.MessageServerRpc(_messageIdentifier, LethalNetworkSerializer.Serialize(data),
            toOtherClients: true, sendToOriginator: (includeLocalClient && waitForServerResponse));
        
        if(includeLocalClient && !waitForServerResponse)
            OnReceivedFromClient?.Invoke(data, NetworkManager.Singleton.LocalClientId);
        NetworkHandler.Instance.MessageServerRpc(_messageIdentifier, LethalNetworkSerializer.Serialize(data));

#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Server with data: {data}");
#endif
    }
    
    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when a message is received from the server.
    /// </summary>
    /// <typeparam name="TData">The received data.</typeparam>
    public event Action<TData>? OnReceived;

    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when a message is received from another client.
    /// </summary>
    /// <typeparam name="TData">The received data.</typeparam>
    /// <typeparam name="ulong">The origin client ID.</typeparam>
    public event Action<TData, ulong>? OnReceivedFromClient;

    #endregion
    
    private void ReceiveMessage(string identifier, byte[] data, ulong originatorClient)
    {
        if (identifier != _messageIdentifier) return;

        if (originatorClient == 99999)
            OnReceived?.Invoke(LethalNetworkSerializer.Deserialize<TData>(data));
        else
            OnReceivedFromClient?.Invoke(LethalNetworkSerializer.Deserialize<TData>(data), originatorClient);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }

    private readonly string _messageIdentifier;
}