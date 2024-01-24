using System.Diagnostics;
using BepInEx;
using HarmonyLib;
using LethalNetworkAPI.Serializable;
using Unity.Collections;

namespace LethalNetworkAPI;

internal interface ILethalNetVar; // To allow lists of any variable type

/// <typeparam name="TData">The serializable data type of the message.</typeparam>
public class LethalNetworkVariable<TData> : LethalNetwork, ILethalNetVar
{
    #region Constructors

    /// <summary>
    /// Create a new server-owned network variable, unless otherwise specified with <c>[PublicNetworkVariable]</c>.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis. MUST be used outside of patches.</remarks>
    public LethalNetworkVariable(string identifier) : this(identifier, null, true, 2) { }

    internal LethalNetworkVariable(string identifier, NetworkObject? owner, bool serverOwned, int frameIndex) : base(identifier, frameIndex + 1)
    {
        _ownerObject = (!serverOwned) ? owner : null;
        
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
            if (id != Identifier) return;
            
            if (NetworkHandler.Instance == null)
            {
                LethalNetworkAPIPlugin.Logger.LogError(string.Format(TextDefinitions.NetworkHandlerDoesNotExist));
                return;
            }
            
            if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)) return;
            
            NetworkHandler.Instance.UpdateVariableClientRpc(Identifier,
                LethalNetworkSerializer.Serialize(_value), 
                clientRpcParams: GenerateClientParams(clientId));
        };

        if (typeof(LethalNetworkVariable<TData>).GetCustomAttributes(typeof(PublicNetworkVariableAttribute), true).Any())
            _public = true;

        if (NetworkHandler.Instance != null && 
            !(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            NetworkHandler.Instance.GetVariableValueServerRpc(Identifier);
        }
        else if (NetworkHandler.Instance != null &&
                 (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            // Send variable data when a player joins (if variable is created during playtime (in lobby))
            NetworkHandler.OnPlayerJoin += OnPlayerJoin;
        }

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug($"NetworkVariable with identifier \"{Identifier}\" has been created.");
#endif
    }
    
    #endregion

    #region Public Properties & Events

    /// <summary>
    /// Get or set the value of the variable.
    /// </summary>
    public TData Value
    {
        get => _value;
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
            _isDirty = true;
            
#if DEBUG
            LethalNetworkAPIPlugin.Logger.LogDebug($"New Value: ({typeof(TData).FullName}) {_value}");
#endif
            
            OnValueChanged?.Invoke(_value);
        }
    }

    // ReSharper disable once InvalidXmlDocComment
    /// <summary>
    /// The callback to invoke when the variable's value changes.
    /// </summary>
    /// <typeparam name="TData"> The received data.</typeparam>
    /// <remarks>Invoked when changed locally and on the network.</remarks>
    public event Action<TData>? OnValueChanged;

    #endregion

    #region Private Methods

    private void OnPlayerJoin(ulong clientId)
    {
        if (IsNetworkHandlerNull() || !IsHostOrServer()) return;

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug($"Player Joined! Sending {Identifier}'s value.");
#endif
        
        NetworkHandler.Instance!.UpdateVariableClientRpc(Identifier,
            LethalNetworkSerializer.Serialize(_value), 
            clientRpcParams: GenerateClientParams(clientId));
    }

    private void SendUpdate()
    {
        if (IsNetworkHandlerNull()) return;

#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug($"New Value: ({typeof(TData).FullName}) {_value}; {LethalNetworkSerializer.Serialize(_value)}");
#endif
        
        NetworkHandler.Instance!.UpdateVariableServerRpc(Identifier,
            LethalNetworkSerializer.Serialize(_value));
    }
    
    private void ReceiveUpdate(string identifier, byte[] data)
    {
        if (identifier != Identifier) return;

        var newValue = LethalNetworkSerializer.Deserialize<TData>(data);

        if (newValue == null) return;

        _value = newValue;
        
#if DEBUG
        LethalNetworkAPIPlugin.Logger.LogDebug($"New Value: ({typeof(TData).FullName}) {newValue}");
#endif
        
        OnValueChanged?.Invoke(newValue);
    }

    private void OnNetworkTick()
    {
        if (!_isDirty) return;
        if (_value is null) return;

        SendUpdate();
        _isDirty = false;
    }

    #endregion

    #region Private Variables
    
    private readonly bool _public;
    private readonly NetworkObject? _ownerObject;

    private bool _isDirty;
    private TData _value = default!;
    
    #endregion
}

/// <summary>
/// Declare <see cref="LethalNetworkVariable{TData}" /> as public.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PublicNetworkVariableAttribute : Attribute;