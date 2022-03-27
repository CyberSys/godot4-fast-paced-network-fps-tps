using System.Collections.Generic;
using Framework.Network;
namespace Framework.Game
{
    /// <summary>
    /// Required interface for levels
    /// </summary>
    public interface ILevel
    {
        /// <summary>
        /// List of avaible spawn points
        /// </summary>
        /// <returns></returns>
        public SpawnPoint[] GetAllSpawnPoints();

        public SpawnPoint[] GetFreeSpawnPoints();
    }
}
