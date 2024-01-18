namespace LethalNetworkAPI;

/// <summary>
/// Internal Class
/// </summary>
public abstract class NetworkMessage : LethalNetwork
{
    protected NetworkMessage(string identifier) : base($"msg.{identifier}")
    {
        
    }
}