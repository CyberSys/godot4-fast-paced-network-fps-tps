using Godot;
using LiteNetLib.Utils;
using System.Runtime.Serialization;

namespace Framework.Network
{
    public enum PlayerConnectionState
    {
        Unknown,
        Connected,
        Initialized,
        Disconnected
    }
}
