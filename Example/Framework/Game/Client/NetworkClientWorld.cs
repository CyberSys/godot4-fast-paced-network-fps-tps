using System.ComponentModel;
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
using System.Diagnostics;
using System.Linq;
using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Utils;
using Framework.Network;
using Framework.Network.Services;
using Framework.Physics;
using Framework.Game.Server;
using Framework.Physics.Commands;

namespace Framework.Game.Client
{
    /// <summary>
    /// Base class for the client world
    /// </summary>
    public partial class NetworkClientWorld : NetworkWorld
    {
        /// <inheritdoc />
        internal ClientNetworkService netService = null;

        /// <inheritdoc />
        internal int _myServerId = -1;


        /// <inheritdoc />
        private Queue<WorldHeartbeat> worldStateQueue = new Queue<WorldHeartbeat>();

        /// <inheritdoc />
        private MovingAverage excessWorldStateAvg = new MovingAverage(10);

        /// <summary>
        /// The current client player id  the server
        /// </summary>
        public int MyServerId => _myServerId;

        internal Utils.LineDrawer3d rayCastTester = new LineDrawer3d();


        /// <inheritdoc />
        internal override void InternalTreeEntered()
        {
            base.InternalTreeEntered();

            this.netService = this.GameInstance.Services.Get<ClientNetworkService>();
            this.netService.SubscribeSerialisable<WorldHeartbeat>(HandleWorldState);
            this.netService.SubscribeSerialisable<ClientWorldInitializer>(InitWorld);
            this.netService.SubscribeSerialisable<ServerVarUpdate>(UpdateWorld);
            this.netService.SubscribeSerialisable<RaycastTest>(RayCastTest);

            this.AddChild(this.rayCastTester);
        }

        internal void RayCastTest(RaycastTest cmd, NetPeer peed)
        {
            this.rayCastTester.AddLine(cmd.from, cmd.to);
        }

        /// <inheritdoc />
        internal override void OnLevelInternalAddToScene()
        {
            applyGlow(ClientSettings.Variables.Get<bool>("cl_draw_glow", false));
            applySDFGI(ClientSettings.Variables.Get<bool>("cl_draw_sdfgi", false));
            applySSAO(ClientSettings.Variables.Get<bool>("cl_draw_ssao", false));
            applySSIL(ClientSettings.Variables.Get<bool>("cl_draw_ssil", false));

            ClientSettings.Variables.OnChange += (state, name, value) =>
            {
                if (name == "cl_draw_glow")
                {
                    applyGlow(ClientSettings.Variables.Get<bool>("cl_draw_glow"));
                }
                if (name == "cl_draw_sdfgi")
                {
                    applySDFGI(ClientSettings.Variables.Get<bool>("cl_draw_sdfgi"));
                }
                if (name == "cl_draw_ssao")
                {
                    applySSAO(ClientSettings.Variables.Get<bool>("cl_draw_ssao"));
                }
                if (name == "cl_draw_ssil")
                {
                    applySSIL(ClientSettings.Variables.Get<bool>("cl_draw_ssil"));
                }
            };

            base.OnLevelInternalAddToScene();
        }

        private void UpdateWorld(ServerVarUpdate cmd, NetPeer peer)
        {
            this._serverVars = new VarsCollection(cmd.ServerVars);
        }

        private void InitWorld(ClientWorldInitializer cmd, NetPeer peer)
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

            this.simulationAdjuster = clientSimulationAdjuster = new ClientSimulationAdjuster(simTickRate / 2);
            WorldTick = clientSimulationAdjuster.GuessClientTick((float)this.GetPhysicsProcessDeltaTime(), initalWorldTick, this.netService.Ping);
            LastServerWorldTick = initalWorldTick;
        }

        /// <inheritdoc />
        public uint LastServerWorldTick { get; private set; } = 0;

        /// <summary>
        /// Client adjuster
        /// Handles server ticks and make them accuracy
        /// </summary>
        public ClientSimulationAdjuster clientSimulationAdjuster;

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
            // Logger.SetDebugUI("cl_rewinds", replayedStates.ToString());
            //    Logger.SetDebugUI("incoming_state_excess", excessWorldStateAvg.Average().ToString());

