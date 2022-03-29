/*
 * Created on Mon Mar 28 2022
 *
 * The MIT License (MIT)
 * Copyright (c) 2022 Stefan Boronczyk, Striked GmbH
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without restriction,
 * including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial
 * portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
 * TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using Godot;
using Framework;
using System;
using System.Collections.Generic;
using LiteNetLib;
using System.Linq;
using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Utils;
using Framework.Network;
using Framework.Network.Services;
using Framework.Physics;
using Framework.Game.Server;

namespace Framework.Game.Client
{
    /// <summary>
    /// Base class for the client world
    /// </summary>
    public abstract class ClientWorld : World
    {
        /// <inheritdoc />
        internal ClientNetworkService netService = null;

        /// <inheritdoc />
        internal uint lastServerWorldTick = 0;

        /// <inheritdoc />
        internal int replayedStates;

        /// <inheritdoc />
        internal int _myServerId = -1;

        /// <summary>
        /// Client adjuster
        /// Handles server ticks and make them accuracy
        /// </summary>
        public ClientSimulationAdjuster clientSimulationAdjuster;

        /// <inheritdoc />
        private Queue<WorldHeartbeat> worldStateQueue = new Queue<WorldHeartbeat>();

        /// <inheritdoc />
        private MovingAverage excessWorldStateAvg = new MovingAverage(10);

        /// <summary>
        /// Local player class
        /// </summary>
        public LocalPlayer localPlayer;

        /// <summary>
        /// The current client player id  the server
        /// </summary>
        public int MyServerId => _myServerId;

        /// <inheritdoc />
        internal override void InternalTreeEntered()
        {
            base.InternalTreeEntered();

            this.netService = this.gameInstance.Services.Get<ClientNetworkService>();
            this.netService.SubscribeSerialisable<WorldHeartbeat>(HandleWorldState);
            this.netService.SubscribeSerialisable<ClientInitializer>(InitWorld);
            this.netService.SubscribeSerialisable<ServerVarUpdate>(UpdateWorld);
        }

        private void UpdateWorld(ServerVarUpdate cmd, NetPeer peer)
        {
            this._serverVars = new VarsCollection(cmd.ServerVars);
        }

        private void InitWorld(ClientInitializer cmd, NetPeer peer)
        {
            Logger.LogDebug(this, "Init world with server user id " + cmd.PlayerId);

            this._myServerId = cmd.PlayerId;
            this?.Init(new VarsCollection(cmd.ServerVars), cmd.GameTick);
        }

        private void HandleWorldState(WorldHeartbeat cmd, NetPeer peed)
        {
            worldStateQueue.Enqueue(cmd);
        }

        /// <inheritdoc />
        internal override void Init(VarsCollection serverVars, uint initalWorldTick)
        {
            base.Init(serverVars, 0);

            var simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
            lastServerWorldTick = initalWorldTick;

            this.simulationAdjuster = clientSimulationAdjuster = new ClientSimulationAdjuster(simTickRate / 2);
            WorldTick = clientSimulationAdjuster.GuessClientTick((float)this.GetPhysicsProcessDeltaTime(), initalWorldTick, this.netService.Ping);
        }

        /// <inheritdoc />
        internal override void PostUpdate()
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

        /// <inheritdoc />  
        internal override void InternalTick(float interval)
        {
            if (this.localPlayer != null && this.localPlayer.Inputable != null)
            {
                float simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
                var serverSendRate = simTickRate / 2;

                var MaxStaleServerStateTicks = (int)MathF.Ceiling(this.ServerVars.Get<int>("sv_max_stages_ms", 500) / serverSendRate);

                var inputHandler = this.localPlayer.Inputable;
                var inputs = (inputHandler != null && !this.gameInstance.GuiDisableInput) ? inputHandler.GetPlayerInput() : new GeneralPlayerInput();

                var lastTicks = WorldTick - lastServerWorldTick;
                if (this.ServerVars.Get<bool>("sv_freze_client", false) && lastTicks >= MaxStaleServerStateTicks)
                {
                    Logger.LogDebug(this, "Server state is too old (is the network connection dead?) - max ticks " + MaxStaleServerStateTicks + " - currentTicks => " + lastTicks);
                    inputs = new GeneralPlayerInput();
                }

                // Update our snapshot buffers.
                uint bufidx = WorldTick % 1024;
                this.localPlayer.localPlayerInputsSnapshots[bufidx] = inputs;
                this.localPlayer.localPlayerStateSnapshots[bufidx] = this.localPlayer.ToNetworkState();
                this.localPlayer.localPlayerWorldTickSnapshots[bufidx] = lastServerWorldTick;

                // Send a command for all inputs not yet acknowledged from the server.
                var unackedInputs = new List<GeneralPlayerInput>();
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
                this.localPlayer.SetPlayerInputs(inputs);

                // SimulateWorld
                this.localPlayer.InternalTick(interval);
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
            this.ProcessServerWorldState(incomingState, interval);

            this.Tick(interval);
        }

        /// <summary>
        /// Execute world heartbeat to initalize players and set values
        /// </summary>
        /// <param name="update"></param>
        internal void ExecuteHeartbeat(WorldHeartbeat update)
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
                var playersToDelete = this._players?.Where(df => !playerIds.Contains(df.Key))
                    .Where(df => df.Value is Player).ToArray();
                foreach (var player in playersToDelete)
                {
                    var networkPlayer = player.Value as Player;
                    networkPlayer.QueueFree();
                    this._players.Remove(player.Key);

                    if (networkPlayer.Id == this.MyServerId)
                    {
                        Logger.LogDebug(this, "Local player are realy disconnected!");
                        (this.gameInstance as ClientLogic<ClientWorld>).Disconnect();
                        return;
                    }
                }

                //players
                foreach (var playerUpdate in playerUpdates)
                {
                    if (!this._players.ContainsKey(playerUpdate.Id))
                    {
                        if (playerUpdate.Id == this.MyServerId)
                        {
                            Logger.LogDebug(this, "Attach new local player");
                            var localPlayer = this.CreateLocalPlayer(playerUpdate.Id);
                            localPlayer.Id = playerUpdate.Id;
                            localPlayer.GameWorld = this;
                            localPlayer.Name = playerUpdate.Id.ToString();
                            this.playerHolder.AddChild(localPlayer);
                            this._players.Add(playerUpdate.Id, localPlayer);

                            //set local player
                            this.localPlayer = localPlayer;
                        }
                        else
                        {
                            Logger.LogDebug(this, "Attach new puppet player");
                            var puppetPlayer = this.CreatePuppetPlayer(playerUpdate.Id);
                            puppetPlayer.Id = playerUpdate.Id;
                            puppetPlayer.GameWorld = this;
                            puppetPlayer.Name = playerUpdate.Id.ToString();
                            this.playerHolder.AddChild(puppetPlayer);
                            this._players.Add(playerUpdate.Id, puppetPlayer);
                        }

                        if (playerUpdate.State != PlayerConnectionState.Initialized
                            && localPlayer.State == PlayerConnectionState.Initialized)
                        {
                            this.OnPlayerInitilaized(localPlayer);
                        }
                    }

                    var player = this._players[playerUpdate.Id] as Player;
                    this.updatePlayerValues(playerUpdate, player);
                }
            }
        }

        private void updatePlayerValues(PlayerUpdate playerUpdate, IPlayer player)
        {
            player.Id = playerUpdate.Id;
            player.Team = playerUpdate.Team;
            player.Latency = playerUpdate.Latency;
            player.PlayerName = playerUpdate.PlayerName;
            player.State = playerUpdate.State;
            player.RequiredComponents = (player is LocalPlayer) ? playerUpdate.RequiredComponents : playerUpdate.RequiredComponents;

            if (playerUpdate.State == PlayerConnectionState.Initialized)
            {
                //check if we have to remove some components
                foreach (var avaiableComponents in player.AvaiablePlayerComponents)
                {
                    var componentExist = player.Components.HasComponent(avaiableComponents.Value.NodeType);
                    var isRequired = player.RequiredComponents.Contains(avaiableComponents.Key);

                    if (componentExist && !isRequired)
                    {
                        player.Components.DeleteComponent(avaiableComponents.Value.NodeType);
                    }

                    else if (isRequired && !componentExist)
                    {
                        Node result = null;

                        if (avaiableComponents.Value.ResourcePath != null)
                        {
                            result = player.Components.AddComponent(avaiableComponents.Value.NodeType, avaiableComponents.Value.ResourcePath);
                        }
                        else
                        {
                            result = player.Components.AddComponent(avaiableComponents.Value.NodeType);
                        }

                        if (result != null && result is IMoveable && player is PhysicsPlayer)
                        {
                            (player as PhysicsPlayer).Body = result as IMoveable;
                        }

                        if (result != null && result is IInputable && player is LocalPlayer)
                        {
                            (player as LocalPlayer).Inputable = result as IInputable;
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Create an puppet player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual PuppetPlayer CreatePuppetPlayer(int id)
        {
            return new PuppetPlayer();
        }

        /// <summary>
        /// Create an local player
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual LocalPlayer CreateLocalPlayer(int id)
        {
            return new LocalPlayer();
        }

        /// <inheritdoc />
        private void ProcessServerWorldState(WorldHeartbeat incomingState, float interval)
        {
            //set the last server world tick
            lastServerWorldTick = incomingState.WorldTick;

            // Calculate our actual tick lead on the server perspective. We add one because the world
            // state the server sends to use is always 1 higher than the latest input that has been
            // processed.
            if (incomingState.YourLatestInputTick > 0)
            {
                int actualTickLead = (int)incomingState.YourLatestInputTick - (int)lastServerWorldTick + 1;
                this.clientSimulationAdjuster.NotifyActualTickLead(actualTickLead, false, this.ServerVars.Get<bool>("sv_agressive_lag_reduction", true));
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

            // Locate the data for our local playerr
            foreach (var playerState in incomingState.PlayerStates)
            {
                if (playerState.Id == this.MyServerId)
                {
                    //send state to local
                    if (this.localPlayer != null)
                    {
                        this.localPlayer.incomingLocalPlayerState = playerState;
                    }
                }
                else
                {
                    //send states to puppets
                    foreach (var player in _players.Where(df => df.Value is PuppetPlayer).Select(df => df.Value as PuppetPlayer).ToArray())
                    {
                        player.ApplyNetworkState(playerState);
                    }
                }
            }

            // Handle local player rewinding
            if (this.localPlayer != null && this.localPlayer.Body != null)
            {
                if (default(PlayerState).Equals(this.localPlayer.incomingLocalPlayerState))
                {
                    Logger.LogDebug(this, "No local player state found!");
                }

                // Lookup the historical state for the world tick we got.
                uint bufidx = incomingState.WorldTick % 1024;
                var stateSnapshot = this.localPlayer.localPlayerStateSnapshots[bufidx];

                // Compare the historical state to see how off it was.
                var error = this.localPlayer.incomingLocalPlayerState.Position - stateSnapshot.Position;
                if (error.LengthSquared() > 0.0001f)
                {
                    if (!headState)
                    {
                        Logger.LogDebug(this, $"Rewind tick#{incomingState.WorldTick}, Error: {error.Length()}, Range: {WorldTick - incomingState.WorldTick} ClientPost: {this.localPlayer.incomingLocalPlayerState.Position.ToString()} ServerPos: {stateSnapshot.Position.ToString()} ");
                        replayedStates++;
                    }

                    // Rewind local player state to the correct state from the server.
                    // TODO: Cleanup a lot of this when its merged with how rockets are spawned.
                    this.localPlayer.ApplyNetworkState(this.localPlayer.incomingLocalPlayerState);

                    // Loop through and replay all captured input snapshots up to the current tick.
                    uint replayTick = incomingState.WorldTick;

                    while (replayTick < WorldTick)
                    {
                        // Grab the historical input.
                        bufidx = replayTick % 1024;
                        var inputSnapshot = this.localPlayer.localPlayerInputsSnapshots[bufidx];

                        // Rewrite the historical sate snapshot.
                        this.localPlayer.localPlayerStateSnapshots[bufidx] = this.localPlayer.ToNetworkState();

                        // Apply inputs to the associated player controller and simulate the world.
                        this.localPlayer.SetPlayerInputs(inputSnapshot);
                        this.localPlayer.InternalTick((float)this.GetPhysicsProcessDeltaTime());

                        ++replayTick;
                    }
                }
            }

            // Internal tick for puppets
            foreach (var player in _players.Where(df => df.Value is PuppetPlayer).Select(df => df.Value as PuppetPlayer).ToArray())
            {
                player.InternalTick(interval);
            }
        }
    }
}
