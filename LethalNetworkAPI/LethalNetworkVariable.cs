using System;
using System.Linq;
using System.Reflection;
using LethalNetworkAPI.Networking;
using Unity.Netcode;
using UnityEngine;

namespace LethalNetworkAPI;

/// <typeparam name="T">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class LethalNetworkVariable<T>
{
    #region Public Constructors
    /// <summary>
    /// Create a new network variable.
    /// </summary>
    /// <param name="guid">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>GUIDs are specific to a per-mod basis.</remarks>
    public LethalNetworkVariable(string guid)
    {
        _variableGuid = $"{Assembly.GetCallingAssembly().GetName().Name}.var.{guid}";
        NetworkHandler.OnVariableUpdate += ReceiveUpdate;
        NetworkHandler.OnOwnershipChange += OwnershipChange;
        NetworkHandler.NetworkTick += OnNetworkTick;

        if (typeof(LethalNetworkVariable<T>).GetCustomAttributes(typeof(LethalNetworkProtectedAttribute), true).Any())
            _protect = true;
        
#if DEBUG
        Plugin.Logger.LogDebug($"NetworkVariable with guid \"{_variableGuid}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Set ownership of the Network Variable so only the owner client can change the values. Only applies if the Network Variable as the <see cref="LethalNetworkProtectedAttribute"/> attribute.
    /// </summary>
    /// <param name="clientId">(<see cref="ulong"/>) The client ID of the new owner.</param>
    /// <returns>(<see cref="bool"/>) Whether new owner was able to be set.</returns>
    public bool SetOwnership(ulong clientId)
    {
        if (!_protect) return false;
        
        if (!(NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId ||
              NetworkManager.Singleton.LocalClientId == _ownerClientId) && _ownerClientId != DefaultId) return false;

        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return false;
        
        if (NetworkManager.Singleton.IsServer)
            NetworkHandler.Instance.UpdateOwnershipClientRpc(_variableGuid, [_ownerClientId, clientId]);
        else
            NetworkHandler.Instance.UpdateOwnershipServerRpc(_variableGuid, clientId);
        
        _ownerClientId = clientId;
        
        return true;
    }

    #endregion

    #region Public Properties & Events

    /// <summary>
    /// Get or set the value of the variable.
    /// </summary>
    public T Value
    {
        get => default;
        set
        {
            if (_protect && _ownerClientId == DefaultId) return;
            if (value.Equals(_previousValue)) return;
            
            OnValueChange?.Invoke(value);
        }
    }

    /// <summary>
    /// The callback to invoke when the variable's value changes.
    /// </summary>
    /// <remarks>Invoked when changed locally and on the network.</remarks>
    public event Action<T> OnValueChange;

    #endregion

    #region Private Methods

    private void SendUpdate()
    {
        NetworkHandler.Instance.UpdateVariableServerRpc(_variableGuid, JsonUtility.ToJson(Value));
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

    private void OnNetworkTick()
    {
        if (_previousValue.Equals(Value)) return;
        
        _previousValue = Value;
        SendUpdate();
    }

    #endregion

    #region Private Variables

    private readonly string _variableGuid;
    private readonly bool _protect;
    
    private T _previousValue;
    private const ulong DefaultId = 123412341234;
    private ulong _ownerClientId = DefaultId;
    
    #endregion
}

/// <summary>
/// Declare <see cref="LethalNetworkVariable&lt;T&gt;" /> as protected.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LethalNetworkProtectedAttribute : Attribute {}