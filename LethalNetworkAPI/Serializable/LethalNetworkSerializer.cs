
#if NETSTANDARD2_1
using OdinSerializer;
#endif

namespace LethalNetworkAPI.Serializable;

internal static class LethalNetworkSerializer
{
    internal static byte[] Serialize<T>(T value)
    {
#if NETSTANDARD2_1
        return SerializationUtility.SerializeValue(value, DataFormat.Binary);
#else
        return [];
#endif
    }
    
    internal static T Deserialize<T>(byte[] data)
    {
#if NETSTANDARD2_1
        return SerializationUtility.DeserializeValue<T>(data, DataFormat.Binary);
#else
        T test = default!;
        return test;
#endif
    }
}