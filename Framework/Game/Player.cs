using Framework.Network;
using Framework.Game;
using System.Collections.Generic;

namespace Framework.Game
{
    /// <summary>
    /// The general player class
    /// </summary>
    public abstract class Player : Godot.Node, IPlayer
    {
        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public string PlayerName { get; set; }

        /// <inheritdoc />
        public int Latency { get; set; }

        /// <inheritdoc />
        public PlayerConnectionState State { get; set; }

        /// <inheritdoc />
        public PlayerTeam Team { get; set; }

        /// <inheritdoc />
        public IWorld GameWorld { get; private set; }

        /// <inheritdoc />
        private readonly ComponentRegistry _components;

        /// <inheritdoc />
        public ComponentRegistry Components => _components;

        /// <summary>
        /// Previous state since last tick
        /// </summary>
        /// <value></value>
        public PlayerConnectionState PreviousState { get; set; }

        /// <summary>
        /// Base player class
        /// </summary>
        /// <param name="id">Server remote id</param>
        /// <param name="world">World instance</param>
        /// <returns></returns>
        public Player(int id, IWorld world) : base()
        {
            this.Id = id;
            this.GameWorld = world;
            this._components = new ComponentRegistry(this);
        }

        /// <summary>
        /// Base player class
        /// </summary>
        /// <returns></returns>
        public Player() : base()
        {

        }
        /// <inheritdoc />
        public virtual void Tick(float delta)
        {
        }

        /// <inheritdoc />
        private readonly Dictionary<string, RegisteredComonent> avaiableComponents = new Dictionary<string, RegisteredComonent>();

        /// <inheritdoc />
        public Dictionary<string, RegisteredComonent> AvaiablePlayerComponents => avaiableComponents;

        /// <inheritdoc />
        public string[] RequiredComponents { get; set; } = new string[0];
    }
}