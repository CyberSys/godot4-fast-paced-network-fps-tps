using Godot;
using Framework.Game;
using Framework;
using Framework.Network.Commands;
using LiteNetLib;
using Framework.Network.Services;
using System;

namespace Framework.Game.Client
{
    public abstract class ClientLogic<T> : GameLogic where T : ClientWorld
    {
        [Export]
        public int DefaultNetworkPort = 27015;

        [Export]
        public string DefaultNetworkHostname = "localhost";

        private ClientNetworkService netService = null;


        private string loadedWorldName = null;


        protected override void OnMapInstance(PackedScene res, uint worldTick)
        {
            T newWorld = Activator.CreateInstance<T>();
            newWorld.Name = "world";

            this.AddChild(newWorld);

            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;

            //send server map loading was completed
            this.netService.SendMessageSerialisable<ServerInitializer>(0, new ServerInitializer());
            this.AfterMapInstance();
        }

        protected override void OnMapDestroy()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;
            this.loadedWorldName = null;

            this.AfterMapDestroy();
        }

        public void Disconnect()
        {
            this.OnMapDestroy();
            this.netService.Disconnect();
        }


        public void DoConnect()
        {
            if (this.CurrentWorld == null)
            {
                this.netService.Connect(new ClientConnectionSettings
                {
                    Port = DefaultNetworkPort,
                    Hostname = DefaultNetworkHostname,
                    SecureKey = this.secureConnectionKey
                });
            }
        }

        public virtual void OnConnect()
        {

        }

        public override void _EnterTree()
        {
            this.netService = this.Services.Create<ClientNetworkService>();
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

            this.AfterInit();
        }

        protected void onDisconnect(DisconnectReason reason, bool fullDisconnect)
        {
            if (fullDisconnect)
            {
                Logger.LogDebug(this, "Full disconnected");
                this.OnMapDestroy();
            }
        }
    }
}
