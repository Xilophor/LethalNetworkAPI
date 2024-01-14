namespace LethalNetworkAPI;

internal static class TextDefinitions
{
    internal const string NotInLobbyMessage = 
        "Unable to send the message with identifier \"{0}\" and data {{{1}}}. Is the player in a lobby?";
    
    internal const string NotServerInfo = 
        "The client {0} cannot use server methods. Identifier: \"{1}\"";
    
    internal const string NotInLobbyEvent = 
        "Unable to invoke the event with identifier \"{0}\". Is the player in a lobby?";

    internal const string NetworkHandlerDoesNotExist = 
        "The NetworkHandler does not exist. This shouldn't occur!";
    
    internal const string TargetClientNotConnected = 
        "The specified client {0} is not connected. Identifier: \"{1}\"";
    
    internal const string TargetClientsNotConnected = 
        "None of the specified clients {0} are connected. Identifier: \"{1}\"";
    
    internal const string UnableToLocateNetworkObjectComponent = 
        "Unable to find the network object component. Are you adding variable to a network object?";
}