namespace LethalNetworkAPI;

/// <summary>
/// Internal class.
/// </summary>
public abstract class LNetworkEventDepricated(string identifier) : LethalNetwork($"evt.{identifier}", "Event");