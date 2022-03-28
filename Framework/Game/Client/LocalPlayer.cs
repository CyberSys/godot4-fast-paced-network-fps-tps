using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;

namespace Framework.Game.Client
{
    public class LocalPlayer : PhysicsPlayer
    {
        public LocalPlayer(int id, IWorld world) : base(id, world)
        {
        }
        public PlayerState incomingLocalPlayerState = new PlayerState();
        public IInputable Inputable { get; set; }

        public IPlayerInput[] localPlayerInputsSnapshots = new IPlayerInput[1024];
        public PlayerState[] localPlayerStateSnapshots = new PlayerState[1024];
        public uint[] localPlayerWorldTickSnapshots = new uint[1024];
    }
}
