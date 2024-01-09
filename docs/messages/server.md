---
prev: false
next: true
description: How to use LethalNetworkAPI's Server Messages.
---

# Server Messages

Messages send information over the network. Use-cases of Network Messages can be sending a specific sound id to play, or any other need send data over for a temporary or in-the-moment action.

Server Messages can only be used on the server/host.

:::warning
Any clients attempting to run actions/methods from the server message class will result in nothing happening.

Any clients attempting to send messages from the server message class will result in errors.
:::

## Constructor

Two things need to be specified for constructing messages, the identifier and the `type` of the data transmitted.

```csharp
LethalServerMessage customServerMessage = new LethalServerMessage<TData>(identifier: "customIdentifier");
```

`TData` is the type of messages, which can be any serializable type. Examples of serializable types are:

- `string`
- `int`
- `Vector3`
- `Color`

You can also add `[Serializable]` before a class to mark it as serializable. For more information, please visit [Unity's Serialization docs](https://docs.unity3d.com/Manual/script-Serialization.html).

:::tip NOTE
This identifier will be shared between server and client messages of the mod.

Any other mods with the same identifier or any events or variables of the same identifier will not interact with your message.
:::

## Methods

There are three available methods to use on your Server Message.

:::tip INFO
Anytime `TData` is used in this doc, it refers to the type specified in the message [constructor](#constructor).
:::

### Send to All Clients

The most commonly used method is `.SendAllClients(TData data, bool receiveOnHost = true)`. This will send data to all Client Messages (including the host by default) with the same identifier. To use, simply do the following:

- **(Optional)** `receiveOnHost` defines whether the message should be received on the host client as well.

```csharp
customServerMessage.SendAllClients(data);
```

:::tip
It is highly recommended to leave `receiveOnHost` as true and instead program your OnReceive event/method of Client Messages to be received on *all* clients instead of every client but the host.
:::

### Send to a Specific Client

A much less commonly used, but still important, method is `.SendClient(TData data, ulong clientId)`. This will send data to the Client Message of the specified client. For example, to send data to the Client Message for a client from their `PlayerControllerB`, one can do the following:

```csharp
customServerMessage.SendClient(data, playerController.actualClientId);
```

### Send to Specific Clients

The least commonly used method is to send data to specific clients: `.SendClients(TData data, IEnumerable<ulong> clientIds)`. This will send data to the Client Messages of the specified clients.

```csharp
customServerMessage.SendClients(data, new List<ulong> { 0, 1 })
```

:::tip
Because the parameter is an `IEnumerable<ulong>`, you can send any list, collection, or array as a parameter.
:::

## Event/Delegate

There is one event to subscribe to in order to receive messages from clients. `OnReceived` will have two parameters passed along, `TData data` and `ulong originatorClientId`. This is the client Id of the client who messaged the server via a Client Message of the same identifier.

```csharp
customServerMessage.OnReceived += ReceiveFromClient;

private void ReceiveFromClient(TData data, ulong clientId) 
{
    //...
}
```

:::tip
You can get the `PlayerControllerB` from a client Id by using the [`GetPlayer` extension method](/extensions#get-player-from-id).
:::