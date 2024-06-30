namespace LethalNetworkAPI.Internal;

internal record MessageData(
    string Identifier,
    EMessageType MessageType,
    object? Data = null
);

internal record DeprecatedMessageData(
    string Identifier,
    EMessageType MessageType,
    byte[] Data = null!
);

