using System;
using System.Linq;
using Godot;
using Shooter.Shared;
using Shooter.Shared.Network.Packages;
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
        private readonly Dictionary<int, PlayerUpdatePackage> _players = new Dictionary<int, PlayerUpdatePackage>();
        public Dictionary<int, PlayerUpdatePackage> Players => _players;

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
            this.netService.SendMessage<ClientInitializationPackage>(0, new ClientInitializationPackage());
            this.Components.DeleteComponent<MapLoadingComponent>();
            this.Components.DeleteComponent<PreConnectComponent>();
            Input.SetMouseMode(Input.MouseMode.Captured);
        }

        protected override void OnMapDestroy()
        {
            this._players.Clear();

            this.currentWorld?.Destroy();
            this.currentWorld = null;
            this.loadedWorldName = null;

            this.Components.DeleteComponent<MenuComponent>();
            this.Components.DeleteComponent<MapLoadingComponent>();
            this.Components.AddComponent<PreConnectComponent>("res://Client/UI/Welcome/PreConnectComponent.tscn");
        }

        protected void UpdatePlayerList(PlayerListUpdatePackage update, uint worldTick)
        {
            Console.WriteLine("Receive player update for " + this.netService.MyId);

            foreach (var player in update.Players)
            {
                Console.WriteLine(player.Id + " => " + player.State);
            }

            //check that the id is initialized
            if (this.netService.MyId < 0)
            {
                return;
            }

            var playerUpdates = update.Players;
            var playerStates = update.PlayerStates;
            if (playerUpdates != null && playerUpdates.Length > 0)
            {
                // Logger.LogDebug(this, "Players heartbeat => Amount: " + playerUpdates.Length);
                var currentPlayerId = this.netService.MyId;

                //get player ids for delete selection
                var playerIds = playerUpdates.Select(df => df.Id);

                //delete unused players
                var playersToDelete = this._players?.Where(df => !playerIds.Contains(df.Key));
                foreach (var player in playersToDelete)
                {
                    this.currentWorld?.RemovePlayerSimulation(player.Key);
                    this._players.Remove(player.Key);

                    if (player.Key == this.netService.MyId)
                    {
                        Logger.LogDebug(this, "Local player are realy disconnected!");
                        this.OnMapDestroy();
                        return;
                    }
                }

                //players
                foreach (var playerUpdate in playerUpdates)
                {
                    this._players[playerUpdate.Id] = playerUpdate;

                    if (playerUpdate.Id == this.netService.MyId)
                    {
                        if (playerUpdate.State == ClientState.Initialized)
                        {
                            var playerSimulation = this.currentWorld?.AddPlayerSimulation<LocalPlayerSimulation>(playerUpdate.Id);
                            if (playerSimulation != null)
                            {
                                var playerState = playerStates.First(df => df.Id == playerUpdate.Id);
                                Logger.LogDebug(this, "Create local player with pos " + playerState.Position);
                                (this.currentWorld as ClientGameWorld).localPlayer = playerSimulation;
                                (this.currentWorld as ClientGameWorld).Init(worldTick);

                                //add component
                                playerSimulation.Components.AddComponent<PlayerCameraComponent>();
                                playerSimulation.Components.AddComponent<PlayerInputComponent>();

                                var body = playerSimulation.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
                                playerSimulation.ApplyNetworkState(playerState);
                            }
                        }
                    }
                    else
                    {
                        if (playerUpdate.State == ClientState.Initialized)
                        {
                            var puppetPlayer = this.currentWorld?.AddPlayerSimulation<PuppetPlayerSimulation>(playerUpdate.Id);
                            if (puppetPlayer != null)
                            {
                                var playerState = playerStates.First(df => df.Id == playerUpdate.Id);
                                Logger.LogDebug(this, "Create puppet player with pos " + playerState.Position);

                                var body = puppetPlayer.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
                                puppetPlayer.ApplyNetworkState(playerState);
                            }
                        }
                    }
                }
            }
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
            this.netService.Subscribe<WorldNetPackage>((package, peer) =>
            {
                if (this.loadedWorldName != package.WorldName)
                {
                    this.loadedWorldName = package.WorldName;
                    this.LoadWorld(package.WorldName, package.WorldTick);
                }
            });

            this.netService.Subscribe<PlayerListUpdatePackage>((package, peer) =>
            {
                this.UpdatePlayerList(package, package.WorldTick);
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
