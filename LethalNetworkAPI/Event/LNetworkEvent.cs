namespace LethalNetworkAPI;

/// <summary>
/// Internal class.
/// </summary>
public abstract class LNetworkEvent(string identifier) : LethalNetwork($"evt.{identifier}", "Event");