using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shooter.Shared;
using Shooter.Client;

namespace Shooter.Client.UI.Welcome
{
    public partial class PreConnectComponent : CanvasLayer, IComponent<ClientGameLogic>
    {
        public ClientGameLogic MainComponent { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
        }

        private void onConnectButtonPressed()
        {
            if (this.MainComponent.CurrentWorld == null)
            {
                this.MainComponent.Connect();
            }
        }
    }
}
