using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using LethalNetworkAPI.Networking;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LethalNetworkAPI;

public class LethalNetworkMessage<T>
{
    #region Constructor
    
    /// <summary>
    /// Create a new network message of a serializable type. See <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">Unity Serialization Docs</a> for specifics.
    /// </summary>
    /// <param name="guid">An identifier for the message. GUIDs are specific to a per-mod basis.</param>
    /// <example><code> customStringMessage = new LethalNetworkMessage&lt;string&gt;(guid: "customStringMessageGuid");</code></example>
    public LethalNetworkMessage(string guid)
    {
        _messageGuid = $"{Assembly.GetCallingAssembly().GetName().Name}.msg.{guid}";
        NetworkHandler.OnMessage += ReceiveMessage;

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkMessage with guid \"{_messageGuid}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods and Event
    
    /// <summary>
    /// Send data to the server/host.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    public void SendServer(T data)
    {
        NetworkHandler.Instance.MessageServerRpc(_messageGuid, JsonUtility.ToJson(data));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to Server with data: {data}");
#endif
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
        
        NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonUtility.ToJson(data), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = new NativeArray<ulong>(new [] {clientId}, Allocator.Persistent) } } );
    }
    
    /// <summary>
    /// Send data to specific clients.
    /// </summary>
    /// <param name="data">The data of type <typeparamref name="T"/> to send.</param>
    /// <param name="clientIds">The clients to send the data to.</param>
    public void SendClients(T data, IEnumerable<ulong> clientIds)
    {
        if (!(NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)) return;

        var allowedClientIds = new NativeArray<ulong>(clientIds.Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray(), Allocator.Persistent);
        
        if (!allowedClientIds.Any()) return;
        
        NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonUtility.ToJson(data), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = allowedClientIds } } );
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
            NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonUtility.ToJson(data));
        else
        {
            var clientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds.Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);

            if (!clientIds.Any()) return;
            
            NetworkHandler.Instance.MessageClientRpc(_messageGuid, JsonUtility.ToJson(data), new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = clientIds } } );
        }
        
#if DEBUG
        Plugin.Logger.LogDebug($"Attempted to Send Message to All Clients {receiveOnHost} with data: {data}; {JsonUtility.ToJson(data)}");
#endif
    }
    
    /// <summary>
    /// The callback to invoke when a message is received by the server.
    /// </summary>
    /// <example><code>customStringMessage.OnServerReceived += CustomMethod; &#xA; &#xA;private static void CustomMethod(string data)</code></example>
    public event Action<T> OnServerReceived;
    
    
    /// <summary>
    /// The callback to invoke when a message is received by the client.
    /// </summary>
    /// <example><code>customStringMessage.OnClientReceived += CustomMethod; &#xA; &#xA;private static void CustomMethod(string data)</code></example>
    public event Action<T> OnClientReceived;

    #endregion

    private void ReceiveMessage(string guid, string data, bool isServerMessage)
    {
        if (guid != _messageGuid) return;

        if (isServerMessage)
            OnServerReceived?.Invoke(JsonUtility.FromJson<T>(data));
        else
            OnClientReceived?.Invoke(JsonUtility.FromJson<T>(data));
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received data: {data}");
#endif
    }

    private readonly string _messageGuid;
}