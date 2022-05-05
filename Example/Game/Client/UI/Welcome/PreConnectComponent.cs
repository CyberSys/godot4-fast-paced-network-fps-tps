using Godot;
using Framework;
using Framework.Game;

namespace Shooter.Client.UI.Welcome
{
    public partial class PreConnectComponent : CanvasLayer, IChildComponent<GameLogic>
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


        public GameLogic BaseComponent { get; set; }

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
