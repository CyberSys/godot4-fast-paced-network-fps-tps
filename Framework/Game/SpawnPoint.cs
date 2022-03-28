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
        public bool inUsage { get; set; } = false;

        private int enteredBodies = 0;

        /// <summary>
        /// Returns if the spawn point are free and not in use
        /// </summary>
        /// <returns></returns>
        public bool isFree()
        {
            return (enteredBodies == 0);
        }

        /// <inheritdoc />
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
