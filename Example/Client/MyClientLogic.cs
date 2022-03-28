using System.ComponentModel;
using Framework.Game.Server;
using Framework.Game;
using Godot;
using Framework.Game.Client;
using Shooter.Client.UI.Welcome;

namespace Shooter.Client
{
    public partial class MyClientLogic : ClientLogic<MyClientWorld>
    {
        private bool showMenu = false;

        public override void _EnterTree()
        {
            this.Components.AddComponent<DebugMenuComponent>("res://Client/UI/Welcome/DebugMenuComponent.tscn");
            this.Components.AddComponent<PreConnectComponent>("res://Client/UI/Welcome/PreConnectComponent.tscn");

            Input.SetMouseMode(Input.MouseMode.Visible);
        }

        public override void AfterMapLoaded()
        {
            this.Components.DeleteComponent<MapLoadingComponent>();
            this.Components.DeleteComponent<PreConnectComponent>();
            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        public override void AfterMapDestroy()
        {
            this.Components.DeleteComponent<MenuComponent>();
            this.Components.DeleteComponent<MapLoadingComponent>();
            this.Components.AddComponent<PreConnectComponent>("res://Client/UI/Welcome/PreConnectComponent.tscn");
        }

        public override void OnDisconnect()
        {
            this.Components.DeleteComponent<PreConnectComponent>();
            this.Components.AddComponent<MapLoadingComponent>("res://Client/UI/Welcome/MapLoadingComponent.tscn");
        }

        public override void _Input(InputEvent @event)
        {
            base._Input(@event);

            if (@event.IsActionReleased("abort") && this.currentWorld != null)
            {
                if (showMenu == true)
                {
                    Input.SetMouseMode(Input.MouseMode.Captured);
                    this.Components.DeleteComponent<MenuComponent>();
                    showMenu = false;
                }
                else
                {
                    Input.SetMouseMode(Input.MouseMode.Visible);
                    this.Components.AddComponent<MenuComponent>("res://Client/UI/Ingame/MenuComponent.tscn");
                    showMenu = true;
                }
            }

            @event.Dispose();
        }

    }
}