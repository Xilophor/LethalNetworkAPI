using System;
using System.Reflection;
using LethalNetworkAPI.Networking;
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
        OnValueChanged += SendUpdate;
        
#if DEBUG
        Plugin.Logger.LogDebug($"NetworkVariable with guid \"{_variableGuid}\" has been created.");
#endif
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
        OnValueChanged?.Invoke(newValue);
    }

    private readonly string _variableGuid;
    private readonly bool _protect;
    
    private T _previousValue;
    private ulong _ownerClientId;

    public T Value
    {
        get => default;
        set
        {
            if (_protect) return;
            _previousValue = value; 
            OnValueChanged?.Invoke(value);
        }
    }

    public event Action<T> OnValueChanged;
}