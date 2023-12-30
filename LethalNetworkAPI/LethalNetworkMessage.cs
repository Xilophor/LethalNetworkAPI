using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LethalNetworkAPI.Networking;
using Unity.Netcode;

namespace LethalNetworkAPI;

public class LethalNetworkMessage<T>
{
    #region Public Constructors
    /// <summary>
    /// Create a new network message of an <a href="https://docs-multiplayer.unity3d.com/netcode/1.5.2/advanced-topics/serialization/serialization-intro/">allowed type.</a>
    /// </summary>
    /// <param name="guid">An identifier for the message. GUIDs are specific to a per-mod basis.</param>
    /// <example><code> customStringMessage = new LethalNetworkMessage&lt;string&gt;(guid: "customStringMessageGuid");</code></example>
    public LethalNetworkMessage(string guid)
    {
        _messageGuid = $"{Assembly.GetCallingAssembly().GetName().Name}.message.{guid}";
        NetworkHandler.OnMessage += ReceiveMessage;

        Plugin.Logger.LogDebug($"NetworkMessage with guid \"{_messageGuid}\" has been created.");
    }
    
    #endregion

    #region Public Methods and Event
    /// <summary>
    /// Send data to the server/host.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    public void SendServer(T data)
    {
        NetworkHandler.Instance.MessageServerRpc(_messageGuid, JsonParser.Parse(data));
        
        Plugin.Logger.LogDebug("Attempted to Send Message to Server with data: "+JsonParser.Parse(data));
    }

    /// <summary>
    /// Send data to a specific client.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="clientId">The client to send the data to.</param>
    public void SendClient(T data, ulong clientId)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return;
        
        NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonParser.Parse(data), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { clientId } } } );
    }
    
    /// <summary>
    /// Send data to specific clients.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="clientIds">The clients to send the data to.</param>
    public void SendClients(T data, IEnumerable<ulong> clientIds)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;

        var allowedClientIds = clientIds.Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray();
        
        if (!allowedClientIds.Any()) return;
        
        NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonParser.Parse(data), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = allowedClientIds } } );
    }

    /// <summary>
    /// Send data to all clients.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="receiveOnHost">Whether the host client should receive as well. Only set to <c>false</c> when absolutely necessary</param>
    public void SendAllClients(T data, bool receiveOnHost = true)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;
        
        if (receiveOnHost)
            NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonParser.Parse(data));
        else
        {
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != NetworkManager.ServerClientId).ToArray();

            if (!clientIds.Any()) return;
            
            NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonParser.Parse(data), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = clientIds } } );
        }
        
        Plugin.Logger.LogDebug($"Attempted to Send Message to All Clients {receiveOnHost} with data: {JsonParser.Parse(data)}");
    }
    
    /// <summary>
    /// The callback to invoke when a message is received.
    /// </summary>
    /// <example><code>customStringMessage.OnReceived += CustomMethod; &#xA; &#xA;private static void CustomMethod(string data)</code></example>
    public event Action<T> OnReceived;

    #endregion

    private void ReceiveMessage(string guid, string message)
    {
        if (guid != _messageGuid) return;
        
        OnReceived?.Invoke((T)JsonParser.Parse(message));
    }

    private readonly string _messageGuid;
}