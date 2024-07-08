namespace LethalNetworkAPI.Internal;

#if NETSTANDARD2_1

using OdinSerializer;

internal record MessageData(
    [property: OdinSerialize] string Identifier,
    [property: OdinSerialize] EMessageType MessageType,
    [property: OdinSerialize] object? Data = null
);

#else

internal record MessageData(
    string Identifier,
    EMessageType MessageType,
    object? Data = null
);

#endif
