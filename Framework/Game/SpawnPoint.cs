using Godot;

namespace Framework.Game
{
    /// <summary>
    /// The gernal spawn point class
    /// </summary>
    public abstract class SpawnPoint : Position3D
    {
        /// <summary>
        /// Set or get the usage of the spawnpoint
        /// </summary>
        public bool inUsage = false;
    }
}
