using GameNetcodeStuff;
using LethalNetworkAPI.Variable;

namespace LethalNetworkAPI;

/// <summary>
/// Additional tools to help with networking.
/// </summary>
internal static class LethalNetworkExtensions
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
    /// <param name="originalComponent">The <see cref="NetworkBehaviour"/> to attach the variable to.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="LethalNetworkVariable{TData}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client.</remarks>
    public static LethalNetworkVariable<TData>? NetworkVariable<TData>(this NetworkBehaviour originalComponent, string identifier)
    {
        return originalComponent.gameObject.NetworkVariable<TData>(identifier);
    }
  
    /// <summary>
    /// Get a NetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to attach the variable to. Only networked objects are permitted.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="LethalNetworkVariable{TData}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client.</remarks>
    public static LethalNetworkVariable<TData>? NetworkVariable<TData>(this GameObject gameObject, string identifier)
    {
        if (gameObject.TryGetComponent(out NetworkObject networkObjectComp) == false)
        {
            Plugin.Logger.LogError(TextDefinitions.UnableToLocateNetworkObjectComponent);
            return null;
        }

        var networkVariable = (LethalNetworkVariable<TData>)
            NetworkHandler.Instance!.ObjectNetworkVariableList.First(i =>
                ((LethalNetworkVariable<TData>)i).VariableIdentifier == $"{identifier}.{networkObjectComp.GlobalObjectIdHash}");

        if (networkVariable != null)
            return networkVariable;

        networkVariable = new LethalNetworkVariable<TData>($"{identifier}.{networkObjectComp.GlobalObjectIdHash}", networkObjectComp);
        NetworkHandler.Instance!.ObjectNetworkVariableList.Add(networkVariable);
        
        return networkVariable;
    }
}