using Godot;
using Framework;

namespace Shooter.Client.UI.Welcome
{
    public partial class PreConnectComponent : CanvasLayer, IChildComponent
    {

        /// <summary>
        /// The default port of the server
        /// </summary>
        [Export]
        public int DefaultNetworkPort { get; set; } = 27015;

        /// <summary>
        /// The default hostname for the client
        /// </summary>
        [Export]
        public string DefaultNetworkHostname = "localhost";


        public IBaseComponent BaseComponent { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
        }

        private void onConnectButtonPressed()
        {
            var component = BaseComponent as MyClientLogic;
            component.Connect(this.DefaultNetworkHostname, this.DefaultNetworkPort);
        }
    }
}
