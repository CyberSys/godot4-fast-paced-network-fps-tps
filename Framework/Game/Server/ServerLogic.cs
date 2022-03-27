using Godot;
using Framework.Game;
using Framework;
using System.Collections.Generic;
using Framework.Network.Services;
using System;

namespace Framework.Game.Server
{
    /// <summary>
    /// The core server logic
    /// </summary>
    /// <typeparam name="T">The type of the server world</typeparam>
    public abstract class ServerLogic<T> : GameLogic where T : ServerWorld
    {
        /// <inheritdoc />
        internal ServerNetworkService netService = null;

        /// <summary>
        /// maximal possible connections
        /// </summary>
        [Export]
        public int MaxConnections { get; set; } = 15;

        /// <summary>
        /// If clients can connect
        /// </summary>
        [Export]
        public bool AcceptClients { get; set; } = false;

        /// <summary>
        /// The dictonary with all server settings (vars);
        /// </summary>
        [Export]
        private Dictionary<string, string> ServerVars = new Dictionary<string, string>  {
            // enable or disable interpolation
            { "sv_interpolate", "true" },
            // force for soft or hard lag reduction
            { "sv_agressive_lag_reduction", "true" },
            // maximal input stages per ms
            { "sv_max_stages_ms", "500" },
            // force freeze client in case of lags
            { "sv_freze_client", "false" }
        };

        /// <summary>
        /// Network server port
        /// </summary>
        [Export]
        public int NetworkPort { get; set; } = 27015;

        /// <summary>
        /// Called when after server started succeffull
        /// </summary>
        public virtual void OnServerStarted()
        {

        }

        /// <inheritdoc />
        internal override void InternalTreeEntered()
        {
            base.InternalTreeEntered();
            this.netService = this.Services.Create<ServerNetworkService>();
            this.netService.ConnectionEstablished += () =>
            {
                Logger.LogDebug(this, "Server was started.");
                this.OnServerStarted();
            };

            this.netService.ConnectionRequest += (request) =>
            {
                if (this.currentWorld == null)
                {
                    request.Reject();
                }
                else
                {
                    if (this.netService.GetConnectionCount() < MaxConnections && AcceptClients && this.OnPreConnect(request))
                    {
                        Logger.LogDebug(this, request.RemoteEndPoint.Address + ":" + request.RemoteEndPoint.Port + " established");
                        request.AcceptIfKey(this.secureConnectionKey);
                    }
                    else
                    {
                        Logger.LogDebug(this, request.RemoteEndPoint.Address + ":" + request.RemoteEndPoint.Port + " rejected");
                        request.Reject();
                    }
                }
            };

            base._EnterTree();
            this.netService.Bind(NetworkPort);
        }

        /// <summary>
        /// Called when client connected 
        /// </summary>
        /// <param name="request">boolean for rejected or accepted</param>
        /// <returns></returns>
        public virtual bool OnPreConnect(LiteNetLib.ConnectionRequest request)
        {
            return true;
        }

        /// <inheritdoc />  
        internal override void LoadWorldInternal(string mapName, uint worldTick)
        {
            this.AcceptClients = false;

            //disconnect all clients
            foreach (var connectedClient in this.netService.GetConnectedPeers())
            {
                connectedClient.Disconnect();
            }

            base.LoadWorldInternal(mapName, worldTick);
        }

        /// <inheritdoc />
        internal override void OnMapInstanceInternal(PackedScene res, uint worldTick)
        {
            T newWorld = Activator.CreateInstance<T>();
            newWorld.Name = "world";
            this.AddChild(newWorld);
            newWorld._resourceWorldPath = res.ResourcePath;
            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;
            this.currentWorld?.Init(this.ServerVars, 0);

            //accept clients
            this.AcceptClients = true;
            this.AfterMapInstance();
        }

        /// <inheritdoc />
        internal override void DestroyMapInternal()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;

            //reject clients
            this.AcceptClients = false;

            this.AfterMapDestroy();
        }
    }
}
