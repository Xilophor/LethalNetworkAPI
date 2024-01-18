using System.Collections.Generic;
using LethalNetworkAPI.Serializable;
using Unity.Collections;

namespace LethalNetworkAPI;

/// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class LethalServerMessage<TData> : NetworkMessage
{
    #region Constructor
    
    /// <summary>
    /// Create a new network message for the server.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalServerMessage(string identifier) : base(identifier)
    {
        NetworkHandler.OnServerMessage += ReceiveServerMessage;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkMessage with identifier \"{Identifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Event

    /// <summary>
    /// Send data to a specified client.
    /// </summary>
    /// <param name="data">(<typeparamref name="TData"/>) The data to send.</param>
    /// <param name="clientId">(<see cref="UInt64">ulong</see>) The client to send the data to.</param>
    public void SendClient(TData data, ulong clientId)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;

        NetworkHandler.Instance!.MessageClientRpc(Identifier, LethalNetworkSerializer.Serialize(data),
            clientRpcParams: GenerateClientParams(clientId));
    }
    
    /// <summary>
    /// Send data to the specified clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="TData"/>) The data to send.</param>
    /// <param name="clientIds">(<see cref="IEnumerable{UInt64}">IEnumerable&lt;ulong&gt;</see>) The clients to send the data to.</param>
    public void SendClients(TData data, IEnumerable<ulong> clientIds)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;

        NetworkHandler.Instance!.MessageClientRpc(Identifier, LethalNetworkSerializer.Serialize(data),
            clientRpcParams: GenerateClientParams(clientIds));
    }
    
    /// <summary>
    /// Send data to all clients.
    /// </summary>
    /// <param name="data">(<typeparamref name="TData"/>) The data to send.</param>
    /// <param name="receiveOnHost">Opt. (<see cref="bool"/>) Whether the host client should receive as well.</param>
    public void SendAllClients(TData data, bool receiveOnHost = true)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;
        
        if (receiveOnHost)
            NetworkHandler.Instance!.MessageClientRpc(Identifier, LethalNetworkSerializer.Serialize(data));
        else
            NetworkHandler.Instance!.MessageClientRpc(Identifier, LethalNetworkSerializer.Serialize(data),
                clientRpcParams: GenerateClientParamsExceptHost());
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to All Clients with data: {data}");
#endif
    }
    
    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when a message is received.
    /// </summary>
    /// <typeparam name="TData"> The received data.</typeparam>
    /// <typeparam name="ulong"> The origin client ID.</typeparam>
    public event Action<TData, ulong>? OnReceived;

    #endregion

    private void ReceiveServerMessage(string identifier, byte[] data, ulong originClientId)
    {
        if (identifier != Identifier) return;
        
        OnReceived?.Invoke(LethalNetworkSerializer.Deserialize<TData>(data), originClientId);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }
}