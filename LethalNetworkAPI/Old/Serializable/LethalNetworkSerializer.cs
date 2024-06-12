using System;
using Unity.Netcode;
using UnityEngine;
#if NETSTANDARD2_1
using OdinSerializer;
#endif

namespace LethalNetworkAPI.Serializable;

internal static class LethalNetworkSerializer
{
    internal static byte[] Serialize<T>(T value)
    {
#if NETSTANDARD2_1
        if (value is null) return [];
        
        return value switch
        {
            GameObject gameObject => SerializationUtility.SerializeValue((NetworkObjectReference)gameObject, DataFormat.Binary),
            NetworkObject networkObject => SerializationUtility.SerializeValue((NetworkObjectReference)networkObject, DataFormat.Binary),
            NetworkBehaviour gameObject => SerializationUtility.SerializeValue((NetworkBehaviourReference)gameObject, DataFormat.Binary),
            _ => SerializationUtility.SerializeValue(value, DataFormat.Binary)
        };
#else
        throw new Exception("The incorrect version of LethalNetworkAPI is installed. Use the netstandard2.1 version provided by the Thunderstore listing or the GitHub release. The currently installed version will *not* work as intended.");  
#endif
    }
    
    internal static T Deserialize<T>(byte[] data)
    {
#if NETSTANDARD2_1
        if (data.Length == 0) return default!;
        
        return default(T) switch
        {
            GameObject => (T)(object)(GameObject)SerializationUtility.DeserializeValue<NetworkObjectReference>(data, DataFormat.Binary),
            NetworkObject => (T)(object)(NetworkObject)SerializationUtility.DeserializeValue<NetworkObjectReference>(data, DataFormat.Binary),
            NetworkBehaviour => (T)(object)(NetworkBehaviour)SerializationUtility.DeserializeValue<NetworkBehaviourReference>(data, DataFormat.Binary),
            _ => SerializationUtility.DeserializeValue<T>(data, DataFormat.Binary)
        };
#else
        throw new Exception("The incorrect version of LethalNetworkAPI is installed. Use the netstandard2.1 version provided by the Thunderstore listing or the GitHub release. The currently installed version will *not* work as intended.");     
#endif
    }
}