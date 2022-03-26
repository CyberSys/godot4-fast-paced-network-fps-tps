using Framework.Network;

namespace Framework.Game
{
    /// <summary>
    /// The general player class
    /// </summary>
    public abstract class Player<T> : IPlayer where T : NetworkPlayerSimulation
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public int Latency { get; set; }

        /// <inheritdoc />
        public PlayerConnectionState State { get; set; }

        /// <inheritdoc />
        public PlayerTeam Team { get; set; }

        /// <inheritdoc />
        public T Simulation { get; set; }

        /// <summary>
        /// Previous state since last tick
        /// </summary>
        /// <value></value>
        public PlayerConnectionState PreviousState { get; set; }
    }
}
