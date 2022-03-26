using Godot;
using Shooter.Shared;
using Shooter.Client.Services;
using Shooter.Client.Simulation;
using Shooter.Client.Simulation.Components;
using System;
using Shooter.Shared.Network.Packages;
using System.Collections.Generic;
using LiteNetLib;
using System.Linq;

namespace Shooter.Client.World
{
    public partial class ClientGameWorld : GameWorld
    {
        protected ClientNetworkService netService = null;

        /// <summary>
        /// Client adjuster
        /// Handles server ticks and make them accuracy
        /// </summary>
        public ClientSimulationAdjuster clientSimulationAdjuster;

        /// <summary>
        /// World Heartbeat Queue
        /// </summary>
        /// <typeparam name="WorldHeartbeat"></typeparam>
        /// <returns></returns>
        private Queue<WorldHeartbeat> worldStateQueue = new Queue<WorldHeartbeat>();

        private MovingAverage excessWorldStateAvg = new MovingAverage(10);

        /// <summary>
        /// Local player class
        /// </summary>
        public LocalPlayer localPlayer;

        /// <summary>
        /// Last executed server tick
        /// </summary>
        public uint lastServerWorldTick = 0;

        /// <summary>
        /// Last replayed states
        /// </summary>
        private int replayedStates;

        private int _myServerId = -1;

        public int MyServerId => _myServerId;

        public override void _EnterTree()
        {
            base._EnterTree();

            this.netService = this.gameInstance.Services.Get<ClientNetworkService>();
            this.netService.Subscribe<WorldHeartbeat>(HandleWorldState);
            this.netService.SubscribeSerialisable<ClientInitializer>(InitWorld);
        }

        private void InitWorld(ClientInitializer cmd, NetPeer peer)
        {
            Logger.LogDebug(this, "Init world with server user id " + cmd.PlayerId + " => vars " + cmd.ServerVars?.Count);
            this._myServerId = cmd.PlayerId;
            this?.Init(cmd.ServerVars, cmd.GameTick);
        }

        private void HandleWorldState(WorldHeartbeat cmd, NetPeer peed)
        {
            worldStateQueue.Enqueue(cmd);
        }

        public override void Init(Dictionary<string, string> serverVars, uint initalWorldTick)
        {
            base.Init(serverVars, 0);

            var simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
            lastServerWorldTick = initalWorldTick;

            this.simulationAdjuster = clientSimulationAdjuster = new ClientSimulationAdjuster(simTickRate / 2);
            WorldTick = clientSimulationAdjuster.GuessClientTick((float)this.GetPhysicsProcessDeltaTime(), initalWorldTick, this.netService.ping);
        }

        /// <summary>
        /// After each physical frame
        /// </summary>
        /// <param name="interval"></param>
        protected override void PostUpdate()
        {
            // Process the remaining world states if there are any, though we expect this to be empty?
            // TODO: This is going to need to be structured pretty differently with other players.
            excessWorldStateAvg.Push(worldStateQueue.Count);
            //while (worldStateQueue.Count > 0) {
            //  ProcessServerWorldState();
            //}
            // Show some debug monitoring values.
            Logger.SetDebugUI("cl rewinds", replayedStates.ToString());
            Logger.SetDebugUI("incoming state excess", excessWorldStateAvg.Average().ToString());

            clientSimulationAdjuster.Monitoring();
        }

