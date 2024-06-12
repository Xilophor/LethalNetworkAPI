using LethalNetworkAPI.Serializable;
using Unity.Netcode;

#if NETSTANDARD2_1
using OdinSerializer;

[assembly: RegisterFormatter(typeof(NetworkBehaviourReferenceFormatter))]

namespace LethalNetworkAPI.Serializable;


/// <summary>
/// Custom formatter for the <see cref="NetworkBehaviour"/> type.
/// </summary>
public class NetworkBehaviourReferenceFormatter : MinimalBaseFormatter<NetworkBehaviourReference>
{
    private static readonly Serializer<ushort> UInt16Serializer = Serializer.Get<ushort>();
    private static readonly Serializer<NetworkObjectReference> NetworkObjectReferenceSerializer = Serializer.Get<NetworkObjectReference>();
    
    protected override void Read(ref NetworkBehaviourReference value, IDataReader reader)
    {
        value.m_NetworkObjectReference = NetworkObjectReferenceSerializer.ReadValue(reader);
        value.m_NetworkBehaviourId = UInt16Serializer.ReadValue(reader);
    }

    protected override void Write(ref NetworkBehaviourReference value, IDataWriter writer)
    {
        NetworkObjectReferenceSerializer.WriteValue(value.m_NetworkObjectReference, writer);
        UInt16Serializer.WriteValue(value.m_NetworkBehaviourId, writer);
    }
}

#endif