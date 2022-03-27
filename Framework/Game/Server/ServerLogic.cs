using Godot;
using Framework.Game;
using Framework;
using System.Collections.Generic;
using Framework.Network.Services;
using System;

namespace Framework.Game.Server
{
    public abstract class ServerLogic<T> : GameLogic where T : ServerWorld
    {
        private ServerNetworkService netService = null;

        [Export]
        /// <summary>
        /// maximal possible connections
        /// </summary>
        public int MaxConnections = 15;

        [Export]
        /// <summary>
        /// If clients can connect
        /// </summary>
        public bool AcceptClients = false;

        [Export]
        /// <summary>
        /// The dictonary with all server settings (vars);
        /// </summary>
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
        public int NetworkPort = 27015;

        public virtual void OnServerStarted()
        {

        }


        /// <summary>
        /// Instance the server game logic and load the map from default settings
        /// </summary>
        public override void _EnterTree()
        {
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


        public virtual bool OnPreConnect(LiteNetLib.ConnectionRequest request)
        {
            return true;
        }

        /// <summary>
        /// Loading the game world (async)
        /// </summary>
        /// <param name="mapName"></param>
        /// <param name="worldTick"></param>
        protected override void LoadWorld(string mapName, uint worldTick)
        {
            this.AcceptClients = false;

            //disconnect all clients
            foreach (var connectedClient in this.netService.GetConnectedPeers())
            {
                connectedClient.Disconnect();
            }

            base.LoadWorld(mapName, worldTick);
        }

        /// <summary>
        /// Instancing the game world
        /// </summary>
        /// <param name="res"></param>
        /// <param name="worldTick"></param>
        protected override void OnMapInstance(PackedScene res, uint worldTick)
        {
            T newWorld = Activator.CreateInstance<T>();
            newWorld.Name = "world";
            this.AddChild(newWorld);
            newWorld.worldPath = res.ResourcePath;

            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;
            this.currentWorld?.Init(this.ServerVars, 0);

            //accept clients
            this.AcceptClients = true;
            this.AfterMapInstance();
        }

        /// <summary>
        /// Destroy the game world
        /// </summary>
        protected override void OnMapDestroy()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;

            //reject clients
            this.AcceptClients = false;

            this.AfterMapDestroy();
        }
    }
}