        /// <summary>
        /// Handle client tick
        /// </summary>
        /// <param name="interval"></param>
        public override void Tick(float interval)
        {
            if (this.localPlayer != null && this.localPlayer.Simulation != null)
            {
                float simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
                var serverSendRate = simTickRate / 2;

                var MaxStaleServerStateTicks = (int)MathF.Ceiling(
                   int.Parse(this.ServerVars["sv_max_stages_ms"]) / serverSendRate);

                var inputHandler = this.localPlayer.Simulation.Components.Get<PlayerInputComponent>();
                var inputs = (inputHandler != null && !this.gameInstance.GuiDisableInput) ? inputHandler.GetPlayerInput() : new PlayerInputs();

                var lastTicks = WorldTick - lastServerWorldTick;
                if (bool.Parse(this.ServerVars["sv_freze_client"]) && lastTicks >= MaxStaleServerStateTicks)
                {
                    Logger.LogDebug(this, "Server state is too old (is the network connection dead?) - max ticks " + MaxStaleServerStateTicks + " - currentTicks => " + lastTicks);
                    inputs = new PlayerInputs();
                }

                // Update our snapshot buffers.
                uint bufidx = WorldTick % 1024;
                this.localPlayer.localPlayerInputsSnapshots[bufidx] = inputs;
                this.localPlayer.localPlayerStateSnapshots[bufidx] = this.localPlayer.Simulation.ToNetworkState();
                this.localPlayer.localPlayerWorldTickSnapshots[bufidx] = lastServerWorldTick;

                // Send a command for all inputs not yet acknowledged from the server.
                var unackedInputs = new List<PlayerInputs>();
                var clientWorldTickDeltas = new List<short>();
                // TODO: lastServerWorldTick is technically not the same as lastAckedInputTick, fix this.
                for (uint tick = lastServerWorldTick; tick <= WorldTick; ++tick)
                {
                    unackedInputs.Add(this.localPlayer.localPlayerInputsSnapshots[tick % 1024]);
                    clientWorldTickDeltas.Add((short)(tick - this.localPlayer.localPlayerWorldTickSnapshots[tick % 1024]));
                }

                var command = new PlayerInputCommand
                {
                    StartWorldTick = lastServerWorldTick,
                    Inputs = unackedInputs.ToArray(),
                    ClientWorldTickDeltas = clientWorldTickDeltas.ToArray(),
                };

                //send to server => command
                this.netService.SendMessageSerialisable(this.netService.ServerPeer.Id, command, LiteNetLib.DeliveryMethod.Sequenced);

                //SetPlayerInputs
                this.localPlayer.Simulation.SetPlayerInputs(inputs);

                // SimulateWorld
                this.localPlayer.Simulation.Simulate(interval);
            }

            //increase worldTick
            ++this.WorldTick;

            //run world state queue
            if (worldStateQueue.Count < 1)
            {
                return;
            }

            var incomingState = worldStateQueue.Dequeue();

            //update player list and values
            this.ExecuteHeartbeat(incomingState);

            //procese player inputs
            this.ProcessServerWorldState(incomingState);
        }

        /// <summary>
        /// Execute world heartbeat to initalize players and set values
        /// </summary>
        /// <param name="update"></param>
        private void ExecuteHeartbeat(WorldHeartbeat update)
        {
            //check that the id is initialized
            if (this.MyServerId < 0)
            {
                return;
            }

            var playerUpdates = update.PlayerUpdates;
            var playerStates = update.PlayerStates;
            if (playerUpdates != null && playerUpdates.Length > 0)
            {
                // Logger.LogDebug(this, "Players heartbeat => Amount: " + playerUpdates.Length);
                var currentPlayerId = this.MyServerId;

                //get player ids for delete selection
                var playerIds = playerUpdates.Select(df => df.Id).ToArray();

                //delete unused players
                var playersToDelete = this._players?.Where(df => !playerIds.Contains(df.Key)).ToArray();
                foreach (var player in playersToDelete)
                {
                    if (player.Value.Simulation != null)
                    {
                        this.RemovePlayerSimulation(player.Value.Simulation);
                        player.Value.Simulation = null;
                    }
                    this._players.Remove(player.Key);

                    if (player.Key == this.MyServerId)
                    {
                        Logger.LogDebug(this, "Local player are realy disconnected!");
                        (this.gameInstance as ClientGameLogic).Disconnect();
                        return;
                    }
                }

                //players
                foreach (var playerUpdate in playerUpdates)
                {
                    //creat local player
                    if (playerUpdate.Id == this.MyServerId)
                    {
                        this.UpdateOrCreateLocalPlayer(playerUpdate, playerStates.FirstOrDefault(df => df.Id == playerUpdate.Id));
                    }
                    //create puppet player
                    else
                    {
                        this.UpdateOrCreatePuppetPlayer(playerUpdate, playerStates.FirstOrDefault(df => df.Id == playerUpdate.Id));
                    }
                }
            }
        }


        /// <summary>
        /// Update or create puppet player
        /// </summary>
        /// <param name="playerUpdate"></param>
        /// <param name="playerState"></param>
        private void UpdateOrCreatePuppetPlayer(PlayerUpdate playerUpdate, PlayerState playerState)
        {
            if (!this._players.ContainsKey(playerUpdate.Id))
            {
                this._players.Add(playerUpdate.Id, new PuppetPlayer());
            }

            var player = this._players[playerUpdate.Id];
            this.updatePlayerValues(playerUpdate, player);

            if (playerUpdate.State == ClientState.Initialized && player.Simulation == null)
            {
                var simulation = this.AddPlayerSimulation<PuppetPlayerSimulation>(playerUpdate.Id);
                if (simulation != null)
                {
                    Logger.LogDebug(this, "Create puppet player with pos " + playerState.Position);
                    var body = simulation.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
                    simulation.ApplyNetworkState(playerState);
                    player.Simulation = simulation;
                }
            }
        }

