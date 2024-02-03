using LethalNetworkAPI.Serializable;

#if NETSTANDARD2_1
using OdinSerializer;

[assembly: RegisterFormatter(typeof(NetworkBehaviourFormatter))]

namespace LethalNetworkAPI.Serializable;


/// <summary>
/// Custom formatter for the <see cref="NetworkBehaviour"/> type.
/// </summary>
public class NetworkBehaviourFormatter : MinimalBaseFormatter<NetworkBehaviour>
{
    private static readonly Serializer<NetworkBehaviourReference> NetworkBehaviourReferenceSerializer = Serializer.Get<NetworkBehaviourReference>();
    
    protected override void Read(ref NetworkBehaviour value, IDataReader reader)
    {
        value = NetworkBehaviourReferenceSerializer.ReadValue(reader);
    }

    protected override void Write(ref NetworkBehaviour value, IDataWriter writer)
    {
        NetworkBehaviourReferenceSerializer.WriteValue(value, writer);
    }
}

#endif