            clientSimulationAdjuster.Monitoring();
        }

        /// <inheritdoc />  
        internal override void InternalTick(float interval)
        {
            foreach (var player in _players.Select(df => df.Value).ToArray())
            {
                player.InternalTick(interval);
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

            this.Tick(interval);
        }

        private List<int> playerCreationInProcess = new List<int>();

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
                var playersToDelete = this._players?.Where(df => !playerIds.Contains(df.Key)).ToArray();
                foreach (var player in playersToDelete)
                {
                    var networkPlayer = player.Value;
                    networkPlayer.QueueFree();
                    this._players.Remove(player.Key);

                    if (networkPlayer.Id == this.MyServerId)
                    {
                        Logger.LogDebug(this, "Local player are realy disconnected!");
                        (this.GameInstance as NetworkClientLogic).Disconnect();
                        return;
                    }
                }

                //players
                foreach (var playerUpdate in playerUpdates)
                {
                    if (!this._players.ContainsKey(playerUpdate.Id))
                    {
                        if (this.playerCreationInProcess.Contains(playerUpdate.Id))
                        {
                            return;
                        }
                        GD.Load<CSharpScript>(playerUpdate.ScriptPath);
                        this.playerCreationInProcess.Add(playerUpdate.Id);

                        Framework.Utils.AsyncLoader.Loader.LoadResource(playerUpdate.ResourcePath, (res) =>
                        {
                            Logger.LogDebug(this, "Attach new local player");
                            NetworkCharacter player = (res as PackedScene).Instantiate<NetworkCharacter>();

                            if (playerUpdate.Id == this.MyServerId)
                            {
                                player.Mode = NetworkMode.CLIENT;
                                player.Id = playerUpdate.Id;
                                player.GameWorld = this;
                                player.Name = playerUpdate.Id.ToString();
                                this.playerHolder.AddChild(player);
                                this._players.Add(playerUpdate.Id, player);
                            }
                            else
                            {
                                Logger.LogDebug(this, "Attach new puppet player");
                                player.Mode = NetworkMode.PUPPET;
                                player.Id = playerUpdate.Id;
                                player.GameWorld = this;
                                player.Name = playerUpdate.Id.ToString();
                                this.playerHolder.AddChild(player);
                                this._players.Add(playerUpdate.Id, player);
                            }

                            if (playerUpdate.State != PlayerConnectionState.Initialized
                            && player.State == PlayerConnectionState.Initialized)
                            {
                                this.OnPlayerInitilaized(player);
                            }

                            this.updatePlayerValues(playerUpdate, player);
                            this.playerCreationInProcess.Remove(playerUpdate.Id);
                        });
                    }
                    else
                    {
                        var player = this._players[playerUpdate.Id];
                        this.updatePlayerValues(playerUpdate, player);
                    }
                }
            }
        }

        /// <summary>
        /// Send an input command to server
        /// </summary>
        /// <param name="command"></param>
        public void SendInputCommand(PlayerInputCommand command)
        {
            this.netService.SendMessageSerialisable(this.netService.ServerPeer.Id, command, LiteNetLib.DeliveryMethod.Sequenced);
        }

        private void updatePlayerValues(PlayerUpdate playerUpdate, NetworkCharacter player)
        {
            player.Id = playerUpdate.Id;
            player.Latency = playerUpdate.Latency;
            player.PlayerName = playerUpdate.PlayerName;
            player.State = playerUpdate.State;

            if (playerUpdate.State == PlayerConnectionState.Initialized)
            {
                int i = 0;
                //check if we have to remove some components
                foreach (var avaiableComponent in player.AvaiablePlayerComponents)
                {
                    var componentExist = player.Components.HasComponent(avaiableComponent.NodeType);
                    var isRequired = (player.IsLocal()) ? playerUpdate.RequiredComponents.Contains(i) : playerUpdate.RequiredPuppetComponents.Contains(i);
                    if (componentExist && !isRequired)
                    {
                        Logger.LogDebug(this, "Delete component from type " + avaiableComponent.NodeType.Name);
                        player.Components.DeleteComponent(avaiableComponent.NodeType);
                    }

                    else if (isRequired && !componentExist)
                    {
                        player.AddAssignedComponent(avaiableComponent);
                    }

                    i++;
                }
            }
        }

        /// <inheritdoc />
        private void ProcessServerWorldState(WorldHeartbeat incomingState)
        {
            //set the last server world tick
            LastServerWorldTick = incomingState.WorldTick;

            // Calculate our actual tick lead on the server perspective. We add one because the world
            // state the server sends to use is always 1 higher than the latest input that has been
            // processed.
            if (incomingState.YourLatestInputTick > 0)
            {
                int actualTickLead = (int)incomingState.YourLatestInputTick - (int)LastServerWorldTick + 1;
                this.clientSimulationAdjuster.NotifyActualTickLead(actualTickLead, false, this.ServerVars.Get<bool>("sv_agressive_lag_reduction", true));
            }

            // For debugging purposes, log the local lead we're running at
            var localWorldTickLead = this.WorldTick - this.LastServerWorldTick;
            Logger.SetDebugUI("cl_local_tick", localWorldTickLead.ToString());

            bool headState = false;
            if (incomingState.WorldTick >= this.WorldTick)
            {
                headState = true;
            }

            if (incomingState.WorldTick > this.WorldTick)
            {
                Logger.LogDebug(this, "Got a FUTURE tick somehow???");
                var tickFix = incomingState.WorldTick + 1;
                GD.Print("Old tick: " + this.WorldTick);
                WorldTick = clientSimulationAdjuster.GuessClientTick((float)this.GetPhysicsProcessDeltaTime(), incomingState.WorldTick, this.netService.Ping);
                GD.Print("New tick: " + this.WorldTick);
            }

            // Locate the data for our local playerr
            foreach (var playerState in incomingState.PlayerStates)
            {
                if (!_players.ContainsKey(playerState.Id))
                    continue;

                var player = _players[playerState.Id];

                // send to local player
                if (player.IsLocal())
                {
                    player.IncomingLocalPlayerState = playerState;
                    player.Rewind(incomingState.WorldTick, headState);
                }
                //send states to puppets
                else
                {
                    player.ApplyNetworkState(playerState);
                    player.InternalTick((float) this.GetPhysicsProcessDeltaTime());
                }
            }
        }
    }
}
