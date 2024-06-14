namespace LethalNetworkAPI;

using System;

[Obsolete("Deprecated Internal Class.")]
public abstract class LNetworkMessageDepricated(string identifier) : LethalNetwork($"msg.{identifier}", "Message");
