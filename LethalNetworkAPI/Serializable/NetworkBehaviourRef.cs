using LethalNetworkAPI.Serializable;
using OdinSerializer;

[assembly: RegisterFormatter(typeof(NetworkBehaviourFormatter))]

namespace LethalNetworkAPI.Serializable;
internal class NetworkBehaviourFormatter : MinimalBaseFormatter<NetworkBehaviour>
{
    private static readonly Serializer<ulong> UInt64Serializer = Serializer.Get<ulong>();
    
    protected override void Read(ref NetworkBehaviour value, IDataReader reader)
    {
        value.NetworkObjectId = UInt64Serializer.ReadValue(reader);
    }

    protected override void Write(ref NetworkBehaviour value, IDataWriter writer)
    {
        UInt64Serializer.WriteValue(value.NetworkObjectId, writer);
    }
}