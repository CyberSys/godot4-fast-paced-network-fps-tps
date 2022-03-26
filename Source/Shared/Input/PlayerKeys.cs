
using Godot;
using Shooter.Shared;
using System;
using LiteNetLib.Utils;

namespace Shooter.Shared
{
    [Flags]
    public enum PlayerKeys : byte
    {
        None = 0,
        Forward = 1,
        Back = 2,
        Right = 4,
        Left = 8,
        Jump = 16,
        Fire = 32,
        Crouch = 64,
        Shifting = 128
    }
}