using Godot;

namespace Framework.Game
{
    /// <summary>
    /// A basic class for an game level
    /// </summary>
    public abstract class Level : Node3D, ILevel
    {
        /// <inheritdoc />
        public SpawnPoint GetFreeSpawnPoint()
        {
            foreach (var point in GetChildren())
            {
                if (point is SpawnPoint)
                {
                    var sp = point as SpawnPoint;
                    if (sp.inUsage == false)
                        return sp;
                }
            }

            return null;
        }
    }
}
