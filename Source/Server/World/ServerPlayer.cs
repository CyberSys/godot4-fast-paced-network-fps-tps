using Godot;
using Shooter.Shared;

namespace Shooter.Server.World
{
    public class ServerPlayer : Player
    {
        public float DisconnectTime = 0;
        public uint latestInputTick = 0;
    }
}
