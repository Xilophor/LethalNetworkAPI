namespace LethalNetworkAPI.Utils;

using System.Diagnostics;
using System.Linq;
using BepInEx;
using HarmonyLib;
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
    public static bool IsConnected => NetworkManager.Singleton?.IsConnectedClient ?? false;

    /// <summary>
    /// Whether the client is the host or server.
    /// </summary>
    public static bool IsHostOrServer => IsConnected && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer);

    /// <summary>
    /// All connected clients' GUIDs.
    /// </summary>
    /// <remarks>This will be empty if not connected to a server.</remarks>
    public static ulong[] AllConnectedClients
    {
        get
        {
            if (NetworkManager.Singleton == null) return [];
            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
                return NetworkManager.Singleton.ConnectedClientsIds.ToArray();
            return _allClients;
        }
        internal set => _allClients = value;
    }

    private static ulong[] _allClients = [];

    /// <summary>
    /// All connected clients' GUIDs, except this client.
    /// </summary>
    /// <remarks>This will be empty if not connected to a server.</remarks>
    public static ulong[] OtherConnectedClients => AllConnectedClientsExcept(NetworkManager.Singleton.LocalClientId);

    /// <summary>
    /// All connected clients' GUIDs, except the specified client.
    /// </summary>
    /// <param name="clientId">The client to exclude.</param>
    /// <remarks>This will be empty if not connected to a server.</remarks>
    public static ulong[] AllConnectedClientsExcept(ulong clientId) => AllConnectedClients.Where(i => i != clientId).ToArray();

    /// <summary>
    /// All connected clients' GUIDs, except the specified client.
    /// </summary>
    /// <param name="clientIds">The clients to exclude.</param>
    /// <remarks>This will be empty if not connected to a server.</remarks>
    public static ulong[] AllConnectedClientsExcept(params ulong[] clientIds) => AllConnectedClients.Where(i => !clientIds.Contains(i)).ToArray();

    internal static string GetModGuid(int frameIndex)
    {
        var method = new StackTrace().GetFrame(frameIndex).GetMethod();
        var assembly = method.ReflectedType!.Assembly;
        var pluginType = AccessTools.GetTypesFromAssembly(assembly).First(type =>
            type.GetCustomAttributes(typeof(BepInPlugin), false).Any());

        return MetadataHelper.GetMetadata(pluginType).GUID;
    }
}
