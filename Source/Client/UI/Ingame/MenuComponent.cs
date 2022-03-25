using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shooter.Shared;
using Shooter.Client;
using Shooter.Client.Services;
namespace Shooter.Client.UI.Welcome
{
    public partial class MenuComponent : CanvasLayer, IComponent<ClientGameLogic>
    {
        public ClientGameLogic MainComponent { get; set; }

        [Export]
        public NodePath DisconnectPath { get; set; }


        public override void _EnterTree()
        {
            this.GetNode<Button>(this.DisconnectPath).Pressed += () =>
            {
                this.MainComponent.Disconnect();
            };
        }

    }
}
