using LethalNetworkAPI.Old.Networking;
using LethalNetworkAPI.Serializable;
using Unity.Netcode;
using System;

namespace LethalNetworkAPI;

/// <typeparam name="TData">The serializable data type of the message.</typeparam>
public sealed class LethalClientMessage<TData> : LNetworkMessageDepricated
{
    #region Constructor
    
    /// <summary>
    /// Create a new network message for clients.
    /// </summary>
    /// 
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// 
    /// <param name="onReceived">Opt. (<see cref="Action{T}">Action&lt;TData&gt;</see>)
    /// The method to run when a message is received from the server.</param>
    /// 
    /// <param name="onReceivedFromClient">Opt. (<see cref="Action{T}">Action&lt;TData, ulong&gt;</see>)
    /// The method to run when a message is received from another client.</param>
    /// 
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalClientMessage(string identifier,
        Action<TData>? onReceived = null,
        Action<TData, ulong>? onReceivedFromClient = null)
        : base(identifier)
    {
        NetworkHandler.OnClientMessage += ReceiveMessage;
        
        OnReceived += onReceived;
        OnReceivedFromClient += onReceivedFromClient;
    }
    
    #endregion

    #region Public Methods and Event
    
    /// <summary>
    /// Invoke event to the server/host.
    /// </summary>
    public void SendServer(TData data)
    {
        if (IsNetworkHandlerNull()) return;
        
        NetworkHandler.Instance!.MessageServerRpc(Identifier, LethalNetworkSerializer.Serialize(data));
    }
    
    /// <summary>
    /// Send data to the server/host.
    /// </summary>
    /// 
    /// <param name="data">(<typeparamref name="TData"/>) The data to send.</param>
    /// 
    /// <param name="includeLocalClient">Opt. (<see cref="bool"/>)
    /// If the local client event should be invoked.</param>
    /// 
    /// <param name="waitForServerResponse">Opt. (<see cref="bool"/>)
    /// If the local client should wait for a server response before
    /// invoking the <see cref="OnReceivedFromClient"/> event.</param>
    /// 
    /// <remarks><paramref name="waitForServerResponse"/> will only be considered
    /// if <paramref name="includeLocalClient"/> is set to true.</remarks>
    public void SendAllClients(TData data, bool includeLocalClient = true, bool waitForServerResponse = false)
    {
        if (IsNetworkHandlerNull()) return;
        
        NetworkHandler.Instance!.MessageServerRpc(Identifier, LethalNetworkSerializer.Serialize(data),
            toOtherClients: true, sendToOriginator: includeLocalClient && waitForServerResponse);
        
        if(includeLocalClient && !waitForServerResponse)
            OnReceivedFromClient?.Invoke(data, NetworkManager.Singleton.LocalClientId);

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Attempted to Send Message to Server with data: {data}");
#endif
    }

    public void ClearSubscriptions()
    {
        OnReceived = delegate { };
        OnReceivedFromClient = delegate { };
    }

    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when a message is received from the server.
    /// </summary>
    /// 
    /// <typeparam name="TData">The received data.</typeparam>
    public event Action<TData>? OnReceived;

    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when a message is received from another client.
    /// </summary>
    /// 
    /// <typeparam name="TData">The received data.</typeparam>
    /// 
    /// <typeparam name="ulong">The origin client ID.</typeparam>
    public event Action<TData, ulong>? OnReceivedFromClient;

    #endregion
    
    private void ReceiveMessage(string identifier, byte[] data, ulong originatorClient)
    {
        if (identifier != Identifier) return;

        if (originatorClient == 99999)
            OnReceived?.Invoke(LethalNetworkSerializer.Deserialize<TData>(data));
        else
            OnReceivedFromClient?.Invoke(LethalNetworkSerializer.Deserialize<TData>(data), originatorClient);
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug(
            $"Received data: {data}");
#endif
    }
}