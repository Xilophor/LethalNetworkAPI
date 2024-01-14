using LethalNetworkAPI.Serializable;
using OdinSerializer;

[assembly: RegisterFormatter(typeof(NetworkObjectFormatter))]

namespace LethalNetworkAPI.Serializable;
internal class NetworkObjectFormatter : MinimalBaseFormatter<NetworkObject>
{
    private static readonly Serializer<ulong> UInt64Serializer = Serializer.Get<ulong>();
    
    protected override void Read(ref NetworkObject value, IDataReader reader)
    {
        value.NetworkObjectId = UInt64Serializer.ReadValue(reader);
    }

    protected override void Write(ref NetworkObject value, IDataWriter writer)
    {
        UInt64Serializer.WriteValue(value.NetworkObjectId, writer);
    }
}