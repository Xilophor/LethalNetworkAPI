using System.Collections.Generic;

namespace LethalNetworkAPI;

public abstract class LethalNetwork
{
    protected LethalNetwork(string identifier)
    {
        Identifier = $"{Assembly.GetCallingAssembly().GetName().Name}.{identifier}";
        
#if DEBUG
        Plugin.Logger.LogDebug($"NetworkEvent with identifier \"{Identifier}\" has been created.");
#endif
    }

    #region Error Checks
    
    internal bool IsNetworkHandlerNull()
    {
        if (NetworkHandler.Instance != null) return false;
        
        Plugin.Logger.LogError(string.Format(
            TextDefinitions.NotInLobbyEvent, Identifier));
        return true;
    }

    internal bool IsHostOrServer()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer) return false;
        
        Plugin.Logger.LogError(string.Format(
            TextDefinitions.NotServerInfo, NetworkManager.Singleton.LocalClientId, Identifier));
        return true;
    }

    internal bool DoClientsExist(IEnumerable<ulong> clientIds)
    {
        if (clientIds.Any()) return true;
        
        Plugin.Logger.LogError(string.Format(
            TextDefinitions.TargetClientsNotConnected, clientIds, Identifier));
        return false;
    }

    internal bool DoesClientExist(ulong clientId) => DoClientsExist([clientId]);

    #endregion

    internal readonly string Identifier;
}