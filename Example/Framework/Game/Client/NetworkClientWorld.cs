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

namespace Framework.Game.Client
{
    public static class MiscExtensions
    {
        // Ex: collection.TakeLast(5);
        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int N)
        {
            return source.Skip(Math.Max(0, source.Count() - N));
        }
    }

    /// <summary>
    /// Base class for the client world
    /// </summary>
    public partial class NetworkClientWorld : NetworkWorld
    {
        /// <inheritdoc />
        internal ClientNetworkService netService = null;

        /// <inheritdoc />
        internal short _myServerId = -1;


        /// <inheritdoc />
        private Queue<WorldHeartbeat> worldStateQueue = new Queue<WorldHeartbeat>();

        /// <inheritdoc />
        private MovingAverage excessWorldStateAvg = new MovingAverage(10);

        /// <summary>
        /// The current client player id  the server
        /// </summary>
        public short MyServerId => _myServerId;

        internal Utils.LineDrawer3d rayCastTester = new LineDrawer3d();

        /// <summary>
        /// The local character of the client world
        /// </summary>
        /// <value></value>
        public NetworkCharacter LocalPlayer { get; set; }

        /// <inheritdoc />
        internal int replayedStates;

        /// <summary>
        /// The local player input snapshots
        /// </summary>
        public GeneralPlayerInput[] LocalPlayerInputsSnapshots = new GeneralPlayerInput[NetworkWorld.MaxTicks];

        /// <summary>
        /// The local player states
        /// </summary>
        public PlayerState[] LocalPlayerStateSnapshots = new PlayerState[NetworkWorld.MaxTicks];

        /// <summary>
        /// The last world player ticks related to the state snapshots
        /// </summary>
        public uint[] LocalPlayerWorldTickSnapshots = new uint[NetworkWorld.MaxTicks];

        /// <inheritdoc />
        internal override void InternalTreeEntered()
        {
            base.InternalTreeEntered();

            this.netService = this.GameInstance.Services.Get<ClientNetworkService>();
            this.netService.Subscribe<WorldHeartbeat>(HandleWorldState);
            this.netService.Subscribe<PlayerUpdateList>(OnPlayerUpdate);

            this.netService.Subscribe<PlayerDeletePackage>(OnPlayerDelete);
            this.netService.Subscribe<ClientWorldInitializer>(InitWorld);
            this.netService.Subscribe<ServerVarUpdate>(UpdateWorld);
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
            if (this.LocalPlayer != null && this.LocalPlayer.State == PlayerConnectionState.Initialized)
            {
                worldStateQueue.Enqueue(cmd);
            }
        }

        /// <inheritdoc />
        internal override void Init(VarsCollection serverVars, uint initalWorldTick)
        {
            this.LocalPlayerInputsSnapshots = new GeneralPlayerInput[NetworkWorld.MaxTicks];
            this.LocalPlayerStateSnapshots = new PlayerState[NetworkWorld.MaxTicks];
            this.LocalPlayerWorldTickSnapshots = new uint[NetworkWorld.MaxTicks];
            LastServerWorldTick = initalWorldTick;
            LastAckedInputTick = initalWorldTick;

            base.Init(serverVars, 0);

            var simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();

            this.simulationAdjuster = clientSimulationAdjuster = new ClientSimulationAdjuster(simTickRate / 2);
            WorldTick = clientSimulationAdjuster.GuessClientTick((float)this.GetPhysicsProcessDeltaTime(), initalWorldTick, this.netService.Ping);
        }

        /// <inheritdoc />
        public uint LastServerWorldTick { get; private set; } = 0;

        /// <inheritdoc />
        public uint LastAckedInputTick { get; private set; } = 0;

        /// <summary>
        /// Client adjuster
        /// Handles server ticks and make them accuracy
        /// </summary>
        public ClientSimulationAdjuster clientSimulationAdjuster;

        /// <inheritdoc />  
        internal override void InternalTick(float interval)
        {
            if (this.LocalPlayer == null || this.LocalPlayer.State != PlayerConnectionState.Initialized)
                return;

            this.ProcessLocalInput();

            //simulate
            this.SimulateWorld(interval);

            //increase worldTick
            ++this.WorldTick;

            //procese player inputs
            this.ProcessServerWorldState();

            //forward tick
            this.Tick(interval);
        }

        private List<short> playerCreationInProcess = new List<short>();

        private void OnPlayerDelete(PlayerDeletePackage playerDelete, NetPeer peer)
        {
            if (this._players.ContainsKey(playerDelete.NetworkId))
            {
                var networkPlayer = this._players[playerDelete.NetworkId];
                networkPlayer.QueueFree();
                this._players.Remove(playerDelete.NetworkId);

                if (networkPlayer.NetworkId == this.MyServerId)
                {
                    Logger.LogDebug(this, "Local player are realy disconnected!");
                    (this.GameInstance as NetworkClientLogic).Disconnect();
                    return;
                }
            }
        }

        private void OnPlayerUpdate(PlayerUpdateList playerUpdateList, NetPeer peer)
        {
            if (playerUpdateList.Updates == null)
                return;

            foreach (var playerUpdate in playerUpdateList.Updates)
            {
                if (!this._players.ContainsKey(playerUpdate.NetworkId))
                {
                    if (this.playerCreationInProcess.Contains(playerUpdate.NetworkId))
                    {
                        return;
                    }

                    if (playerUpdate.ScriptPaths != null)
                    {
                        foreach (var path in playerUpdate.ScriptPaths)
                        {
                            GD.Load<CSharpScript>(path);
                        }
                    }

                    this.playerCreationInProcess.Add(playerUpdate.NetworkId);
                    Framework.Utils.AsyncLoader.Loader.LoadResource(playerUpdate.ResourcePath, (res) =>
                    {
                        var resource = (res as PackedScene);
                        // resource.ResourceLocalToScene = true;
                        NetworkCharacter player = resource.Instantiate<NetworkCharacter>();

                        if (playerUpdate.NetworkId == this.MyServerId)
                        {
                            var simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();

                            Logger.LogDebug(this, "Attach new local player with id " + playerUpdate.NetworkId);
                            player.Mode = NetworkMode.CLIENT;
                            player.NetworkId = playerUpdate.NetworkId;
                            player.GameWorld = this;
                            player.Name = playerUpdate.NetworkId.ToString();
                            this.playerHolder.AddChild(player);
                            this._players.Add(playerUpdate.NetworkId, player);
                            this.LocalPlayer = player;
                        }
                        else
                        {
                            Logger.LogDebug(this, "Attach new puppet player with id " + playerUpdate.NetworkId);
                            player.Mode = NetworkMode.PUPPET;
                            player.NetworkId = playerUpdate.NetworkId;
                            player.GameWorld = this;
                            player.Name = playerUpdate.NetworkId.ToString();
                            this.playerHolder.AddChild(player);
                            this._players.Add(playerUpdate.NetworkId, player);
                        }

                        this.OnPlayerInitilaized(player);

                        this.updatePlayerValues(playerUpdate, player);
                        this.playerCreationInProcess.Remove(playerUpdate.NetworkId);
                    });
                }
                else
                {
                    var player = this._players[playerUpdate.NetworkId];
                    this.updatePlayerValues(playerUpdate, player);
                }
            }
        }

        private void updatePlayerValues(PlayerUpdate playerUpdate, NetworkCharacter player)
        {
            player.NetworkId = playerUpdate.NetworkId;
            player.PlayerName = playerUpdate.PlayerName;
            player.State = playerUpdate.State;

            if (playerUpdate.State == PlayerConnectionState.Initialized && player.IsInsideTree() && playerUpdate.RequiredComponents != null)
            {
                foreach (var avaiableComponent in player.Components.All.Where(df => df is IPlayerComponent).Select(df => df as IPlayerComponent))
                {
                    var channel = avaiableComponent.NetworkId;
                    var enable = playerUpdate.RequiredComponents.Contains(channel);
                    if (enable != avaiableComponent.IsEnabled)
                    {
                        player.ActivateComponent(avaiableComponent.NetworkId, enable);
                    }
                }
            }
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
            // Logger.SetDebugUI("cl_rewinds", replayedStates.ToString());
            //    Logger.SetDebugUI("incoming_state_excess", excessWorldStateAvg.Average().ToString());

            clientSimulationAdjuster.Monitoring();
        }

        /// <inheritdoc />
        private void ProcessServerWorldState()
        {
            //run world state queue
            if (worldStateQueue.Count < 1)
            {
                return;
            }

            var incomingState = worldStateQueue.Dequeue();

            //set the last server world tick
            LastServerWorldTick = incomingState.WorldTick;

            // Calculate our actual tick lead on the server perspective. We add one because the world
            // state the server sends to use is always 1 higher than the latest input that has been
            // processed.
            if (incomingState.YourLatestInputTick > 0)
            {

                LastAckedInputTick = incomingState.YourLatestInputTick;

                int actualTickLead = (int)LastAckedInputTick - (int)LastServerWorldTick + 1;
                this.clientSimulationAdjuster.NotifyActualTickLead(actualTickLead, false, this.ServerVars.Get<bool>("sv_agressive_lag_reduction", true));
            }

            // For debugging purposes, log the local lead we're running at
            var localWorldTickLead = this.WorldTick - this.LastServerWorldTick;
            Logger.SetDebugUI("cl_local_tick", localWorldTickLead.ToString());

            PlayerState incomingLocalPlayerState = new PlayerState();
            if (incomingState.PlayerStates != null)
            {
                foreach (var playerState in incomingState.PlayerStates)
                {
                    if (playerState.NetworkId == this.LocalPlayer.NetworkId)
                    {
                        incomingLocalPlayerState = playerState;
                    }
                    else
                    {
                        if (this._players.ContainsKey(playerState.NetworkId))
                        {
                            var puppet = this._players[playerState.NetworkId];
                            puppet.ApplyNetworkState(playerState);
                        }
                    }
                }
            }

            if (default(PlayerState).Equals(incomingLocalPlayerState))
            {
                // This is unexpected.
                Logger.LogDebug(this, "No local player state found!");
            }

            if (incomingState.WorldTick >= WorldTick)
            {
                // We're running behind the server at this point, which can happen
                // if the application is suspended for some reason, so just snap our 
                // state.
                // TODO: Look into interpolation here as well.
                Logger.LogDebug(this, "Got a future world state, snapping to latest state.");
                // TODO: We need to add local estimated latency here like we do during init.
                WorldTick = incomingState.WorldTick;
                LocalPlayer.ApplyNetworkState(incomingLocalPlayerState);
                return;
            }

            //test
            uint bufidx = incomingState.WorldTick % 1024;
            var stateSnapshot = LocalPlayerStateSnapshots[bufidx];

            var incomingPosition = incomingLocalPlayerState.GetVar<Vector3>(LocalPlayer, "NetworkPosition", Vector3.Zero);
            var snapshotPosition = stateSnapshot.GetVar<Vector3>(LocalPlayer, "NetworkPosition", Vector3.Zero);

            var error = incomingPosition - snapshotPosition;


            if (error.LengthSquared() > 0.0001f)
            {
                Logger.LogDebug(this, $"Rewind tick#{incomingState.WorldTick}, Error: {error.Length()}, Range: {WorldTick - incomingState.WorldTick}");
                replayedStates++;

                // TODO: If the error was too high, snap rather than interpolate.

                // Rewind local player state to the correct state from the server.
                // TODO: Cleanup a lot of this when its merged with how rockets are spawned.
                LocalPlayer.ApplyNetworkState(incomingLocalPlayerState);

                // Loop through and replay all captured input snapshots up to the current tick.
                uint replayTick = incomingState.WorldTick;
                while (replayTick < WorldTick)
                {
                    // Grab the historical input.
                    bufidx = replayTick % 1024;
                    var inputSnapshot = LocalPlayerInputsSnapshots[bufidx];

                    // Rewrite the historical sate snapshot.
                    LocalPlayerStateSnapshots[bufidx] = LocalPlayer.ToNetworkState();

                    // Apply inputs to the associated player controller and simulate the world.
                    var input = LocalPlayer.Components.Get<NetworkInput>();
                    input?.SetPlayerInputs(inputSnapshot);

                    this.SimulateWorld((float)this.GetPhysicsProcessDeltaTime());
                    ++replayTick;
                }
            }
        }

        /// <summary>
        internal void ProcessLocalInput()
        {
            float simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
            var serverSendRate = simTickRate / 2;

            var MaxStaleServerStateTicks = (int)MathF.Ceiling(this.ServerVars.Get<int>("sv_max_stages_ms", 500) / serverSendRate);

            GeneralPlayerInput inputs = new GeneralPlayerInput();

            var inputComponent = LocalPlayer.Components.Get<NetworkInput>();
            var lastTicks = this.WorldTick - this.LastServerWorldTick;
            if (this.ServerVars.Get<bool>("sv_freze_client", false) && lastTicks >= MaxStaleServerStateTicks)
            {
                Logger.LogDebug(this, "Server state is too old (is the network connection dead?) - max ticks " + MaxStaleServerStateTicks + " - currentTicks => " + lastTicks);
                inputs = new GeneralPlayerInput();
            }
            else if (inputComponent != null)
            {
                if (!this.GameInstance.GuiDisableInput)
                {
                    inputs = inputComponent.GetInput();
                }
                else
                {
                    inputs.ViewDirection = inputComponent.LastInput.ViewDirection;
                    inputs.Apply(inputComponent.AvaiableInputs, new Dictionary<string, bool>());
                }
            }

            // Update our snapshot buffers.
            uint bufidx = this.WorldTick % NetworkWorld.MaxTicks;
            this.LocalPlayerInputsSnapshots[bufidx] = inputs;
            this.LocalPlayerStateSnapshots[bufidx] = LocalPlayer.ToNetworkState();
            this.LocalPlayerWorldTickSnapshots[bufidx] = this.LastServerWorldTick;

            // Send a command for all inputs not yet acknowledged from the server.
            var unackedInputs = new List<GeneralPlayerInput>();
            var clientWorldTickDeltas = new List<short>();

            // TODO: lastServerWorldTick is technically not the same as lastAckedInputTick, fix this.
            for (uint tick = this.LastServerWorldTick; tick <= this.WorldTick; ++tick)
            {
                unackedInputs.Add(this.LocalPlayerInputsSnapshots[tick % NetworkWorld.MaxTicks]);
                clientWorldTickDeltas.Add((short)(tick - this.LocalPlayerWorldTickSnapshots[tick % NetworkWorld.MaxTicks]));
            }

            var command = new PlayerInputCommand
            {
                StartWorldTick = this.LastServerWorldTick,
                Inputs = unackedInputs.ToArray(),
                ClientWorldTickDeltas = clientWorldTickDeltas.ToArray(),
            };

            // send to server => command
            this.netService.SendMessageSerialisable(this.netService.ServerPeer.Id, command, LiteNetLib.DeliveryMethod.Sequenced);

            // SetPlayerInputs
            if (inputComponent != null)
                inputComponent.SetPlayerInputs(inputs);
        }
    }
}
