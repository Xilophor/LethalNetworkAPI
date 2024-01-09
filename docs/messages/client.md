---
prev: true
next: false
description: How to use LethalNetworkAPI's Client Messages.
---

# Client Messages

Messages send information over the network. Use-cases of Network Messages can be sending a specific sound id to play, or any other need send data over for a temporary or in-the-moment action.

Client Messages can only be used by any clients/host.

## Constructor

Two things need to be specified for constructing messages, the identifier and the `type` of the data transmitted.

```csharp
LethalClientMessage customClientMessage = new LethalClientMessage<TData>(identifier: "customIdentifier");
```

`TData` is the type of messages, which can be any serializable type. Examples of serializable types are:

- `string`
- `int`
- `Vector3`
- `Color`

You can also add `[Serializable]` before a class to mark it as serializable. For more information, please visit [Unity's Serialization docs](https://docs.unity3d.com/Manual/script-Serialization.html).

:::tip NOTE
This identifier will be shared between server and client message of the mod.

Any other mods with the same identifier or any events or variables of the same identifier will not interact with your message.
:::

## Methods

There are two available methods to use on your Client Message.

:::tip INFO
Anytime `TData` is used in this doc, it refers to the type specified in the message [constructor](#constructor).
:::

### Send data to Server

The most commonly used method is `.SendServer(TData data)`. This will invoke the Server Message with the same identifier. To use, simply do the following:

```csharp
customClientMessage.SendServer(data);
```

### Send data to All Clients

This method is not recommended but in place to help simplify code. `.SendAllClients(TData data, bool includeLocalClient = true, bool waitForServerResponse = false)` invokes the `OnReceivedFromClient` event on all Client Messages with the same identifier.

- **(Optional)** `includeLocalClient` defines whether the local client message should be invoked.
- **(Optional)** `waitForServerResponse` defines - *if the local client message is invoked* - whether the message should fire after receiving a response from the server or not (include `Client`⇒`Server`⇒`Client` latency).

```csharp
customClientMessage.SendAllClients(data);
```

:::warning
This method bypasses any Server Messages, and thus any checks on the server you may want.
:::

## Events/Delegates

There are two event to subscribe to in order to receive messages from the server or clients. 

`OnReceived` will have one parameter passed along, `TData data`, and will invoke upon an event from the server.

```csharp
customClientMessage.OnReceived += ReceiveFromServer;

private void ReceiveFromServer(TData data)
{
    //...
}
```

`OnReceivedFromClient` will have two parameters passed along, `TData data` and `ulong originatorClientId`. This is the client Id of the client who invoked any `SendAllClients` method of the Client Message class.

```csharp
customClientMessage.OnReceivedFromClient += ReceiveFromClient;

private void ReceiveFromClient(TData data, ulong clientId)
{
    //...
}
```

:::tip
You can get the `PlayerControllerB` from a client Id by using the [`GetPlayer` extension method](/extensions#get-player-from-id).
:::