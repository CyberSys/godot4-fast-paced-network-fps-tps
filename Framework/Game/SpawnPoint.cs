using Godot;

namespace Framework.Game
{
    /// <summary>
    /// The gernal spawn point class
    /// </summary>
    public abstract class SpawnPoint : Area3D
    {
        /// <summary>
        /// Set or get the usage of the spawnpoint
        /// </summary>
        public bool inUsage = false;

        private int enteredBodies = 0;

        public bool isFree()
        {
            return (enteredBodies == 0);
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.BodyEntered += (body) =>
            {
                if (body is RigidDynamicBody3D || body is CharacterBody3D)
                    enteredBodies++;
            };

            this.BodyExited += (body) =>
            {
                if (body is RigidDynamicBody3D || body is CharacterBody3D)
                    enteredBodies--;
            };
        }
    }
}
