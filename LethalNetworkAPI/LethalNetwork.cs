using System.Collections.Generic;
using System.Diagnostics;
using BepInEx;
using HarmonyLib;
using Unity.Collections;
using ClientRpcSendParams = Unity.Netcode.ClientRpcSendParams;

namespace LethalNetworkAPI;

public abstract class LethalNetwork
{
    protected LethalNetwork(string identifier, int frameIndex = 3)
    {
        try
        {
            var m = new StackTrace().GetFrame(frameIndex).GetMethod();
            var assembly = m.ReflectedType!.Assembly;
            var pluginType = AccessTools.GetTypesFromAssembly(assembly).First(type => type.GetCustomAttributes(typeof(BepInPlugin), false).Any());
            
            Identifier = $"{MetadataHelper.GetMetadata(pluginType).GUID}.{identifier}";
        
#if DEBUG
            LethalNetworkAPIPlugin.Logger.LogDebug($"LethalNetwork with identifier \"{Identifier}\" has been created.");
#endif
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Unable to find plugin info for calling mod with identifier {identifier}. Are you using BepInEx? \n Stacktrace: {e}");
        }
    }

    #region Error Checks
    
    protected bool IsNetworkHandlerNull()
    {
        if (NetworkHandler.Instance != null) return false;
        
        LethalNetworkAPIPlugin.Logger.LogError(string.Format(
            TextDefinitions.NotInLobbyEvent, Identifier));
        return true;
    }

    protected bool IsHostOrServer()
    {
        if (NetworkManager.Singleton == null) return false;
        
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) return true;
        
        LethalNetworkAPIPlugin.Logger.LogError(string.Format(
            TextDefinitions.NotServerInfo, NetworkManager.Singleton.LocalClientId, Identifier));
        return false;
    }

    private bool DoClientsExist(IEnumerable<ulong> clientIds)
    {
        if (clientIds.Any()) return true;
        
        LethalNetworkAPIPlugin.Logger.LogError(string.Format(
            TextDefinitions.TargetClientsNotConnected, clientIds, Identifier));
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

        if (!DoClientsExist(allowedClientIds)) return new ClientRpcParams();
        
        return new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIdsNativeArray = allowedClientIds
            }
        };
    }
    
    protected ClientRpcParams GenerateClientParams(IEnumerable<ulong> clientIds) => GenerateClientParams(clientIds, false);
    protected ClientRpcParams GenerateClientParams(ulong clientId) => GenerateClientParams([clientId], false);
    
    protected ClientRpcParams GenerateClientParamsExcept(IEnumerable<ulong> clientIds) => GenerateClientParams(clientIds, true);
    protected ClientRpcParams GenerateClientParamsExcept(ulong clientId) => GenerateClientParams([clientId], true);
    
    protected ClientRpcParams GenerateClientParamsExceptHost() => GenerateClientParams([], true);

    #endregion

    internal readonly string Identifier;
}