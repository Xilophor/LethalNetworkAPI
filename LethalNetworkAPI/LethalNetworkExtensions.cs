using GameNetcodeStuff;

namespace LethalNetworkAPI;

/// <summary>
/// Additional tools to help with networking.
/// </summary>
public static class LethalNetworkExtensions
{
    /// <summary>
    /// Gets the <see cref="PlayerControllerB"/> from a given clientId.
    /// </summary>
    /// <param name="clientId">(<see cref="UInt64">ulong</see>) The client id. </param>
    /// <returns>(<see cref="PlayerControllerB">PlayerControllerB?</see>) The player controller component.</returns>
    /// <remarks>Will return <c>null</c> if the controller is not found.</remarks>
    public static PlayerControllerB? GetPlayerFromId(this ulong clientId)
    {
        return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientId]];
    }

    /// <summary>
    /// Get a NetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="LethalNetworkVariable{TData}"/>) The network variable.</returns>
    // ReSharper disable once InvalidXmlDocComment
    public static LethalNetworkVariable<TData>? NetworkVariable<TData>(this NetworkBehaviour originalComponent, string identifier)
    {
        if (originalComponent.TryGetComponent(out NetworkObject networkObjectComp) == false)
        {
            Plugin.Logger.LogError(TextDefinitions.UnableToLocateNetworkObjectComponent);
            return null;
        }

        var networkVariable = (LethalNetworkVariable<TData>)
            NetworkHandler.Instance!.ObjectNetworkVariableList.First(i =>
            ((LethalNetworkVariable<TData>)i).VariableIdentifier == $"{identifier}.{networkObjectComp.GlobalObjectIdHash}");

        if (networkVariable != null)
            return networkVariable;
        
        networkVariable = new LethalNetworkVariable<TData>($"{identifier}.{networkObjectComp.GlobalObjectIdHash}");
        NetworkHandler.Instance!.ObjectNetworkVariableList.Add(networkVariable);
        
        return networkVariable;
    }
}