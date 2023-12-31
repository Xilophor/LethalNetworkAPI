using System;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

namespace LethalNetworkAPI.Networking;

internal class NetworkHandler : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if ((NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) && Instance != null)
            Instance.gameObject.GetComponent<NetworkObject>().Despawn(); 
        Instance = this;
    }

    #region Messages
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, bool[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, char[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, sbyte[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, byte[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, short[] message) =>
        OnMessage?.Invoke(guid, message, true);

    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, ushort[] message) =>
        OnMessage?.Invoke(guid, message, true);

    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, int[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, uint[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, long[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, ulong[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, float[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, double[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, string[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Color[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Color32[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Vector2[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Vector3[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Vector4[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Quaternion[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Ray[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    [ServerRpc(RequireOwnership = false)]
    internal void MessageServerRpc(string guid, Ray2D[] message) =>
        OnMessage?.Invoke(guid, message, true);
    
    
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, bool[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, char[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, sbyte[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, byte[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, short[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);

    [ClientRpc]
    internal void MessageClientRpc(string guid, ushort[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);

    [ClientRpc]
    internal void MessageClientRpc(string guid, int[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, uint[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, long[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, ulong[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, float[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, double[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, string[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Color[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Color32[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Vector2[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Vector3[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Vector4[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Quaternion[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Ray[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);
    
    [ClientRpc]
    internal void MessageClientRpc(string guid, Ray2D[] message, ClientRpcParams clientRpcParams = default) =>
        OnMessage?.Invoke(guid, message, false);

    #endregion Messages

    #region Events

    [ServerRpc(RequireOwnership = false)]
    internal void EventServerRpc(string guid)
    {
        OnEvent?.Invoke(guid, true);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received server data: {guid}");
#endif
    }

    [ClientRpc]
    internal void EventClientRpc(string guid, ClientRpcParams clientRpcParams = default)
    {
        OnEvent?.Invoke(guid, false);
        
#if DEBUG
        Plugin.Logger.LogDebug($"Received event with guid: {guid}");
#endif
    }

    #endregion Events

    internal static NetworkHandler Instance { get; private set; }
    
    internal static event Action<string, object, bool> OnMessage; // guid, message, isServerMessage
    internal static event Action<string, bool> OnEvent; // guid, isServerEvent
}