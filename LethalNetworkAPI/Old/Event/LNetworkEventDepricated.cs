namespace LethalNetworkAPI;

using System;
using Old;

[Obsolete("Deprecated Internal Class")]
public abstract class LNetworkEventDepricated(string identifier) : LethalNetworkDeprecated($"evt.{identifier}", "Event");
