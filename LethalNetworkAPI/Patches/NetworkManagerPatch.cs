namespace LethalNetworkAPI.Patches;

using HarmonyLib;
using Internal;
using Old.Networking;
using Unity.Netcode;
using Utils;

[HarmonyPatch(typeof(NetworkManager))]
[HarmonyPriority(Priority.HigherThanNormal)]
[HarmonyWrapSafe]
internal static class NetworkManagerPatch
{
    [HarmonyPatch(nameof(NetworkManager.Initialize))]
    [HarmonyPostfix]
    public static void InitializePatch(NetworkManager __instance)
    {
        _ = new UnnamedMessageHandler();
        _ = new NetworkHandler();

        __instance.OnClientConnectedCallback += LNetworkUtils.InvokeOnNetworkStart;
    }

    [HarmonyPatch(nameof(NetworkManager.ShutdownInternal))]
    [HarmonyPrefix]
    public static void ShutdownPatch(NetworkManager __instance)
    {
        __instance.OnClientConnectedCallback -= LNetworkUtils.InvokeOnNetworkStart;

        UnnamedMessageHandler.Instance?.Dispose();
        NetworkHandler.Instance?.Dispose();
    }
}
