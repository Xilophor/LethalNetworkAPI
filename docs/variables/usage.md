---
prev: false
next: true
description: How to use LethalNetworkAPI's Network Variables.
---

# Network Variable Usage

Network Variables sync data between the server and all clients. They can be modified by the server or any client, [unless given ownership/protection](/variables/ownership).

They automatically sync when joining the server, and thus are great for consistent information.

## Constructor

To make a new network variable, there are two pieces of information required: the `identifier` and the `type` of the variable.

```csharp
LethalNetworkVariable customVariable = new LethalNetworkVariable<TData>(identifier: "customIdentifier");
```

`TData` is the type of messages, which can be any serializable type. Examples of serializable types are:

- `string`
- `int`
- `Vector3`
- `Color`

You can also add `[Serializable]` before a class to mark it as serializable. For more information, please visit [Unity's Serialization docs](https://docs.unity3d.com/Manual/script-Serialization.html).

:::tip NOTE
This identifier will be shared between all instances of network variables in the mod.

Any other mods with the same identifier or any messages or events of the same identifier will not interact with your variable.
:::

If you want to set a value to the network variable while initializing it, you can use object initializers:

```csharp
LethalNetworkVariable customString = new LethalNetworkVariable<string>("customString") { Value = "Hello, World!" };
```

## Getting/Setting the Value

Because the variable is a class and not a type, to get or set the value, you must use `.Value`.

The following code prints the value of a `LethalNetworkVariable` of type `string`:

```csharp
Plugin.Logger.LogDebug(customString.Value);
```

To set the value, it's as simple as setting a normal variable (additionally with `.Value`);

```csharp
customString.Value = "Hasta la vista, baby!";
```

## Event/Delegate

There is a `OnValueChanged` event that runs when the value is changed. The following cases are when it is invoked:

- When setting the variable
- When joining a lobby/game<sup>[1](#fn-1)</sup>
- When creating a variable during playtime (in the lobby)<sup>[1](#fn-1)</sup>
- When a variable is updated over the network
  - This only happens every Network Tick; the following is taken (and adjusted) from [Unity's NGO docs](https://docs-multiplayer.unity3d.com/netcode/1.5.2/learn/rpcvnetvar/#choosing-between-networkvariables-or-rpcs).

![NetworkVariable Network Tick Explaination](/public/variables/usage/networktick.png)

The `OnValueChanged` event is invoked with the parameter `TData data`, where `TData` is the type specified in the [constructor](#constructor).

```csharp
customString.OnValueChanged += NewValue;

private void NewValue(string newValue)
{
    //...
}
```

---

<b id="fn-1" style="color: var(--vp-c-brand-1);">1</b>: These will only happen if the network variable of the same identifier exists on the server/host (if the variable is initialized on the server/host).