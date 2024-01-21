namespace LethalNetworkAPI;

/// <summary>
/// Internal Class
/// </summary>
public abstract class LNetworkMessage(string identifier) : LethalNetwork($"msg.{identifier}");