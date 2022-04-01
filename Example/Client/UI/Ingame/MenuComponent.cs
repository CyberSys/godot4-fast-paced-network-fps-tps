
using Godot;
using Framework;

namespace Shooter.Client.UI.Welcome
{
    public partial class MenuComponent : CanvasLayer, IChildComponent
    {

        public IBaseComponent BaseComponent { get; set; }

        [Export]
        public NodePath DisconnectPath { get; set; }

        public override void _EnterTree()
        {
            this.GetNode<Button>(this.DisconnectPath).Pressed += () =>
            {
                var component = BaseComponent as MyClientLogic;
                component.Disconnect();
            };
        }

    }
}
