using LethalNetworkAPI.Serializable;

#if NETSTANDARD2_1
using OdinSerializer;

[assembly: RegisterFormatter(typeof(NetworkObjectReferenceFormatter))]

namespace LethalNetworkAPI.Serializable;

/// <summary>
/// Custom formatter for the <see cref="NetworkObject"/> type.
/// </summary>
internal class NetworkObjectReferenceFormatter : MinimalBaseFormatter<NetworkObjectReference>
{
    private static readonly Serializer<ulong> UInt64Serializer = Serializer.Get<ulong>();
    
    protected override void Read(ref NetworkObjectReference value, IDataReader reader)
    {
        value.NetworkObjectId = UInt64Serializer.ReadValue(reader);
    }

    protected override void Write(ref NetworkObjectReference value, IDataWriter writer)
    {
        UInt64Serializer.WriteValue(value.NetworkObjectId, writer);
    }
}

#endif