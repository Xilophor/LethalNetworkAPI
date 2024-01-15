using System.Collections.Generic;
using Unity.Collections;

namespace LethalNetworkAPI.Utils;

internal static class ClientRpcParamsGenerator
{
    internal static ClientRpcParams? Generate(IEnumerable<ulong> clientIds, string identifier)
    {
        NativeArray<ulong> allowedClientIds;

        var enumerable = clientIds as ulong[] ?? clientIds.ToArray();
        
        if (!enumerable.Any())
            allowedClientIds = new NativeArray<ulong>(NetworkManager.Singleton.ConnectedClientsIds
                .Where(i => i != NetworkManager.ServerClientId).ToArray(), Allocator.Persistent);
        else
            allowedClientIds = new NativeArray<ulong>(enumerable
                .Where(i => NetworkManager.Singleton.ConnectedClientsIds.Contains(i)).ToArray(), Allocator.Persistent);
       
        if (!allowedClientIds.Any())
        {
            Plugin.Logger.LogError(string.Format(
                TextDefinitions.TargetClientsNotConnected, clientIds, identifier));
            return null;
        }
        
        return new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIdsNativeArray = allowedClientIds } };
    }
    
    internal static ClientRpcParams? Generate(ulong clientId, string identifier) => Generate([clientId], identifier);

    internal static ClientRpcParams? GenerateWithoutHost(string identifier) => Generate([], identifier);
}