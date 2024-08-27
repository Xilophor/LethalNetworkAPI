namespace LethalNetworkAPI;

using System;
using System.Runtime.CompilerServices;

internal static class LobbyCompatibilityDep
{
    private static bool? _enabled;
    public static bool Enabled => _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility");

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static void Initialize() =>
        LobbyCompatibility.Features.PluginHelper.RegisterPlugin(
            MyPluginInfo.PLUGIN_GUID,
            new Version(MyPluginInfo.PLUGIN_VERSION),
            LobbyCompatibility.Enums.CompatibilityLevel.ServerOnly,
            LobbyCompatibility.Enums.VersionStrictness.Minor);
}
