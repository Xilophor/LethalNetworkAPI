namespace LethalNetworkAPI.Internal;

using System;

[Flags]
internal enum EMessageType
{
    None                    = 0b0000_0000, // 0
    Event                   = 0b0000_0001, // 1
    Message                 = 0b0000_0010, // 2
    Variable                = 0b0000_0100, // 4
    [Obsolete] SyncedEvent  = 0b0000_1000, // 8

    ServerMessage           = 0b0001_0000, // 16
    ClientMessage           = 0b0010_0000, // 32
    ClientMessageToClient   = 0b0100_0000,  // 64
    [Obsolete] Request      = 0b1000_0000,  // 128

    DataUpdate              = 0b0001_0000, // 16
    OwnershipUpdate         = 0b0010_0000, // 32
    ForceUpdate             = 0b0100_0000, // 64
    UpdateClientList        = 0b1000_0000, // 128
}
