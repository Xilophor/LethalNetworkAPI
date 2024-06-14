namespace LethalNetworkAPI;

using System;

[Obsolete("Deprecated Internal Class")]
public abstract class LNetworkEventDepricated(string identifier) : LethalNetwork($"evt.{identifier}", "Event");
