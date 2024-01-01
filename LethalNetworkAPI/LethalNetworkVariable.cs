using System;
using System.Reflection;
using LethalNetworkAPI.Networking;
using Unity.Netcode;
using UnityEngine;

namespace LethalNetworkAPI;

public class LethalNetworkVariable<T>
{
    #region Public Constructors
    /// <summary>
    /// Create a new network variable of a serializable type. See <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">Unity Serialization Docs</a> for specifics.
    /// </summary>
    /// <param name="guid">An identifier for the variable. GUIDs are specific to a per-mod basis.</param>
    /// <example><code> customEvent = new LethalNetworkEvent(guid: "customStringMessageGuid");</code></example>
    public LethalNetworkVariable(string guid)
    {
        _variableGuid = $"{Assembly.GetCallingAssembly().GetName().Name}.evt.{guid}";
        NetworkHandler.OnVariableUpdate += ReceiveUpdate;
        NetworkHandler.OnOwnershipChange += OwnershipChange;
        OnValueChange += SendUpdate;
        
#if DEBUG
        Plugin.Logger.LogDebug($"NetworkVariable with guid \"{_variableGuid}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods

    public bool SetOwnership(ulong clientId)
    {
        if (!(NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId ||
              NetworkManager.Singleton.LocalClientId == _ownerClientId || _ownerClientId == DefaultId)) return false;

        
        if (NetworkManager.Singleton.IsServer)
            NetworkHandler.Instance.UpdateOwnershipClientRpc(_variableGuid, new []{_ownerClientId, clientId});
        else
            NetworkHandler.Instance.UpdateOwnershipServerRpc(_variableGuid, clientId);
        
        _ownerClientId = clientId;
        
        return true;
    }

    #endregion

    private void SendUpdate(T data)
    {
        NetworkHandler.Instance.UpdateVariableServerRpc(_variableGuid, JsonUtility.ToJson(data));
    }
    
    private void ReceiveUpdate(string guid, string data)
    {
        if (guid != _variableGuid) return;

        var newValue = JsonUtility.FromJson<T>(data);

        if (newValue.Equals(_previousValue)) return;

        _previousValue = newValue;
        OnValueChange?.Invoke(newValue);
    }

    private void OwnershipChange(string guid, ulong[] clientIds)
    {
        if (guid != _variableGuid) return;

        if (_ownerClientId != clientIds[0] && _ownerClientId != DefaultId) return;

        _ownerClientId = clientIds[1];
    }

    private readonly string _variableGuid;
    
    private T _previousValue;
    private const ulong DefaultId = 123412341234;
    private ulong _ownerClientId = DefaultId;

    public T Value
    {
        get => default;
        set
        {
            if (_ownerClientId == DefaultId) return;
            if (value.Equals(_previousValue)) return;
            
            _previousValue = value; 
            OnValueChange?.Invoke(value);
        }
    }

    public event Action<T> OnValueChange;
}