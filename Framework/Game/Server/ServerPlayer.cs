using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Physics;
using System.Collections.Generic;
namespace Framework.Game.Server
{
    /// <summary>
    /// Core class for server player
    /// </summary>
    public class ServerPlayer : PhysicsPlayer
    {
        /// <inheritdoc />
        public ServerPlayer(int id, IWorld world) : base(id, world)
        {
        }

        /// <summary>
        /// Time since last disconnect
        /// </summary>
        /// <value></value>
        public float DisconnectTime { get; set; } = 0;

        /// <summary>
        /// Get the last tick of the last input
        /// </summary>
        /// <value></value>
        public uint LatestInputTick { get; set; } = 0;

        /// <summary>
        /// Return if the player is syncronized with server.
        /// </summary>
        /// <value></value>
        public bool IsSynchronized { get; set; } = false;


        /// <summary>
        /// Archived player states
        /// </summary>
        public PlayerState[] States = new PlayerState[1024];


        /// <summary>
        /// The active tick based input
        /// </summary>
        /// <value></value>
        // Current input struct for each player.
        // This is only needed because the ProcessAttack delegate flow is a bit too complicated.
        // TODO: Simplify this.
        public TickInput CurrentPlayerInput { get; set; }

        /// <inheritdoc />
        public string[] RequiredPuppetComponents { get; set; } = new string[0];


    }
}
