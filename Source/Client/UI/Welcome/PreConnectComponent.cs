using Godot;
using Framework;

namespace Shooter.Client.UI.Welcome
{
    public partial class PreConnectComponent : CanvasLayer, IChildComponent
    {
        public IBaseComponent BaseComponent { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
        }

        private void onConnectButtonPressed()
        {
            var component = BaseComponent as MyClientLogic;
            component.DoConnect();
        }
    }
}
