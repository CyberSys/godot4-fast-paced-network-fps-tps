using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;

namespace Framework.Game.Client
{
    /// <summary>
    /// The base class for local players
    /// </summary>
    public class LocalPlayer : PhysicsPlayer
    {
        /// <inheritdoc />
        public LocalPlayer(int id, IWorld world) : base(id, world)
        {
        }

        /// <summary>
        /// The last incoming local player state
        /// </summary>
        /// <returns></returns>
        public PlayerState incomingLocalPlayerState = new PlayerState();

        /// <summary>
        /// The component which handles the outgoing inputs
        /// </summary>
        /// <value></value>
        public IInputable Inputable { get; set; }

        /// <summary>
        /// The local player input snapshots
        /// </summary>
        public GeneralPlayerInput[] localPlayerInputsSnapshots = new GeneralPlayerInput[1024];

        /// <summary>
        /// The local player states
        /// </summary>
        public PlayerState[] localPlayerStateSnapshots = new PlayerState[1024];

        /// <summary>
        /// The last world player ticks related to the state snapshots
        /// </summary>
        public uint[] localPlayerWorldTickSnapshots = new uint[1024];
    }
}
