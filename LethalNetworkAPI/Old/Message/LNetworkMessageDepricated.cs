namespace LethalNetworkAPI;

/// <summary>
/// Internal class.
/// </summary>
public abstract class LNetworkMessageDepricated(string identifier) : LethalNetwork($"msg.{identifier}", "Message");