namespace LethalNetworkAPI;

/// <summary>
/// Internal Class
/// </summary>
public abstract class NetworkEvent : LethalNetwork
{
    protected NetworkEvent(string identifier) : base($"evt.{identifier}")
    {
        
    }
}