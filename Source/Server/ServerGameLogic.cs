using Godot;
using Shooter.Shared;
using Shooter.Server.World;
using System.Collections.Generic;

namespace Shooter.Server
{

    public partial class ServerGameLogic : CoreGameLogic
    {
        private Shooter.Server.Services.ServerNetworkService netService = null;

        [Export]
        /// <summary>
        /// The dictonary with all server settings (vars);
        /// </summary>
        public Dictionary<string, string> ServerVars = new Dictionary<string, string>  {
            // enable or disable interpolation
            { "sv_interpolate", "true" },
            // force for soft or hard lag reduction
            { "sv_agressive_lag_reduction", "true" },
            // maximal input stages per ms
            { "sv_max_stages_ms", "500" },
            // force freeze client in case of lags
            { "sv_freze_client", "false" }
        };

        [Export]
        /// <summary>
        /// The default map name which is loaded after startup
        /// </summary>
        public string DefaultMapName = "res://Assets/Maps/Test.tscn";

        /// <summary>
        /// Instance the server game logic and load the map from default settings
        /// </summary>
        public override void _EnterTree()
        {
            this.netService = this._serviceRegistry.Create<Shooter.Server.Services.ServerNetworkService>();
            this.netService.ConnectionEstablished += () =>
            {
                this.LoadWorld(DefaultMapName, 0);
            };

            base._EnterTree();
        }

        /// <summary>
        /// Loading the game world (async)
        /// </summary>
        /// <param name="mapName"></param>
        /// <param name="worldTick"></param>
        protected override void LoadWorld(string mapName, uint worldTick)
        {
            this.netService.acceptClients = false;

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
            var newWorld = new ServerGameWorld();
            newWorld.Name = "world";
            this.AddChild(newWorld);
            newWorld.worldPath = res.ResourcePath;

            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;
            this.currentWorld?.Init(this.ServerVars, 0);

            //accept clients
            this.netService.acceptClients = true;
        }

        /// <summary>
        /// Destroy the game world
        /// </summary>
        protected override void OnMapDestroy()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;

            //reject clients
            this.netService.acceptClients = false;
        }
    }
}
