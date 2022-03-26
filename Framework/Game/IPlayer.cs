using Framework.Network;

namespace Framework.Game
{
    /// <summary>
    /// Required interface for players
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// Name of player
        /// </summary>
        /// <value></value>
        public string Name { get; set; }

        /// <summary>
        /// Current latency (ping)
        /// </summary>
        /// <value></value>
        public int Latency { get; set; }

        /// <summary>
        /// Current connection state
        /// </summary>
        /// <value></value>
        public PlayerConnectionState State { get; set; }

        /// <summary>
        /// Assigned team of player
        /// </summary>
        /// <value></value>
        public PlayerTeam Team { get; set; }
    }
}
