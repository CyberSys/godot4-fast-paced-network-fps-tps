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
            this.AfterMapLoaded();
        }

        /// <inheritdoc />        
        internal override void DestroyMapInternal()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;
            this.loadedWorldName = null;

            this.AfterMapDestroy();
        }

        /// <summary>
        /// Disconnect the client
        /// </summary>
        public void Disconnect()
        {
            this.DestroyMapInternal();
            this.netService.Disconnect();
        }

        /// <summary>
        /// Connect with an server
        /// </summary>
        /// <param name="hostname"></param>
        /// <param name="port"></param>
        public void Connect(string hostname, int port)
        {
            if (this.CurrentWorld == null)
            {
                this.netService.Connect(new ClientConnectionSettings
                {
                    Port = port,
                    Hostname = hostname,
                    SecureKey = this.secureConnectionKey
                });
            }
        }

        /// <summary>
        /// On local client is connected
        /// </summary>
        public virtual void OnConnected()
        {

        }

        /// <summary>
        /// On local client are disconneted
        /// </summary>
        public virtual void OnDisconnect()
        {

        }

        /// <inheritdoc />  
        internal override void InternalTreeEntered()
        {
            this.netService = this.Services.Create<ClientNetworkService>();
            this.netService.OnDisconnect += this.OnInternalDisconnect;
            this.netService.Connected += this.OnConnected;

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
        private void OnInternalDisconnect(DisconnectReason reason, bool fullDisconnect)
        {
            if (fullDisconnect)
            {
                Logger.LogDebug(this, "Full disconnected");
                this.DestroyMapInternal();
                this.OnDisconnect();
            }
        }
    }
}
