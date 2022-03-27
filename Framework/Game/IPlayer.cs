using Framework.Network;
using System.Collections.Generic;

namespace Framework.Game
{
    /// <summary>
    /// Required interface for players
    /// </summary>
    public interface IPlayer : IBaseComponent
    {
        /// <summary>
        /// Id of player
        /// </summary>
        /// <value></value>
        public int Id { get; set; }

        /// <summary>
        /// Name of player
        /// </summary>
        /// <value></value>
        public string PlayerName { get; set; }

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

        /// <summary>
        /// Execute each server tick
        /// </summary>
        /// <param name="delta"></param>
        public void Tick(float delta);

        /// <summary>
        /// Contains possible components
        /// </summary>
        /// <value></value>
        public Dictionary<string, RegisteredComonent> AvaiablePlayerComponents { get; }

        /// <summary>
        /// List of required components for this player instance
        /// </summary>
        /// <value></value>
        public string[] RequiredComponents { get; set; }
    }
}
