---
prev: true
next: false
description: How to use LethalNetworkAPI's Client Events.
---

# Client Events

Events are a custom network message that send no information over the network. Use-cases of this class are things like invoking of custom effects, or any other temporary and in-the-moment actions.

Client Events are a sub-class of events that can only be used by any clients/host.

## Constructor

The method of constructing an event instance/object is simple; all that needs to be specified for events is the identifier.

```csharp
LethalClientEvent customClientEvent = new LethalClientEvent(identifier: "customIdentifier");
```

:::tip NOTE
This identifier will be shared between server and client events of the mod.

Any other mods with the same identifier or any messages or variables of the same identifier will not interact with your event.
:::

## Methods

There are three available methods to use on your Client Event.

### Invoke Server

The most commonly used method is `.InvokeServer()`. This will invoke the Server Event with the same identifier. To use, simply do the following:

```csharp
customClientEvent.InvokeServer();
```

### Invoke All Clients

This method is not recommended but in place to help simplify code. `.InvokeAllClients(bool includeLocalClient = true, bool waitForServerResponse = false)` invokes the `OnReceivedFromClient` event on all Client Events with the same identifier.

- **(Optional)** `includeLocalClient` defines whether the local client event should be invoked.
- **(Optional)** `waitForServerResponse` defines - *if the local client event is invoked* - whether the event should fire after receiving a response from the server or not (include `Client`⇒`Server`⇒`Client` latency).

```csharp
customClientEvent.InvokeAllClients();
```

:::warning
This method bypasses any Server Events, and thus any checks on the server you may want.
:::

### Synced Invoke of All Clients

This method attempts to invoke a synced event for all clients - where it is invoked at the same time, regardless of latency (within reason). `.InvokeAllClientsSynced()` invokes the `OnReceivedFromClient` event on all clients.

```csharp
customClientEvent.InvokeAllClientsSynced();
```

:::warning
This method bypasses any Server Events, and thus any checks on the server you may want.
:::

## Events/Delegates

There are two events to subscribe to in order to receive events from the Server or Clients. 

`OnReceived` will have *no* parameters passed along, and will invoke upon an event from the server.

```csharp
customClientEvent.OnReceived += ReceiveFromServer;

private void ReceiveFromServer()
{
    //...
}
```

`OnReceivedFromClient` will have one parameter passed along, `ulong originatorClientId`. This is the client Id of the client who invoked any `InvokeAllClients` method of the Client Event class.

```csharp
customClientEvent.OnReceivedFromClient += ReceiveFromClient;

private void ReceiveFromClient(ulong clientId)
{
    //...
}
```

:::tip
You can get the `PlayerControllerB` from a client Id by using the [`GetPlayer` extension method](/extensions#get-player-from-id).
:::