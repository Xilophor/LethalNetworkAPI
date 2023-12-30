# LethalNetworkAPI

Currently in preview.

## Usage

### Constructor

There are currently two classes/objects you can make, `LethalNetworkMessage` and `LethalNetworkEvent`.

To create a `LethalNetworkMessage`, you do as so:

```csharp
customMessage = new LethalNetworkMessage<T>("customGuid");
```

`T` can be any of the currently supported types: https://www.newtonsoft.com/json/help/html/SerializationGuide.htm

To create a `LethalNetworkEvent`, you do as so:

```csharp
customEvent = new LethalNetworkEvent("customGuid");
```

> The `guid` is specific to the mod and class. Other mods can have the same `guid`, and an `Event` and a `Message` can have the same `guid`.

### Send MessagesEvents

There are four ways to send a message or an event:

```csharp
customMessage.SendServer(<T> data);

customMessage.SendClient(<T> data, ulong clientId);

customMessage.SendClients(<T> data, List<ulong> clientIds); // Any form of list, array, collection, or IEnumerable is acceptable

customMessage.SendAllClients(<T> data, (optional) bool recieveOnHost = true) // Only set recieveOnHost to false if absolutely necessary; doing so can create large amounts of lag on large servers.
```

> Events are used the same, except without the `data` parameter.

### Receive Messages/Events

There are two receive delegates/events for Messages and Events:

```csharp
customMessage.OnServerReceived += ReceiveMessage;
customMessage.OnClientReceived += ReceiveMessage;

void ReceiveMessage(<T> data) {}

customEvent.OnServerReceived += ReceiveEvent;
customEvent.OnClientReceived += ReceiveEvent;

void ReceiveEvent() {}
```

## Acknowledgements

This API uses [@EvaisaDev](https://github.com/EvaisaDev/)'s [Unity Netcode Weaver/Patcher](https://github.com/EvaisaDev/UnityNetcodeWeaver/). Without it, this API would not be possible as it is currently done.