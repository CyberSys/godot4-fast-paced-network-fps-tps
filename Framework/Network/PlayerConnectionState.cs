using Godot;
using LiteNetLib.Utils;
using System.Runtime.Serialization;

namespace Framework.Network
{
    public enum PlayerConnectionState
    {
        Unknown = 0,
        Connected = 1,
        Initialized = 2,
        Disconnected = 3
    }
}
