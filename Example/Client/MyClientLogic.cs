using System.ComponentModel;
using Framework.Game.Server;
using Framework.Game;
using Godot;
using Framework.Game.Client;
using Shooter.Client.UI.Welcome;
using Shooter.Client.UI.Ingame;

namespace Shooter.Client
{
    public partial class MyClientLogic : ClientLogic
    {
        public bool showMenu = false;

        public override ClientWorld CreateWorld()
        {
            return new MyClientWorld();
        }

        public override void _EnterTree()
        {
            //  this.Components.AddComponent<DebugMenuComponent>("res://Client/UI/Welcome/DebugMenuComponent.tscn");
            this.Components.AddComponent<PreConnectComponent>("res://Client/UI/Welcome/PreConnectComponent.tscn");

            Input.SetMouseMode(Input.MouseMode.Visible);
        }

        public override void AfterMapLoaded()
        {
            this.Components.DeleteComponent<MapLoadingComponent>();
            this.Components.DeleteComponent<PreConnectComponent>();
            //  this.Components.AddComponent<HudComponent>("res://Client/UI/Ingame/HudComponent.tscn");
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