using Godot;
using Framework.Input;
using Framework.Network;
using Framework;

namespace Shooter.Shared.Components
{
    public partial class PlayerFootstepsComponent : Node, IChildComponent
    {
        public string GetComponentName()
        {
            return "footsteps";
        }
        public IBaseComponent BaseComponent { get; set; }

        public override void _Process(float delta)
        {

        }
    }
}
