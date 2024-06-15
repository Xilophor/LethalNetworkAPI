namespace LethalNetworkAPI;

using System;

[Obsolete("Deprecated Internal Class.")]
public abstract class LNetworkMessageDeprecated(string identifier) : LethalNetwork($"msg.{identifier}", "Message");
