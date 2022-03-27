using System.Linq;
using Godot;
using System;

namespace Framework.Game
{
    /// <summary>
    /// A basic class for an game level
    /// </summary>
    public abstract class Level : Node3D, ILevel
    {
        /// <inheritdoc />
        public SpawnPoint[] GetAllSpawnPoints()
        {
            return GetChildren().OfType<SpawnPoint>().ToArray<SpawnPoint>();
        }

        /// <inheritdoc />
        public SpawnPoint[] GetFreeSpawnPoints()
        {
            return GetAllSpawnPoints().Where(df => df.isFree()).ToArray();
        }
    }
}
