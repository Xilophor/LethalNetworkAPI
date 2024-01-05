namespace LethalNetworkAPI;

internal static class TextDefinitions
{
    internal const string NotInLobbyMessage = 
        "Unable to send the message with id \"{0}\" and data {{{1}}}. Is the player in a lobby?";
    
    internal const string NotServerInfo = 
        "The client {0} cannot use server methods. Message/Event identifier: \"{1}\"";
    
    internal const string NotInLobbyEvent = 
        "Unable to invoke the event with id \"{0}\". Is the player in a lobby?";

    internal const string NetworkHandlerDoesNotExist = 
        "The NetworkHandler does not exist. This shouldn't occur!";
}