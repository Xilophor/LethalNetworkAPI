using LethalNetworkAPI.Serializable;

#if NETSTANDARD2_1
using OdinSerializer;

[assembly: RegisterFormatter(typeof(NetworkObjectFormatter))]
[assembly: RegisterFormatter(typeof(GameObjectFormatter))]

namespace LethalNetworkAPI.Serializable;

/// <summary>
/// Custom formatter for the <see cref="NetworkObject"/> type.
/// </summary>
public class NetworkObjectFormatter : MinimalBaseFormatter<NetworkObject>
{
    private static readonly Serializer<NetworkObjectReference> NetworkObjectReferenceSerializer = Serializer.Get<NetworkObjectReference>();
    
    protected override void Read(ref NetworkObject value, IDataReader reader)
    {
        value = NetworkObjectReferenceSerializer.ReadValue(reader);
    }

    protected override void Write(ref NetworkObject value, IDataWriter writer)
    {
        NetworkObjectReferenceSerializer.WriteValue(value, writer);
    }
}

/// <summary>
/// Custom formatter for the <see cref="GameObject"/> type.
/// </summary>
public class GameObjectFormatter : MinimalBaseFormatter<GameObject>
{
    private static readonly Serializer<NetworkObjectReference> NetworkObjectReferenceSerializer = Serializer.Get<NetworkObjectReference>();
    
    protected override void Read(ref GameObject value, IDataReader reader)
    {
        value = NetworkObjectReferenceSerializer.ReadValue(reader);
    }

    protected override void Write(ref GameObject value, IDataWriter writer)
    {
        NetworkObjectReferenceSerializer.WriteValue(value, writer);
    }
}

#endif