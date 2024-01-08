using GameNetcodeStuff;

namespace LethalNetworkAPI;

/// <summary>
/// Additional tools to help with networking.
/// </summary>
public static class LethalNetworkExtensions
{
    /// <summary>
    /// Gets the <see cref="PlayerControllerB"/> from a given clientId.
    /// </summary>
    /// <param name="clientId">(<see cref="UInt64">ulong</see>) The client id. </param>
    /// <returns>(<see cref="PlayerControllerB">PlayerControllerB?</see>) The player controller component.</returns>
    /// <remarks>Will return <c>null</c> if the controller is not found.</remarks>
    public static PlayerControllerB? GetPlayerFromId(this ulong clientId)
    {
        return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientId]];
    }
}