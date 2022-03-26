using Godot;
using System.Collections.Generic;

namespace Shooter.Shared
{
    public partial class GameLevel : Node3D
    {
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
        public SpawnPoint FreeAllSpawnPoints()
        {
            foreach (var point in GetChildren())
            {
                if (point is SpawnPoint)
                {
                    var sp = point as SpawnPoint;
                    sp.inUsage = false;
                }
            }

            return null;
        }
    }
}
