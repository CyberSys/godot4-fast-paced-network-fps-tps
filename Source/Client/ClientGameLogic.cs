using System;
using System.Linq;
using Godot;
using Shooter.Shared;
using Shooter.Shared.Network.Packages;
using Shooter.Client.World;
using Shooter.Client.Simulation;
using System.Collections.Generic;
using LiteNetLib;
using Shooter.Client.UI.Welcome;
using Shooter.Client.Simulation.Components;
namespace Shooter.Client
{
    public partial class ClientGameLogic : CoreGameLogic
    {
        private Shooter.Client.Services.ClientNetworkService netService = null;

        private string loadedWorldName = null;
        public ComponentRegistry<ClientGameLogic> Components { get; set; }

        public ClientGameLogic() : base()
        {
            this.Components = new ComponentRegistry<ClientGameLogic>(this);
        }

        protected override void OnMapInstance(PackedScene res, uint worldTick)
        {
            var newWorld = new ClientGameWorld();
            newWorld.Name = "world";

            this.AddChild(newWorld);

            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;

            //send server map loading was completed
            this.netService.SendMessageSerialisable<ServerInitializer>(0, new ServerInitializer());
            this.Components.DeleteComponent<MapLoadingComponent>();
            this.Components.DeleteComponent<PreConnectComponent>();
            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        protected override void OnMapDestroy()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;
            this.loadedWorldName = null;

            this.Components.DeleteComponent<MenuComponent>();
            this.Components.DeleteComponent<MapLoadingComponent>();
            this.Components.AddComponent<PreConnectComponent>("res://Client/UI/Welcome/PreConnectComponent.tscn");
        }



        public void Disconnect()
        {
            this.OnMapDestroy();
            this.netService.Disconnect();
        }

        private bool showMenu = false;

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

        public void Connect()
        {
            this.Components.DeleteComponent<PreConnectComponent>();
            this.Components.AddComponent<MapLoadingComponent>("res://Client/UI/Welcome/MapLoadingComponent.tscn");
            this.netService.Connect("localhost");
        }

        public override void _EnterTree()
        {
            this.netService = this._serviceRegistry.Create<Shooter.Client.Services.ClientNetworkService>();
            this.netService.OnDisconnect += this.onDisconnect;

            this.netService.SubscribeSerialisable<ClientWorldInitializer>((package, peer) =>
            {
                if (this.loadedWorldName != package.WorldName)
                {
                    this.loadedWorldName = package.WorldName;
                    this.LoadWorld(package.WorldName, package.WorldTick);
                }
            });


            base._EnterTree();
            Input.SetMouseMode(Input.MouseMode.Visible);

            this.Components.AddComponent<DebugMenuComponent>("res://Client/UI/Welcome/DebugMenuComponent.tscn");
            this.Components.AddComponent<PreConnectComponent>("res://Client/UI/Welcome/PreConnectComponent.tscn");
        }

        protected void onDisconnect(DisconnectReason reason, bool fullDisconnect)
        {
            if (fullDisconnect)
            {
                Logger.LogDebug(this, "Full disconnected");
                this.OnMapDestroy();
                this.netService.MyId = -1;
            }
        }
    }
}
