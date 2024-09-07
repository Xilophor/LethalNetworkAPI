namespace LethalNetworkAPI.Internal;

using System.Linq;
using Unity.Netcode;

/// <remarks>Internal Class</remarks>
public abstract class LNetworkVariableBase
{
    internal string Identifier { get; init; }
    internal LNetworkVariableWritePerms WritePerms { get; init; }
    internal ulong[]? OwnerClients { get; set; }

    internal abstract void ReceiveUpdate(object? data);
    internal abstract void ResetValue();
    internal abstract bool IsDirty();
    internal abstract void ResetDirty();
    internal abstract object? GetValue();

    internal bool CanWrite() =>
        this.WritePerms switch
        {
            LNetworkVariableWritePerms.Owner => this.OwnerClients != null && this.OwnerClients.Contains(NetworkManager.Singleton.LocalClientId),
            LNetworkVariableWritePerms.Server => NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost,
            LNetworkVariableWritePerms.Everyone => true,
            _ => false
        };
}
