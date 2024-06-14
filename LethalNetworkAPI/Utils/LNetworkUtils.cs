namespace LethalNetworkAPI.Utils;

using System.Linq;
using Unity.Netcode;

public static class LNetworkUtils
{
    /// <summary>
    /// Get the client's GUID from the player ID.
    /// </summary>
    /// <param name="playerId">The in-game player ID.</param>
    /// <returns>The client's NGO GUID.</returns>
    public static ulong GetClientGuid(int playerId) =>
        StartOfRound.Instance.allPlayerScripts[playerId].actualClientId;

    /// <summary>
    /// Get the client's player ID from the client's GUID.
    /// </summary>
    /// <param name="clientGuid">The client's NGO GUID.</param>
    /// <returns>The client's in-game player ID.</returns>
    public static int GetPlayerId(ulong clientGuid) =>
        (int)StartOfRound.Instance.allPlayerScripts.First(player => player.actualClientId == clientGuid).playerClientId;

    /// <summary>
    /// Whether the client is connected to a server.
    /// </summary>
    public static bool IsConnected => NetworkManager.Singleton != null;

    /// <summary>
    /// Whether the client is the host or server.
    /// </summary>
    public static bool IsHostOrServer => IsConnected && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer);

    /// <summary>
    /// All connected clients' GUIDs.
    /// </summary>
    /// <remarks>This will be empty if not connected to a server.</remarks>
    public static ulong[] AllConnectedClients => NetworkManager.Singleton?.ConnectedClientsIds.ToArray() ?? [];

    /// <summary>
    /// All connected clients' GUIDs, except this client.
    /// </summary>
    /// <remarks>This will be empty if not connected to a server.</remarks>
    public static ulong[] OtherConnectedClients => NetworkManager.Singleton?.ConnectedClientsIds.Where(i => i != NetworkManager.Singleton.LocalClientId).ToArray() ?? [];

}
