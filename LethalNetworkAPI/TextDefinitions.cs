namespace LethalNetworkAPI;

internal class TextDefinitions
{
    private const string NotInLobbyMessage = 
        "Unable to send the message with id \"{0}\" and data {{{1}}}. Is the player in a lobby?";
    
    private const string NotInLobbyEvent = 
        "Unable to invoke the event with id {0}. Is the player in a lobby?";

    private const string NetworkHandlerDoesNotExist = 
        "The NetworkHandler does not exist. This shouldn't occur!";
}