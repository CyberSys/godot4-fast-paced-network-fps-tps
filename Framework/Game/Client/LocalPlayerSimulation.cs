using Godot;
using Framework.Physics;
using Framework.Input;

namespace Framework.Game.Client
{
    public partial class LocalPlayerSimulation : PhysicsPlayerSimulation
    {
        public IInputable Inputable { get; set; }

        public LocalPlayerSimulation() : base()
        {

        }
    }
}
