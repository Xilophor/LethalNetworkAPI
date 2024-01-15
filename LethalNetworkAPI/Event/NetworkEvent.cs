namespace LethalNetworkAPI;

public abstract class NetworkEvent(string identifier) : LethalNetwork($"evt.{identifier}");