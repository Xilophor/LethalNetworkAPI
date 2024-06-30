namespace LethalNetworkAPI.Old;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Networking;
using Unity.Collections;
using Unity.Netcode;

/// <summary>
/// Internal class.
/// </summary>
[Obsolete]
public abstract class LethalNetworkDeprecated
{

    internal readonly string Identifier = null!;
    private readonly string _networkType = null!;

    protected LethalNetworkDeprecated(string identifier, string networkType, int frameIndex = 3)
    {
        try
        {
            var m = new StackTrace().GetFrame(frameIndex).GetMethod();
            var assembly = m.ReflectedType!.Assembly;
            var pluginType = AccessTools.GetTypesFromAssembly(assembly).First(type => type.GetCustomAttributes(typeof(BepInPlugin), false).Any());

            this.Identifier = $"{MetadataHelper.GetMetadata(pluginType).GUID}.{identifier}";
            this._networkType = networkType;

#if DEBUG
            LethalNetworkAPIPlugin.Logger.LogDebug($"LethalNetwork {this._networkType} with identifier \"{this.Identifier}\" has been created.");
#endif
        }
        catch (Exception e)
        {
            LethalNetworkAPIPlugin.Logger.LogError(e);
        }
    }

    #region Error Checks

    /// <returns>true if it is null</returns>
    protected bool IsNetworkHandlerNull(bool log = true)
    {
        if (NetworkHandler.Instance != null) return false;

        if (log) LethalNetworkAPIPlugin.Logger.LogError(string.Format(
            TextDefinitions.NotInLobbyEvent, this._networkType.ToLower(), this.Identifier));
        return true;
    }

    /// <returns>true if it is host</returns>
    protected bool IsHostOrServer(bool log = true)
    {
        if (NetworkManager.Singleton == null) return false;
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) return true;

        if (log) LethalNetworkAPIPlugin.Logger.LogError(string.Format(
            TextDefinitions.NotServerInfo, NetworkManager.Singleton.LocalClientId, this._networkType, this.Identifier));

        return false;
    }

    /// <returns>true if it exists</returns>
    private bool DoClientsExist(IEnumerable<ulong> clientIds, bool log = true)
    {
        if (clientIds.Any()) return true;

        if (log) LethalNetworkAPIPlugin.Logger.LogError(string.Format(
            TextDefinitions.TargetClientsNotConnected, clientIds, this._networkType, this.Identifier));

        return false;
    }

    #endregion

    #region ClientRpcParams

    private ClientRpcParams GenerateClientParams(IEnumerable<ulong> clientIds, bool allExcept)
    {
        NativeArray<ulong> allowedClientIds;

        var enumerable = clientIds as ulong[] ?? clientIds.ToArray();

        if (!enumerable.Any() && allExcept)
            allowedClientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);
        else if (allExcept)
            allowedClientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => enumerable.All(j => i != j)).ToArray(), Allocator.Persistent);
        else
            allowedClientIds = new NativeArray<ulong>(enumerable
                .Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray(), Allocator.Persistent);

        if (!this.DoClientsExist(allowedClientIds)) return new ClientRpcParams();

        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIdsNativeArray = allowedClientIds
            }
        };
    }

    protected ClientRpcParams GenerateClientParams(IEnumerable<ulong> clientIds) => this.GenerateClientParams(clientIds, false);
    protected ClientRpcParams GenerateClientParams(ulong clientId) => this.GenerateClientParams([clientId], false);

    protected ClientRpcParams GenerateClientParamsExcept(IEnumerable<ulong> clientIds) => this.GenerateClientParams(clientIds, true);
    protected ClientRpcParams GenerateClientParamsExcept(ulong clientId) => this.GenerateClientParams([clientId], true);

    protected ClientRpcParams GenerateClientParamsExceptHost() => this.GenerateClientParams([], true);

    #endregion
}
