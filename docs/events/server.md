---
prev: false
next: true
description: How to use LethalNetworkAPI's Server Events.
---

# Server Events

Events are a custom network message that send no information over the network. Use-cases of this class are things like invoking of custom effects, or any other temporary and in-the-moment actions.

Server Events are a sub-class of events that can only be used on the server/host.

:::warning
Any clients attempting to run actions/methods from the server event class will result in nothing happening.

Any clients attempting to invoke events from the server events class will result in errors.
:::

## Constructor

The method of constructing an event instance/object is simple; all that needs to be specified for events is the identifier.

```csharp
LethalServerEvent customServerEvent = new LethalServerEvent(identifier: "customIdentifier");
```

:::tip NOTE
This identifier will be shared between server and client events of the mod.

Any other mods with the same identifier or any messages or variables of the same identifier will not interact with your event.
:::

## Methods

There are three available methods to use on your Server Event.

### Invoke All Clients

The most commonly used method is `.InvokeAllClients(bool receiveOnHost = true)`. This will invoke all Client Events (including the host by default) with the same identifier. To use, simply do the following:

- **(Optional)** `receiveOnHost` defines whether the event should be received on the host client as well.

```csharp
customServerEvent.InvokeAllClients();
```

:::tip
It is highly recommended to leave `receiveOnHost` as true and instead program your event/method to be received on *all* clients instead of every client but the host.
:::

### Invoke a Specific Client

A much less commonly used, but still important, method is `.InvokeClient(ulong clientId)`. This will invoke the Client Event of the specified client. For example, to invoke the Client Event for a client from their `PlayerControllerB`, one can do the following:

```csharp
customServerEvent.InvokeClient(playerController.actualClientId);
```

### Invoke Specific Clients

The least commonly used method is to invoke specific clients: `.InvokeClients(IEnumerable<ulong> clientIds)`. This will invoke the Client Events of the specified clients.

```csharp
customServerEvent.InvokeClients(new List<ulong> { 0, 1 })
```

:::tip
Because the parameter is an `IEnumerable<ulong>`, you can send any list, collection, or array as a parameter.
:::

## Event/Delegate

There is one event to subscribe to in order to receive events from clients. `OnReceived` will have one parameter passed along, `ulong originatorClientId`. This is the client Id of the client who invoked the server via a Client Event of the same identifier.

```csharp
customServerEvent.OnReceived += ReceiveFromClient;

private void ReceiveFromClient(ulong clientId) 
{
    //...
}
```

:::tip
You can get the `PlayerControllerB` from a client Id by using the [`GetPlayer` extension method](/extensions#get-player-from-id).
:::