using OdinSerializer;

namespace LethalNetworkAPI.Serializable;

internal static class LethalNetworkSerializer
{
    internal static byte[] Serialize<T>(T value)
    {
        return value switch
        {
            GameObject gameObject => SerializationUtility.SerializeValue((NetworkObjectReference)gameObject, DataFormat.Binary),
            NetworkObject networkObject => SerializationUtility.SerializeValue((NetworkObjectReference)networkObject, DataFormat.Binary),
            NetworkBehaviour gameObject => SerializationUtility.SerializeValue((NetworkBehaviourReference)gameObject, DataFormat.Binary),
            _ => SerializationUtility.SerializeValue(value, DataFormat.Binary)
        };
    }
    
    internal static T Deserialize<T>(byte[] data)
    {
        T type = default!;
        return type switch
        {
            GameObject => (T)(object)(GameObject)SerializationUtility.DeserializeValue<NetworkObjectReference>(data, DataFormat.Binary),
            NetworkObject => (T)(object)(NetworkObject)SerializationUtility.DeserializeValue<NetworkObjectReference>(data, DataFormat.Binary),
            NetworkBehaviour => (T)(object)(NetworkBehaviour)SerializationUtility.DeserializeValue<NetworkBehaviourReference>(data, DataFormat.Binary),
            _ => SerializationUtility.DeserializeValue<T>(data, DataFormat.Binary)
        };
    }
}