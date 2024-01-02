# LethalNetworkAPI

[Unity Netcode Patcher](https://github.com/EvaisaDev/UnityNetcodePatcher/) is **not** required to use this API for your mod.

## Usage

### Network Event

The simplest addition of the API. Used to trigger methods on the server or other clients.

#### Constructor

To create a new Network Event, all you need is the simple constructor with your event identifier:

```csharp
LethalNetworkEvent customEvent = new LethalNetworkEvent(identifier: "customIdentifier");
```

Any invoke of this event will be received by events with the same identifier on other clients.

#### Methods

There are quite a few methods available to invoke the event.

1. Invoke an event to the server:

```csharp
customEvent.InvokeServer();
```

2. Invoke an event to all clients:

```csharp
customEvent.InvokeAllClients(bool receiveOnHost = true);
```

3. Invoke an event to a specific client:

```csharp
customEvent.InvokeClient(ulong clientId);
```

4. Invoke an event to specific clients:

```csharp
customEvent.InvokeClients(ulong[] clientId);
```

5. Invoke a synchronized event to all clients:

```csharp
customEvent.InvokeAllClientsSynced(bool receiveOnHost = true);
```

6. Invoke a synchronized event to other clients:

```csharp
customEvent.InvokeOtherClientsSynced();
```

> Synchronized Events will attempt to invoke at the same time regardless of latency (within reason).

#### Events/Delegates

There are two delegates you can subscribe to for receiving events.

1. `OnServerReceived`

```csharp
customEvent.OnServerReceived += CustomMethod;

private void CustomMethod() {}
```

2. `OnServerReceivedFrom`

```csharp
customEvent.OnServerReceivedFrom += CustomMethod;

private void CustomMethod(ulong originClientId) {}
```

3. `OnClientReceived`

```csharp
customEvent.OnClientReceived += CustomMethod;

private void CustomMethod() {}
```

---

### Network Message

Another way of sending information to other clients is via `LethalNetworkMessage`. They can send serializable data (ie `int`, `string`, `Vector3`, etc.).

#### Constructor

When creating a Network Message, you have to specify the `Type` of the message.

```csharp
LethalNetworkMessage<Type> customMessage = new LethalNetworkMessage<Type>(identifier: "customIdentifier");
```

The type can be any of the aforementioned serializable data types.

#### Methods

There are more specific methods available to send messages.

1. Send a message to the server:

```csharp
customMessage.SendServer(Type data);
```

2. Send a message to all clients:

```csharp
customMessage.SendAllClients(Type data, bool receiveOnHost = true);
```

3. Send a message to a specific client:

```csharp
customMessage.SendClient(Type data, ulong clientId);
```

4. Send a message to specific clients:

```csharp
customMessage.SendClients(Type data, ulong[] clientId);
```

#### Events/Delegates

There are two delegates you can subscribe to for receiving messages.

1. `OnServerReceived`

```csharp
customMessage.OnServerReceived += CustomMethod;

private void CustomMethod(Type data) {}
```

2. `OnServerReceivedFrom`

```csharp
customMessage.OnServerReceivedFrom += CustomMethod;

private void CustomMethod(Type data, ulong originClientId) {}
```

3. `OnClientReceived`

```csharp
customMessage.OnClientReceived += CustomMethod;

private void CustomMethod(Type data) {}
```

---

### Network Variable

The final addition of the API, available in any serializable data type.

#### Constructor/Initializer

When creating a Network Variable, you have to specify the `Type` of the variable.

```csharp
LethalNetworkVariable<Type> customVariable = new LethalNetworkVariable<Type>(identifier: "customIdentifier");
```

If you want to initialize it with data, you can use an object initializer:

```csharp
LethalNetworkVariable<Type> customVariable = new LethalNetworkVariable<Type>(identifier: "customIdentifier") { Value = (Type)data; };
```

> If you want to make a protected variable (that other clients can't modify), you need to add the `[LethalNetworkProtected]` attribute as so:
> 
> ```csharp
> [LethalNetworkProtected]
> LethalNetworkVariable<Type> protectedCustomVariable = new LethalNetworkVariable<Type>(identifier: "customIdentifier");
> ```
> 
> Ownership of the variable cannot be assigned until after the lobby is created/joined.

#### Value

In order to get or change the value of the variable, you have to use `.Value`. For example, this method clamps a Network Variable of type `int`:

```csharp
private void ClampNetworkVariable()
{
    switch (customIntVariable.Value)
    {
        case > 5:
            customIntVariable.Value = 5;
            break;
        case < -5:
            customIntVarable.Value = -5;
            break;
    }
}
```

#### Methods

To set or change ownership of a protected variable, you can use `SetOwnership`:

```csharp
bool ownerChanged = protectedCustomVariable.SetOwnership(ulong newOwnerClientId);
```

Any client can set ownership of the network variable when it has not been assigned an owner yet. Once it has been assigned ownership, only the owner client or the server can change it's ownership.

#### Event/Delegate

There is only one available delegate to subscribe to:

```csharp
customMessage.OnValueChanged += CustomMethod;

private void CustomMethod(Type newValue) {}
```

This will be invoked any time the variable is changed locally, and anytime a change is received through the network. Networked changes will only occur once every Network Tick; if a value is changed multiple times in a tick, the delegate will only be invoked once on all other clients.

---

## Acknowledgements

This API uses [@EvaisaDev](https://github.com/EvaisaDev/)'s [Unity Netcode Patcher](https://github.com/EvaisaDev/UnityNetcodePatcher/). Without it, this API would not be possible as it is currently done.
