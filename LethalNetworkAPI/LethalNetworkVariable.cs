namespace LethalNetworkAPI;

/// <typeparam name="T">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class LethalNetworkVariable<T>
{
    #region Public Constructors
    /// <summary>
    /// Create a new network variable.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalNetworkVariable(string identifier)
    {
        _variableIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.var.{identifier}";
        NetworkHandler.OnVariableUpdate += ReceiveUpdate;
        NetworkHandler.OnOwnershipChange += OwnershipChange;
        NetworkHandler.NetworkTick += OnNetworkTick;

        if (typeof(LethalNetworkVariable<T>).GetCustomAttributes(typeof(LethalNetworkProtectedAttribute), true).Any())
            _protect = true;
        
#if DEBUG
        Plugin.Logger.LogDebug($"NetworkVariable with identifier \"{_variableIdentifier}\" has been created.");
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
        if (!_protect || NetworkHandler.Instance == null) return false;
        
        if (!(NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId ||
              NetworkManager.Singleton.LocalClientId == _ownerClientId) && _ownerClientId != DefaultId) return false;

        if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId)) return false;
        
        if (NetworkManager.Singleton.IsServer)
            NetworkHandler.Instance.UpdateOwnershipClientRpc(_variableIdentifier, [_ownerClientId, clientId]);
        else
            NetworkHandler.Instance.UpdateOwnershipServerRpc(_variableIdentifier, clientId);
        
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
        get { return _value!; }
        set
        {
            if (_protect && _ownerClientId == DefaultId) return;
            if (value == null) return;
            
            if (value.Equals(_value)) return;
            
            _value = value;
            
#if DEBUG
            Plugin.Logger.LogDebug($"New Value: ({typeof(T).FullName}) {_value}");
#endif
            
            OnValueChanged?.Invoke(_value);
        }
    }

    /// <summary>
    /// The callback to invoke when the variable's value changes.
    /// </summary>
    /// <remarks>Invoked when changed locally and on the network.</remarks>
    public event Action<T>? OnValueChanged;

    #endregion

    #region Private Methods

    private void SendUpdate()
    {
        if (NetworkHandler.Instance == null) return;
        
#if DEBUG
        Plugin.Logger.LogDebug($"New Value: ({typeof(T).FullName}) {_value}; {JsonUtility.ToJson(new ValueWrapper<T>(_value))}");
#endif
        
        NetworkHandler.Instance.UpdateVariableServerRpc(_variableIdentifier,
            JsonUtility.ToJson(new ValueWrapper<T>(_value)));
    }
    
    private void ReceiveUpdate(string identifier, string data)
    {
        if (identifier != _variableIdentifier) return;

        var newValue = JsonUtility.FromJson<ValueWrapper<T>>(data).var;

        if (newValue == null) return;
        if (newValue.Equals(_previousValue)) return;

        _previousValue = newValue;
        Value = newValue;
        
#if DEBUG
        Plugin.Logger.LogDebug($"New Value: ({typeof(T).FullName}) {newValue}");
#endif
        
        OnValueChanged?.Invoke(newValue);
    } 

    private void OwnershipChange(string identifier, ulong[] clientIds)
    {
        if (identifier != _variableIdentifier) return;

        if (_ownerClientId != clientIds[0] && _ownerClientId != DefaultId) return;

        _ownerClientId = clientIds[1];
    }

    private void OnNetworkTick()
    {
        if (_value == null) return;
        if (_value.Equals(_previousValue)) return;
        
        _previousValue = _value;
        SendUpdate();
    }

    #endregion

    #region Private Variables

    private readonly string _variableIdentifier;
    private readonly bool _protect;
    
    private T? _previousValue;
    private T? _value;
    
    private const ulong DefaultId = 999999;
    private ulong _ownerClientId = DefaultId;
    
    #endregion
}

/// <summary>
/// Declare <see cref="LethalNetworkVariable&lt;T&gt;" /> as protected.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LethalNetworkProtectedAttribute : Attribute;