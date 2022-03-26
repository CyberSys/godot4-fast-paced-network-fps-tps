using Godot;
using Shooter.Shared;
using Shooter.Shared.Network.Packages;

namespace Shooter.Server.World
{
    public class ServerPlayer : Player
    {
        public float DisconnectTime = 0;
        public uint latestInputTick = 0;
        public bool synchronized;
        public PlayerState[] states = new PlayerState[1024];
        public SpawnPoint SpawnPoint { get; set; }

        // Current input struct for each player.
        // This is only needed because the ProcessAttack delegate flow is a bit too complicated.
        // TODO: Simplify this.
        public TickInput currentPlayerInput { get; set; }
    }
}
