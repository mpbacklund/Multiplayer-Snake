﻿
namespace Shared.Messages
{
    public enum Type : UInt16
    {
        ConnectAck,     // Server to client
        NewEntity,      // Server to client
        UpdateEntity,   // Server to client
        RemoveEntity,   // Server to client
        GameSize,       // Server to client
        Turn,           // Server to client
        Collision,      // Server to client
        Join,           // Client to server
        Input,          // Client to server
        Disconnect      // Client to server
    }
}
