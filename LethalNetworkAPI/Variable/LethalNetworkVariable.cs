using OdinSerializer;
using Unity.Collections;

namespace LethalNetworkAPI.Variable;

internal interface ILethalNetVar; // To allow lists of any variable type

/// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
public class LethalNetworkVariable<TData> : ILethalNetVar
{
    #region Constructors

    /// <summary>
    /// Create a new server-owned network variable, unless otherwise specified with <c>[PublicNetworkVariable]</c>.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis.</remarks>
    public LethalNetworkVariable(string identifier) : this(identifier, null) { }

    internal LethalNetworkVariable(string identifier, NetworkObject? owner)
    {
        VariableIdentifier = $"{Assembly.GetCallingAssembly().GetName().Name}.var.{identifier}";
        _ownerObject = owner;
        
        NetworkHandler.OnVariableUpdate += ReceiveUpdate;
        NetworkHandler.NetworkTick += OnNetworkTick;
        NetworkHandler.NetworkSpawn += () =>  {
            if (NetworkHandler.Instance != null &&
                (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
            {
                // Send variable data when a player joins (if variable is created outside of playtime (in main menu))
                NetworkHandler.OnPlayerJoin += OnPlayerJoin;
            }
        };
        
        // Send variable data when a variable is initialized during playtime (in lobby)
        NetworkHandler.GetVariableValue += (id, clientId) =>
        {
            if (id != VariableIdentifier) return;
            
            if (NetworkHandler.Instance == null)
            {
                Plugin.Logger.LogError(string.Format(TextDefinitions.NetworkHandlerDoesNotExist));
                return;
            }
            
            if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)) return;
            
            NetworkHandler.Instance.UpdateVariableClientRpc(VariableIdentifier,
                SerializationUtility.SerializeValue(_value, DataFormat.Binary), new ClientRpcParams
                {
                    Send = { TargetClientIdsNativeArray = new NativeArray<ulong>(new[] { clientId }, Allocator.Persistent) }
                });
        };

        if (typeof(LethalNetworkVariable<TData>).GetCustomAttributes(typeof(PublicNetworkVariableAttribute), true).Any())
            _public = true;

        if (NetworkHandler.Instance != null && 
            !(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            NetworkHandler.Instance.GetVariableValueServerRpc(VariableIdentifier);
        }
        else if (NetworkHandler.Instance != null &&
                 (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            // Send variable data when a player joins (if variable is created during playtime (in lobby))
            NetworkHandler.OnPlayerJoin += OnPlayerJoin;
        }

#if DEBUG
        Plugin.Logger.LogDebug($"NetworkVariable with identifier \"{VariableIdentifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Properties & Events

    /// <summary>
    /// Get or set the value of the variable.
    /// </summary>
    public TData Value
    {
        get { return _value!; }
        set
        {
            if (!(
                _public || 
                (_ownerObject is null && NetworkManager.Singleton.IsServer) || 
                _ownerObject is null || 
                _ownerObject.OwnerClientId == NetworkManager.Singleton.LocalClientId
            )) return;
            
            if (value is null) return;
            
            if (value.Equals(_value)) return;
            
            _value = value;
            
#if DEBUG
            Plugin.Logger.LogDebug($"New Value: ({typeof(TData).FullName}) {_value}");
#endif
            
            OnValueChanged?.Invoke(_value);
        }
    }

    /// <summary>
    /// The callback to invoke when the variable's value changes.
    /// </summary>
    /// <remarks>Invoked when changed locally and on the network.</remarks>
    public event Action<TData>? OnValueChanged;

    #endregion

    #region Private Methods

    private void OnPlayerJoin(ulong clientId)
    {
        if (NetworkHandler.Instance is null)
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NetworkHandlerDoesNotExist));
            return;
        }

        if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)) return;

#if DEBUG
        Plugin.Logger.LogDebug($"Player Joined! Sending {VariableIdentifier}'s value.");
#endif
        
        NetworkHandler.Instance.UpdateVariableClientRpc(VariableIdentifier,
            SerializationUtility.SerializeValue(_value, DataFormat.Binary), new ClientRpcParams
            {
                Send = { TargetClientIdsNativeArray = new NativeArray<ulong>(new[] { clientId }, Allocator.Persistent) }
            });
    }

    private void SendUpdate()
    {
        if (NetworkHandler.Instance is null)
        {
            Plugin.Logger.LogError(string.Format(TextDefinitions.NetworkHandlerDoesNotExist));
            return;
        }

#if DEBUG
        Plugin.Logger.LogDebug($"New Value: ({typeof(TData).FullName}) {_value}; {SerializationUtility.SerializeValue(_value, DataFormat.Binary)}");
#endif
        
        NetworkHandler.Instance.UpdateVariableServerRpc(VariableIdentifier,
            SerializationUtility.SerializeValue(_value, DataFormat.Binary));
    }
    
    private void ReceiveUpdate(string identifier, byte[] data)
    {
        if (identifier != VariableIdentifier) return;

        var newValue = SerializationUtility.DeserializeValue<TData>(data, DataFormat.Binary);

        if (newValue == null) return;
        if (newValue.Equals(_previousValue)) return;

        _previousValue = newValue;
        Value = newValue;
        
#if DEBUG
        Plugin.Logger.LogDebug($"New Value: ({typeof(TData).FullName}) {newValue}");
#endif
        
        OnValueChanged?.Invoke(newValue);
    }

    private void OnNetworkTick()
    {
        if (_value is null) return;
        if (_value.Equals(_previousValue)) return;
        
        _previousValue = _value;
        SendUpdate();
    }

    #endregion

    internal readonly string VariableIdentifier;

    #region Private Variables
    private readonly bool _public;
    private readonly NetworkObject? _ownerObject;
    
    private TData? _previousValue;
    private TData? _value;
    
    #endregion
}

/// <summary>
/// Declare <see cref="LethalNetworkVariable{TData}" /> as public.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PublicNetworkVariableAttribute : Attribute;