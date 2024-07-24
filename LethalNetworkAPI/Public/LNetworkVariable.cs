namespace LethalNetworkAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Internal;
using Unity.Netcode;
using Utils;

internal interface INetVariable;

/// <summary>
/// A variable that can be used to send data between clients.
/// </summary>
/// <typeparam name="TData">The type of data to send.</typeparam>
/// <remarks>The type should not be mutable, otherwise you will have to manually use <see cref="MakeDirty"/> upon modification (e.g. add, remove) of the mutable value.</remarks>
public class LNetworkVariable<TData> : LNetworkVariableBase
{
    private TData _offlineValue;

    private TData _value;
    private TData _previousValue;

    internal bool Dirty { get; set; }

    #region Public Fields & Properties

    /// <summary>
    /// The "default" value of the variable. This value will be used when starting a new game.
    /// </summary>
    public TData OfflineValue
    {
        get => this._offlineValue;
        set
        {
            this._offlineValue = value;

            if (NetworkManager.Singleton == null)
            {
                this.ResetValue();
            }
        }
    }

    /// <summary>
    /// The current value of the variable.
    /// </summary>
    /// <remarks>This value cannot be modified when disconnected. To change the "default" value, use <see cref="OfflineValue"/>.</remarks>
    public TData Value
    {
        get => this._value;
        set
        {
            if(Equals(this._value, value)) return;

            this.SetDirty(true);
            this._previousValue = this._value;
            this._value = value;
            this.OnValueChanged?.Invoke(this._previousValue, this._value);
        }
    }

    /// <summary>
    /// A callback that runs when the value of the variable changes.
    /// </summary>
    public event Action<TData, TData>? OnValueChanged = delegate { };

    #endregion

    #region Constructors

    private LNetworkVariable(
        string identifier,
        TData offlineValue,
        LNetworkVariableWritePerms writePerms,
        Action<TData, TData>? onValueChanged)
    {
        if (UnnamedMessageHandler.LNetworkVariables.TryGetValue(identifier, out var _))
        {
            throw new InvalidOperationException
            (
                $"A variable with the identifier {identifier} already exists! " +
                "Please use a different identifier."
            );
        }

        this.Identifier = identifier;
        this.WritePerms = writePerms;

        this._offlineValue = offlineValue;
        this._value = offlineValue;
        this._previousValue = offlineValue;

        this.OnValueChanged += onValueChanged;

        UnnamedMessageHandler.VariableCheck += this.CheckForDirt;
        UnnamedMessageHandler.LNetworkVariables.Add(this.Identifier, this);

        this.SetDirty(true);
    }

    /// <summary>
    /// Create a new <see cref="LNetworkVariable{TData}"/> if it doesn't already exist,
    /// otherwise return the existing variable of the same identifier.
    /// </summary>
    /// <param name="identifier">The identifier of the <see cref="LNetworkVariable{TData}"/>.</param>
    /// <param name="offlineValue">[Opt.] The default value of the <see cref="LNetworkVariable{TData}"/>. This value will be used when starting a new game.</param>
    /// <param name="writePerms">[Opt.] Who can modify the value of the <see cref="LNetworkVariable{TData}"/>. Defaults to <see cref="LNetworkVariableWritePerms.Server"/>. Will be ignored if the variable is already created.</param>
    /// <param name="onValueChanged">[Opt.] The method to run when the value of the <see cref="LNetworkVariable{TData}"/> changes.</param>
    /// <returns>The <see cref="LNetworkVariable{TData}"/>.</returns>
    /// <remarks>If you set the <paramref name="writePerms"/> to <see cref="LNetworkVariableWritePerms.Owner"/>, only the owner of the variable can modify it. See <see cref="UpdateOwner(int[])"/> to update ownership.</remarks>
    public static LNetworkVariable<TData> Connect(
        string identifier,
        TData offlineValue = default!,
        LNetworkVariableWritePerms writePerms = LNetworkVariableWritePerms.Server,
        Action<TData, TData>? onValueChanged = null)
    {
        string actualIdentifier;

        try
        {
            actualIdentifier = $"{LNetworkUtils.GetModGuid(2)}.{identifier}";
        }
        catch (Exception e)
        {
            LethalNetworkAPIPlugin.Logger.LogError($"Unable to find Mod Guid! To still work, this Message will only use the given identifier. " +
                                                   $"Warning: This may cause collision with another mod's NetworkMessage! Stack Trace: {e}");
            actualIdentifier = identifier;
        }

        if (!UnnamedMessageHandler.LNetworkVariables.TryGetValue(actualIdentifier, out var variable))
            return new LNetworkVariable<TData>(actualIdentifier, offlineValue, writePerms, onValueChanged);

        var networkVariable = (LNetworkVariable<TData>)variable;

        networkVariable.OfflineValue = offlineValue;
        networkVariable.OnValueChanged += onValueChanged;

        return networkVariable;
    }

