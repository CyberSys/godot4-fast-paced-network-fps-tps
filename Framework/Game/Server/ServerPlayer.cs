using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;

namespace Framework.Game.Server
{
    public class ServerPlayer : PhysicsPlayer
    {
        public ServerPlayer(int id, IWorld world) : base(id, world)
        {
        }

        public float DisconnectTime = 0;
        public uint latestInputTick = 0;
        public bool synchronized;
        public PlayerState[] states = new PlayerState[1024];

        // Current input struct for each player.
        // This is only needed because the ProcessAttack delegate flow is a bit too complicated.
        // TODO: Simplify this.
        public TickInput currentPlayerInput { get; set; }

    }
}
