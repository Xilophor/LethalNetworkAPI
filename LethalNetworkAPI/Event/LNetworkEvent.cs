namespace LethalNetworkAPI;

/// <summary>
/// Internal Class
/// </summary>
public abstract class LNetworkEvent(string identifier) : LethalNetwork($"evt.{identifier}");