namespace LethalNetworkAPI.Patches;

using HarmonyLib;
using Internal;
using Old.Networking;
using Unity.Netcode;

[HarmonyPatch(typeof(NetworkManager))]
[HarmonyPriority(Priority.HigherThanNormal)]
[HarmonyWrapSafe]
internal static class NetworkManagerPatch
{
    [HarmonyPatch(nameof(NetworkManager.Initialize))]
    [HarmonyPostfix]
    public static void InitializePatch()
    {
        _ = new UnnamedMessageHandler();
        _ = new NetworkHandler();
    }

    [HarmonyPatch(nameof(NetworkManager.ShutdownInternal))]
    [HarmonyPrefix]
    public static void ShutdownPatch()
    {
        UnnamedMessageHandler.Instance?.Dispose();
        NetworkHandler.Instance?.Dispose();
    }
}
