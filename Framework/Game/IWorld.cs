using System.Collections.Generic;
using Framework.Network;
namespace Framework.Game
{
    /// <summary>
    /// The required interface for an game world
    /// </summary>
    public interface IWorld
    {
        /// <summary>
        /// Init the server world (first time)
        /// </summary>
        /// <param name="serverVars">List of avaible server vars</param>
        /// <param name="initalWorldTick">Staring game time</param>
        public void Init(Dictionary<string, string> serverVars, uint initalWorldTick);

        /// <summary>
        /// Destroy the game world
        /// </summary>
        public void Destroy();

        /// <summary>
        /// Add an physical player simulation
        /// </summary>
        /// <param name="id">Id of player</param>
        /// <typeparam name="T">Type of player simulation</typeparam>
        /// <returns></returns>
        public T AddPlayerSimulation<T>(int id) where T : NetworkPlayerSimulation;

        /// <summary>
        /// Remove player simulation
        /// </summary>
        /// <param name="simulation">Given player simulation</param>
        public void RemovePlayerSimulation(NetworkPlayerSimulation simulation);

        /// <summary>
        /// The current tick of the world
        /// </summary>
        /// <value></value>
        public uint WorldTick { get; }

        /// <summary>
        /// The loaded game level of the world
        /// </summary>
        /// <value></value>
        public ILevel Level { get; }

        /// <summary>
        /// The server vars of the world
        /// </summary>
        /// <value></value>
        public Dictionary<string, string> ServerVars { get; }

        /// <summary>
        /// All players of the world
        /// </summary>
        /// <value></value>
        public Dictionary<int, IPlayer> Players { get; }

    }
}
