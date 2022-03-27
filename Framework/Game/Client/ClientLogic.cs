using Godot;
using Framework.Game;
using Framework;
using Framework.Network.Commands;
using LiteNetLib;
using Framework.Network.Services;
using System;

namespace Framework.Game.Client
{
    /// <summary>
    /// Is the base class for any client (SubViewport)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ClientLogic<T> : GameLogic where T : ClientWorld
    {

        /// <inheritdoc />
        private ClientNetworkService netService = null;

        /// <inheritdoc />
        private string loadedWorldName = null;

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

        /// <inheritdoc />
        internal override void OnMapInstanceInternal(PackedScene res, uint worldTick)
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

        /// <inheritdoc />        
        internal override void DestroyMapInternal()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;
            this.loadedWorldName = null;

            this.AfterMapDestroy();
        }

        public void Disconnect()
        {
            this.DestroyMapInternal();
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

        /// <inheritdoc />  
        internal override void InternalTreeEntered()
        {
            this.netService = this.Services.Create<ClientNetworkService>();
            this.netService.OnDisconnect += this.onDisconnect;

            this.netService.SubscribeSerialisable<ClientWorldInitializer>((package, peer) =>
            {
                if (this.loadedWorldName != package.WorldName)
                {
                    this.loadedWorldName = package.WorldName;
                    this.LoadWorldInternal(package.WorldName, package.WorldTick);
                }
            });


            base.InternalTreeEntered();
        }

        /// <inheritdoc />  
        internal void onDisconnect(DisconnectReason reason, bool fullDisconnect)
        {
            if (fullDisconnect)
            {
                Logger.LogDebug(this, "Full disconnected");
                this.DestroyMapInternal();
            }
        }
    }
}
