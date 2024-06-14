namespace LethalNetworkAPI;

internal static class TextDefinitions
{
    internal const string NotInLobbyMessage =
        "Unable to send the {0} with identifier \"{1}\" and data {{{2}}}. Is the player in a lobby?";

    internal const string UnableToFindGuid =
        "Unable to find plugin info for calling mod for {0} with identifier \"{1}\". Are you using BepInEx? \n Stacktrace: {2}";

    internal const string NotServerInfo =
        "The client {0} cannot use server methods. {1} Identifier: \"{2}\"";

    internal const string NotInLobbyEvent =
        "Unable to invoke the {0} with identifier \"{1}\". Is the player in a lobby?";

    internal const string NetworkHandlerDoesNotExist =
        "The NetworkHandler does not exist. This shouldn't occur!";

    internal const string TargetClientNotConnected =
        "The specified client {0} is not connected. {1} Identifier: \"{2}\"";

    internal const string TargetClientsNotConnected =
        "None of the specified clients {0} are connected. {1} Identifier: \"{2}\"";

    internal const string UnableToLocateNetworkObjectComponent =
        "Unable to find the network object component. Are you adding variable \"{0}\" to a network object?";
}