        /// <summary>
        /// Update or create local player
        /// </summary>
        /// <param name="playerUpdate"></param>
        /// <param name="playerState"></param>
        private void UpdateOrCreateLocalPlayer(PlayerUpdate playerUpdate, PlayerState playerState)
        {
            if (!this._players.ContainsKey(playerUpdate.Id))
            {
                this._players.Add(playerUpdate.Id, new LocalPlayer());
            }

            var player = this._players[playerUpdate.Id];
            this.updatePlayerValues(playerUpdate, player);

            if (player.State == ClientState.Initialized && player.Simulation == null)
            {
                var playerSimulation = this.AddPlayerSimulation<LocalPlayerSimulation>(playerUpdate.Id);
                if (playerSimulation != null)
                {
                    Logger.LogDebug(this, "Create local player with pos " + playerState.Position);
                    //(this.currentWorld as ClientGameWorld).Init(worldTick);

                    //add component
                    playerSimulation.Components.AddComponent<PlayerCameraComponent>();
                    playerSimulation.Components.AddComponent<PlayerInputComponent>();

                    var body = playerSimulation.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
                    playerSimulation.ApplyNetworkState(playerState);
                    player.Simulation = playerSimulation;

                    this.localPlayer = player as LocalPlayer;
                }
            }
        }
        /// <summary>
        /// Compare inputs with current state and do client interpolation
        /// </summary>
        /// <param name="incomingState"></param>
        private void ProcessServerWorldState(WorldHeartbeat incomingState)
        {
            //set the last server world tick
            lastServerWorldTick = incomingState.WorldTick;

            // Calculate our actual tick lead on the server perspective. We add one because the world
            // state the server sends to use is always 1 higher than the latest input that has been
            // processed.
            if (incomingState.YourLatestInputTick > 0)
            {
                int actualTickLead = (int)incomingState.YourLatestInputTick - (int)lastServerWorldTick + 1;
                this.clientSimulationAdjuster.NotifyActualTickLead(actualTickLead, false, bool.Parse(this.ServerVars["sv_agressive_lag_reduction"]));
            }

            // For debugging purposes, log the local lead we're running at
            var localWorldTickLead = WorldTick - lastServerWorldTick;
            //Logger.LogDebug(this, "local tick lead =>" + localWorldTickLead);

            bool headState = false;
            if (incomingState.WorldTick >= WorldTick)
            {
                headState = true;
            }
            if (incomingState.WorldTick > WorldTick)
            {
                Logger.LogDebug(this, "Got a FUTURE tick somehow???");
            }

            // Locate the data for our local player.
            PlayerState incomingLocalPlayerState = new PlayerState();

            foreach (var playerState in incomingState.PlayerStates)
            {
                if (playerState.Id == this.MyServerId)
                {
                    incomingLocalPlayerState = playerState;
                }
                else
                {
                    var player = this.playerHolder.GetNodeOrNull<PuppetPlayerSimulation>(playerState.Id.ToString());
                    player?.ApplyNetworkState(playerState);
                }
            }

            if (default(PlayerState).Equals(incomingLocalPlayerState))
            {
                Logger.LogDebug(this, "No local player state found!");
            }

            if (this.localPlayer != null)
            {
                // Lookup the historical state for the world tick we got.
                uint bufidx = incomingState.WorldTick % 1024;
                var stateSnapshot = this.localPlayer.localPlayerStateSnapshots[bufidx];

                // Compare the historical state to see how off it was.
                if (this.localPlayer.Simulation != null)
                {
                    var error = incomingLocalPlayerState.Position - stateSnapshot.Position;
                    if (error.LengthSquared() > 0.0001f)
                    {
                        if (!headState)
                        {
                            Logger.LogDebug(this, $"Rewind tick#{incomingState.WorldTick}, Error: {error.Length()}, Range: {WorldTick - incomingState.WorldTick} ClientPost: {incomingLocalPlayerState.Position.ToString()} ServerPos: {stateSnapshot.Position.ToString()} ");
                            replayedStates++;
                        }

                        // Rewind local player state to the correct state from the server.
                        // TODO: Cleanup a lot of this when its merged with how rockets are spawned.
                        this.localPlayer.Simulation.ApplyNetworkState(incomingLocalPlayerState);

                        // Loop through and replay all captured input snapshots up to the current tick.
                        uint replayTick = incomingState.WorldTick;

                        while (replayTick < WorldTick)
                        {
                            // Grab the historical input.
                            bufidx = replayTick % 1024;
                            var inputSnapshot = this.localPlayer.localPlayerInputsSnapshots[bufidx];

                            // Rewrite the historical sate snapshot.
                            this.localPlayer.localPlayerStateSnapshots[bufidx] = this.localPlayer.Simulation.ToNetworkState();

                            // Apply inputs to the associated player controller and simulate the world.
                            this.localPlayer.Simulation.SetPlayerInputs(inputSnapshot);
                            this.localPlayer.Simulation.Simulate((float)this.GetPhysicsProcessDeltaTime());

                            ++replayTick;
                        }
                    }
                }
            }
        }
    }
}
