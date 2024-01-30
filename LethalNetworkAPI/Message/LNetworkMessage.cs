namespace LethalNetworkAPI;

/// <summary>
/// Internal class.
/// </summary>
public abstract class LNetworkMessage(string identifier) : LethalNetwork($"msg.{identifier}", "Message");