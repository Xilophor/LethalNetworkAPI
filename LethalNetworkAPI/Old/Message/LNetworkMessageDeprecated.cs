namespace LethalNetworkAPI;

using System;
using Old;

[Obsolete("Deprecated Internal Class.")]
public abstract class LNetworkMessageDeprecated(string identifier) : LethalNetworkDeprecated($"msg.{identifier}", "Message");
