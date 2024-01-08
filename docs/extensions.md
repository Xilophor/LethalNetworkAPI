---
prev: false
next: false
description: Extension Methods for LethalNetworkAPI
---

# Lethal Network Extensions

These are a few extension methods to aid your programming with this API.

## Get Player Controller from Id {#getplayer}

The method `GetPlayer()` allows you to get a `PlayerContollerB` from a `ulong` id. This is an extension to `ulong`, so using it is as simple as shown:

```csharp
ulong clientId = 1;

PlayerControllerB player = clientId.GetPlayer(); //[!code highlight]
```

::: warning REMARKS
If no player is found from the id, the method will return `null`.
:::