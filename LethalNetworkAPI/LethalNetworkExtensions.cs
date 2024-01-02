using GameNetcodeStuff;

namespace LethalNetworkAPI;

public static class LethalNetworkExtensions
{
    public static PlayerControllerB? GetPlayerFromId(ulong clientId)
    {
        return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientId]] ?? null;
    }
}