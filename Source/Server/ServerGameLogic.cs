using System;
using System.Linq;
using Godot;
using Shooter.Shared;
using Shooter.Shared.Network.Packages;
using Shooter.Server.World;
using System.Collections.Generic;
using LiteNetLib;
using Shooter.Server.Simulation;
using Shooter.Client.Simulation.Components;

namespace Shooter.Server
{

    public partial class ServerGameLogic : CoreGameLogic
    {
        Shooter.Server.Services.ServerNetworkService netService = null;
        public const string DefaultMapName = "Test";

        private const float PlayerListUpdateSync = 0.5f;

        private float nextPLayerListUpdate = 0;

        private const float deleteTimeForPlayer = 10;

        protected readonly Dictionary<int, ServerPlayer> _players = new Dictionary<int, ServerPlayer>();
        public Dictionary<int, ServerPlayer> Players => _players;


        public override void LoadWorld(string mapName, uint worldTick)
        {
            this.netService.acceptClients = false;

            //disconnect all clients
            foreach (var connectedClient in this.netService.GetConnectedPeers())
            {
                connectedClient.Disconnect();
            }

            this._players.Clear();
            base.LoadWorld(mapName, worldTick);
        }

        protected override void OnMapInstance(PackedScene res, uint worldTick)
        {
            var newWorld = new ServerGameWorld();
            newWorld.Name = "world";
            this.AddChild(newWorld);

            newWorld.InstanceLevel(res);

            this.currentWorld = newWorld;
            this.currentWorld?.Init(0);

            //accept clients
            this.netService.acceptClients = true;
        }

        protected override void OnMapDestroy()
        {
            this.currentWorld?.Destroy();
            this.currentWorld = null;

            //reject clients
            this.netService.acceptClients = false;
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            //sync update list
            if (nextPLayerListUpdate <= 0)
            {
                this.SendPlayerListUpdate();
                nextPLayerListUpdate += PlayerListUpdateSync;
            }
            else
            {
                nextPLayerListUpdate -= delta;
            }

            //check if players are realy disconnected and delete them totaly
            foreach (var player in this._players.Where(df => df.Value.State == ClientState.Disconnected).ToArray())
            {
                if (player.Value.DisconnectTime <= 0)
                {
                    var id = player.Key;
                    this._players.Remove(id);
                    this.currentWorld?.RemovePlayerSimulation(id);
                }
                else
                {
                    player.Value.DisconnectTime -= delta;
                }
            }
        }

        public override void _EnterTree()
        {
            this.netService = this._serviceRegistry.Create<Shooter.Server.Services.ServerNetworkService>();
            this.netService.ConnectionEstablished += () =>
            {
                this.LoadWorld(DefaultMapName, 0);
            };

            this.netService.ClientConnected += this.onPlayerConnected;
            this.netService.ClientDisconnect += this.onPlayerDisconnect;
            this.netService.ClientLatencyUpdate += (clientId, latency) =>
            {
                if (this._players.ContainsKey(clientId))
                {
                    this._players[clientId].Latency = latency;
                }
            };

            this.netService.Subscribe<ClientInitializationPackage>(this.InitializePlayer);
            this.netService.SubscribeSerialisable<PlayerInputCommand>(this.OnPlayerInput);
            base._EnterTree();
        }

        public void OnPlayerInput(PlayerInputCommand package, NetPeer peer)
        {
            var clientId = peer.Id;
            if (this._players.ContainsKey(clientId))
            {
                (this.currentWorld as ServerGameWorld).EnqueneInput(clientId, package);
            }
        }

        public void InitializePlayer(ClientInitializationPackage package, NetPeer peer)
        {
            var clientId = peer.Id;
            if (this._players.ContainsKey(clientId))
            {
                var oldState = this._players[clientId].State;
                if (oldState != ClientState.Initialized)
                {
                    Logger.LogDebug(this, "[" + clientId + "] " + " Initialize player.");
                    var simulation = this.currentWorld?.AddPlayerSimulation<ServerPlayerSimulation>(clientId);
                    if (simulation != null)
                    {
                        var spawnPoint = this.currentWorld.Level.GetFreeSpawnPoint();
                        spawnPoint.inUsage = true;

                        Logger.LogDebug(this, "[" + clientId + "] " + " Set spawnpoint to " + spawnPoint.Transform.origin);

                        var component = simulation.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
                        var playerServerCamera = simulation.Components.AddComponent<PlayerCameraComponent>();
                        playerServerCamera.cameraMode = CameraMode.Server;

                        var gt = component.GlobalTransform;
                        gt.origin = spawnPoint.GlobalTransform.origin;
                        component.GlobalTransform = gt;

                        (this.currentWorld as ServerGameWorld).InitializePlayerState(clientId);
                        this._players[clientId].State = ClientState.Initialized;

                        this.SendPlayerListUpdate();
                    }
                    else
                    {
                        var currentPlayer = this.currentWorld?.GetPlayerSimulation(clientId);
                        this.SendPlayerListUpdate();
                    }
                }
            }

            this.netService.SendMessage<ServerInitializationPackage>(peer.Id, new ServerInitializationPackage { PlayerId = clientId, GameTick = this.currentWorld.WorldTick });
        }

        private void onPlayerDisconnect(int clientId, DisconnectReason reason)
        {
            if (this._players.ContainsKey(clientId))
            {
                this._players[clientId].PreviousState = this._players[clientId].State;
                this._players[clientId].State = ClientState.Disconnected;
                this._players[clientId].DisconnectTime = deleteTimeForPlayer;
            }
        }

        private void onPlayerConnected(int clientId)
        {
            if (!this._players.ContainsKey(clientId))
            {
                this._players.Add(clientId, new ServerPlayer
                {
                    Team = PlayerTeam.SPECTATOR,
                    State = ClientState.Connected
                });
            }
            else
            {
                this._players[clientId].State = this._players[clientId].PreviousState;
            }

            var message = new Shared.Network.Packages.WorldNetPackage();
            message.WorldName = DefaultMapName;
            message.WorldTick = this.currentWorld.WorldTick;

            this.netService.SendMessage<Shared.Network.Packages.WorldNetPackage>(clientId, message);
        }

        private void SendPlayerListUpdate()
        {
            if (this.currentWorld != null)
            {
                //get list of connected players
                var list = this._players.
                            Select(df => new PlayerUpdatePackage { Id = df.Key, Team = df.Value.Team, State = df.Value.State, Latency = df.Value.Latency }).ToArray();

                //get current network states
                var states = new List<PlayerStatePackage>();
                foreach (var client in list)
                {
                    var simulation = this.currentWorld.GetPlayerSimulation(client.Id);
                    if (simulation != null)
                    {
                        states.Add(simulation.ToNetworkState());
                    }
                }

                //send list to each client
                var playerList = new PlayerListUpdatePackage { Players = list, PlayerStates = states.ToArray(), WorldTick = this.currentWorld.WorldTick };
                this.netService.SentMessageToAll(playerList);
            }
        }
    }
}
