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
    public static PlayerControllerB? GetPlayerController(this ulong clientId)
    {
        return StartOfRound.Instance.allPlayerScripts[StartOfRound.Instance.ClientPlayerList[clientId]];
    }

    [Obsolete("GetPlayerFromId is deprecated, please use GetPlayerController instead.")]
    public static PlayerControllerB? GetPlayerFromId(this ulong clientId) => clientId.GetPlayerController();
    
    /// <summary>
    /// Gets the <see cref="UInt64">ulong</see> from a given <see cref="PlayerControllerB"/>.
    /// </summary>
    /// <param name="player">(<see cref="PlayerControllerB"/>) The player controller. </param>
    /// <returns>(<see cref="UInt64">ulong</see>) The player's client id.</returns>
    public static ulong GetClientId(this PlayerControllerB player)
    {
        return player.actualClientId;
    }

    /// <summary>
    /// Get a NetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="networkBehaviour">The <see cref="NetworkBehaviour"/> to attach the variable to.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <param name="serverOwned">Opt. (<see cref="bool"/>) Set to true if only the server should be able to write to it.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="LethalNetworkVariable{TData}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client. In order to sync on all clients, the host must also run this method on the same GameObject with the same identifier.</remarks>
    public static LethalNetworkVariable<TData>? GetNetworkVariable<TData>(this NetworkBehaviour networkBehaviour, string identifier, bool serverOwned = false) => networkBehaviour.gameObject.NetworkVariable<TData>(identifier, serverOwned);

    /// <summary>
    /// Get a NetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="networkObject">The <see cref="NetworkObject"/> to attach the variable to.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <param name="serverOwned">Opt. (<see cref="bool"/>) Set to true if only the server should be able to write to it.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="LethalNetworkVariable{TData}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client. In order to sync on all clients, the host must also run this method on the same GameObject with the same identifier.</remarks>
    public static LethalNetworkVariable<TData>? GetNetworkVariable<TData>(this NetworkObject networkObject, string identifier, bool serverOwned = false) => networkObject.gameObject.NetworkVariable<TData>(identifier, serverOwned);

    /// <summary>
    /// Get a NetworkVariable with the identifier specific to the NetworkObject. If one doesn't exist, it creates a new one on all clients.
    /// </summary>
    /// <param name="gameObject">The <see cref="GameObject"/> to attach the variable to. Only networked objects are permitted.</param>
    /// <param name="identifier">(<see cref="string"/>) An identifier for the variable. Specific to the network object.</param>
    /// <param name="serverOwned">Opt. (<see cref="bool"/>) Set to true if only the server should be able to write to it.</param>
    /// <typeparam name="TData">The <a href="https://docs.unity3d.com/2022.3/Documentation/Manual/script-Serialization.html#SerializationRules">serializable data type</a> of the message.</typeparam>
    /// <returns>(<see cref="LethalNetworkVariable{TData}"/>) The network variable.</returns>
    /// <remarks>The variable is set to only allow writing by the object's owner client. In order to sync on all clients, the host must also run this method on the same GameObject with the same identifier.</remarks>
    public static LethalNetworkVariable<TData>? GetNetworkVariable<TData>(this GameObject gameObject, string identifier, bool serverOwned = false) => gameObject.NetworkVariable<TData>(identifier, serverOwned);

    private static LethalNetworkVariable<TData>? NetworkVariable<TData>(this GameObject gameObject, string identifier, bool serverOwned)
    {
        if (gameObject.TryGetComponent(out NetworkObject networkObjectComp) == false)
        {
            LethalNetworkAPIPlugin.Logger.LogError(string.Format(TextDefinitions.UnableToLocateNetworkObjectComponent, identifier));
            return null;
        }

        var networkVariable = (LethalNetworkVariable<TData>)
            NetworkHandler.Instance!.ObjectNetworkVariableList.FirstOrDefault(i =>
                ((LethalNetworkVariable<TData>)i).Identifier == $"{identifier}.{networkObjectComp.GlobalObjectIdHash}")!;

        if (networkVariable != null!)
            return networkVariable;

        networkVariable = new LethalNetworkVariable<TData>($"{identifier}.{networkObjectComp.GlobalObjectIdHash}", networkObjectComp, serverOwned, 3);
        NetworkHandler.Instance.ObjectNetworkVariableList.Add(networkVariable);
        
        return networkVariable;
    }
}