using System.Collections.Generic;
using Framework.Network;
using Framework.Extensions;
using Framework.Game.Server;

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
        public void Init(ServerVars serverVars, uint initalWorldTick);

        /// <summary>
        /// Destroy the game world
        /// </summary>
        public void Destroy();

        /// <summary>
        /// The physics and network related tick process method
        /// </summary>
        public void Tick(float tickInterval);

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
        public ServerVars ServerVars { get; }

        /// <summary>
        /// All players of the world
        /// </summary>
        /// <value></value>
        public Dictionary<int, IPlayer> Players { get; }


        /// <summary>
        /// Calls when an player is initialized and the map was loaded sucessfulll
        /// </summary>
        /// <param name="p"></param>
        public void OnPlayerInitilaized(IPlayer p);

        /// <summary>
        /// Path of the world resource 
        /// </summary>
        /// <value></value>
        public string ResourceWorldPath { get; }
    }
}
