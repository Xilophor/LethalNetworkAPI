namespace LethalNetworkAPI.Message;

using System;
using Internal;

public class LNetworkMessage<TData> : IMessage
{
    public string Identifier { get; }

    public Action<TData>? OnReceived { get; set; }

    #region Constructor

    public LNetworkMessage(string identifier)
    {
        this.Identifier = identifier;
        NamedMessageHandler.Messages.Add(identifier, this);
    }

    #endregion
}

internal interface IMessage;
