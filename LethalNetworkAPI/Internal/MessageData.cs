namespace LethalNetworkAPI.Internal;

internal record MessageData(
    string Identifier,
    EMessageType MessageType,
    object? Data = null
);
