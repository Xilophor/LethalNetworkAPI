using System.Diagnostics;
using BepInEx;
using HarmonyLib;
using LethalNetworkAPI.Serializable;
using Unity.Collections;

namespace LethalNetworkAPI;

internal interface ILethalNetVar; // To allow lists of any variable type

/// <typeparam name="TData">The serializable data type of the message.</typeparam>
public class LethalNetworkVariable<TData> : ILethalNetVar
{
    #region Constructors

    /// <summary>
    /// Create a new server-owned network variable, unless otherwise specified with <c>[PublicNetworkVariable]</c>.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <remarks>Identifiers are specific to a per-mod basis. MUST be used outside of patches.</remarks>
    public LethalNetworkVariable(string identifier) : this(identifier, null, true, 2) { }

    internal LethalNetworkVariable(string identifier, NetworkObject? owner, bool serverOwned, int frameIndex)
    {
        try
        {
            var m = new StackTrace().GetFrame(frameIndex).GetMethod();
            var assembly = m.ReflectedType!.Assembly;
            var pluginType = AccessTools.GetTypesFromAssembly(assembly)
                .First(type => type.GetCustomAttributes(typeof(BepInPlugin), false).Any());

            VariableIdentifier = $"{MetadataHelper.GetMetadata(pluginType).GUID}.var.{identifier}";
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Unable to find plugin info for calling mod with identifier {identifier}. Are you using BepInEx? \n Stacktrace: {e}");
            return;
        }
        //VariableIdentifier = $"var.{identifier}"; // Due to patching limitations, I cannot get the guid of the mod assembly.
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
            if (id != VariableIdentifier) return;
            
            if (NetworkHandler.Instance == null)
            {
                Plugin.Logger.LogError(string.Format(TextDefinitions.NetworkHandlerDoesNotExist));
                return;
            }
            
            if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)) return;
            
            NetworkHandler.Instance.UpdateVariableClientRpc(VariableIdentifier,
                LethalNetworkSerializer.Serialize<TData>(_value!), new ClientRpcParams
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
            LethalNetworkSerializer.Serialize(_value), new ClientRpcParams
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
        Plugin.Logger.LogDebug($"New Value: ({typeof(TData).FullName}) {_value}; {LethalNetworkSerializer.Serialize(_value)}");
#endif
        
        NetworkHandler.Instance.UpdateVariableServerRpc(VariableIdentifier,
            LethalNetworkSerializer.Serialize(_value));
    }
    
    private void ReceiveUpdate(string identifier, byte[] data)
    {
        if (identifier != VariableIdentifier) return;

        var newValue = LethalNetworkSerializer.Deserialize<TData>(data);

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
    
    private TData _previousValue = default!;
    private TData _value = default!;
    
    #endregion
}

/// <summary>
/// Declare <see cref="LethalNetworkVariable{TData}" /> as public.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class PublicNetworkVariableAttribute : Attribute;