    /// <summary>
    /// Create a new <see cref="LNetworkVariable{TData}"/> if it doesn't already exist. If it already exists, an exception will be thrown.
    /// </summary>
    /// <param name="identifier">The identifier of the <see cref="LNetworkVariable{TData}"/>.</param>
    /// <param name="defaultValue">[Opt.] The default value of the <see cref="LNetworkVariable{TData}"/>. This value will be used when starting a new game.</param>
    /// <param name="writePerms">[Opt.] Who can modify the value of the <see cref="LNetworkVariable{TData}"/>. Defaults to <see cref="LNetworkVariableWritePerms.Server"/>.</param>
    /// <param name="onValueChanged">[Opt.] The method to run when the value of the <see cref="LNetworkVariable{TData}"/> changes.</param>
    /// <returns>The <see cref="LNetworkVariable{TData}"/>.</returns>
    /// <remarks>If you set the <paramref name="writePerms"/> to <see cref="LNetworkVariableWritePerms.Owner"/>, only the owner of the variable can modify it. See <see cref="UpdateOwner(int[])"/> to update ownership.</remarks>
    public static LNetworkVariable<TData> Create(
        string identifier,
        TData defaultValue = default!,
        LNetworkVariableWritePerms writePerms = LNetworkVariableWritePerms.Server,
        Action<TData, TData>? onValueChanged = null)
    {
        string actualIdentifier;

        try
        {
            actualIdentifier = $"{LNetworkUtils.GetModGuid(2)}.{identifier}";
        }
        catch (Exception e)
        {
            LethalNetworkAPIPlugin.Logger.LogError($"Unable to find Mod Guid! To still work, this Message will only use the given identifier. " +
                                                   $"Warning: This may cause collision with another mod's NetworkMessage! Stack Trace: {e}");
            actualIdentifier = identifier;
        }

        return new LNetworkVariable<TData>(actualIdentifier, defaultValue, writePerms, onValueChanged);
    }

    #endregion

    #region Private & Internal Methods

    private void CheckForDirt() => this.IsDirty();

    private void SetDirty(bool value)
    {
        if (value) UnnamedMessageHandler.Instance?.DirtyBois.Add(this);
        this.Dirty = value;
    }

    internal override bool IsDirty()
    {
        if (this.Dirty) return true;

        if (Equals(this._previousValue, this.Value)) return false;
        if (!this.CanWrite()) return false;

        if (typeof(TData).IsByRef)
            this._previousValue = (TData)AccessTools.MakeDeepCopy(this._value, typeof(TData));
        else
            this._previousValue = this._value;

        this.SetDirty(true);
        return true;
    }

    internal override void ResetValue()
    {
        this._value = this._previousValue = this._offlineValue;
        this.Dirty = false;
    }

    internal override void ResetDirty()
    {
        if (!this.Dirty) return;

        this._previousValue = this._value;

        this.Dirty = false;
    }

    internal override object? GetValue() => this._value;

    internal override void ReceiveUpdate(object? data)
    {
        var newValue = (TData)data!;

        if (!Equals(this._value, newValue))
            this.OnValueChanged?.Invoke(this._value, newValue);

        this._value = newValue;

        if (typeof(TData).IsByRef)
            this._previousValue = (TData)AccessTools.MakeDeepCopy(newValue, typeof(TData));
        else
            this._previousValue = newValue;

        this.Dirty = false;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Force the variable to send an update throughout the network.
    /// </summary>
    /// <remarks>
    /// This will not trigger the <see cref="OnValueChanged"/> event.
    /// </remarks>
    public void MakeDirty() => this.SetDirty(true);

    /// <summary>
    /// Update the owner of the variable.
    /// </summary>
    /// <param name="clientGuidArray">[Opt.] The NGO guids of the clients to update the owner of the variable.</param>
    /// <remarks>Can only be used by the server, and will be ignored if the <see cref="LNetworkVariableWritePerms"/> is not <see cref="LNetworkVariableWritePerms.Owner"/>.</remarks>
    public void UpdateOwner(params ulong[] clientGuidArray)
    {
        if (this.WritePerms != LNetworkVariableWritePerms.Owner)
            throw new InvalidOperationException($"The variable `{this.Identifier}` does not allow changing ownership (writePerms: {this.WritePerms}).");
        if (NetworkManager.Singleton == null)
            throw new InvalidOperationException($"Cannot modify ownership of the variable `{this.Identifier}` while not connected to a server!");
        if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            LethalNetworkAPIPlugin.Logger.LogWarning($"Cannot modify ownership of the variable `{this.Identifier}` if not the host!");
            return;
        }

        this.OwnerClients = clientGuidArray;
    }

    /// <summary>
    /// Update the owner of the variable.
    /// </summary>
    /// <param name="playerIdArray">[Opt.] The in-game ids of the clients to update the owner of the variable.</param>
    /// <remarks>Can only be used by the server, and will be ignored if the <see cref="LNetworkVariableWritePerms"/> is not <see cref="LNetworkVariableWritePerms.Owner"/>.</remarks>
    public void UpdateOwner(params int[] playerIdArray)
    {
        if (this.WritePerms != LNetworkVariableWritePerms.Owner)
            throw new InvalidOperationException($"The variable `{this.Identifier}` does not allow changing ownership (writePerms: {this.WritePerms}).");
        if (NetworkManager.Singleton == null)
            throw new InvalidOperationException($"Cannot modify ownership of the variable `{this.Identifier}` while not connected to a server!");
        if (!(NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost))
        {
            LethalNetworkAPIPlugin.Logger.LogWarning($"Cannot modify ownership of the variable `{this.Identifier}` if not the host!");
            return;
        }

        this.OwnerClients = playerIdArray.Select(LNetworkUtils.GetClientGuid).ToArray();
    }

    /// <summary>
    /// Dispose of the NetworkVariable. Updates will no longer be sent or received, and any internal references to the variable will be removed.
    /// </summary>
    /// <remarks>Only use this if you are sure you no longer need the variable.</remarks>
    public void Dispose()
    {
        UnnamedMessageHandler.LNetworkVariables.Remove(this.Identifier);
        UnnamedMessageHandler.Instance?.DirtyBois.Remove(this);
        this.OnValueChanged = null;
    }

    #endregion
}
