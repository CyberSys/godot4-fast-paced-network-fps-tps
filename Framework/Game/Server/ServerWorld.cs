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

using System.Linq;
using System;
using System.Collections.Generic;
using LiteNetLib;
using Framework.Utils;
using Framework.Network.Commands;
using Framework.Network;
using Framework.Network.Services;
using Framework.Input;
using Framework.Physics;
using Godot;

namespace Framework.Game.Server
{
    /// <summary>
    /// Base class for an server world
    /// </summary>
    public class ServerWorld : World
    {

        /// <summary>
        /// Time after player totaly deleted (0 means directly)
        /// </summary>
        public float deleteTimeForPlayer = 10;

        /// <summary>
        /// The network service for the server world
        /// </summary>
        /// <value></value>
        public ServerNetworkService netService { get; set; } = null;

        /// <inheritdoc />
        private FixedTimer worldStateBroadcastTimer;

        /// <inheritdoc />
        private HashSet<int> unprocessedPlayerIds = new HashSet<int>();

        /// <inheritdoc />
        private PlayerInputProcessor playerInputProcessor = new PlayerInputProcessor();

        /// <inheritdoc />
        private int missedInputs;

        /// <inheritdoc />
        internal override void InternalTreeEntered()
        {
            base.InternalTreeEntered();

            this.netService = this.gameInstance.Services.Get<ServerNetworkService>();
            this.netService.ClientConnected += this.OnPlayerConnected;
            this.netService.ClientDisconnect += this.OnPlayerDisconnect;

            this.netService.SubscribeSerialisable<ServerInitializer>(this.InitializeClient);
            this.netService.SubscribeSerialisable<PlayerInputCommand>(this.OnPlayerInput);

            this.netService.ClientLatencyUpdate += (clientId, latency) =>
            {
                if (this._players.ContainsKey(clientId))
                {
                    this._players[clientId].Latency = latency;
                }
            };


            float SimulationTickRate = 1 / (float)this.GetPhysicsProcessDeltaTime();
            float ServerSendRate = SimulationTickRate / 2;

            worldStateBroadcastTimer = new FixedTimer(ServerSendRate, BroadcastWorldHearbeat);
            worldStateBroadcastTimer.Start();
        }

        /// <summary>
        /// Delete an player from the server world
        /// </summary>
        /// <param name="clientId">player id</param>
        /// <param name="withDelay">use an delay for deletion</param>
        public void DeletePlayer(int clientId, bool withDelay = true)
        {
            if (this._players.ContainsKey(clientId))
            {
                var serverPlayer = this._players[clientId] as ServerPlayer;

                serverPlayer.PreviousState = serverPlayer.State;
                serverPlayer.State = PlayerConnectionState.Disconnected;

                if (withDelay)
                {
                    serverPlayer.DisconnectTime = deleteTimeForPlayer;
                    this.ActiveGameRule?.OnPlayerLeaveTemporary(serverPlayer);
                }
                else
                {
                    serverPlayer.QueueFree();
                    this._players.Remove(clientId);
                    this.ActiveGameRule?.OnPlayerLeave(serverPlayer);
                }
            }
        }

        /// <summary>
        /// Add an player to the server world
        /// </summary>
        /// <param name="clientId"></param>
        /// <typeparam name="T">Type of the server player</typeparam>
        public void AddPlayer<T>(int clientId) where T : ServerPlayer
        {
            if (!this._players.ContainsKey(clientId))
            {
                T serverPlayer = Activator.CreateInstance(typeof(T)) as T;
                serverPlayer.Name = clientId.ToString();
                serverPlayer.Id = clientId;
                serverPlayer.GameWorld = this;
                serverPlayer.Team = PlayerTeam.SPECTATOR;
                serverPlayer.State = PlayerConnectionState.Connected;

                this.playerHolder.AddChild(serverPlayer);
                this._players.Add(clientId, serverPlayer);

                this.ActiveGameRule?.OnNewPlayerJoined(serverPlayer);
            }
            else
            {
                var serverPlayer = this._players[clientId] as ServerPlayer;
                serverPlayer.State = serverPlayer.PreviousState;

                this.ActiveGameRule?.OnPlayerRejoined(serverPlayer);
            }

            var message = new ClientWorldLoader();
            message.WorldName = ResourceWorldPath;
            message.WorldTick = this.WorldTick;

            this.netService.SendMessageSerialisable<ClientWorldLoader>(clientId, message);
        }

        /// <summary>
        /// Event called after client is connected to server
        /// </summary>
        /// <param name="clientId"></param>
        public virtual void OnPlayerConnected(int clientId)
        {
            Logger.LogDebug(this, "[" + clientId + "] Connected");
            this.AddPlayer<ServerPlayer>(clientId);
        }

        /// <summary>
        /// Event called after client is disconnected from server
        /// </summary>
        /// <param name="clientId">he client id of the network player</param>
        /// <param name="reason">The reason why the player are disconnted</param>
        public virtual void OnPlayerDisconnect(int clientId, DisconnectReason reason)
        {
            Logger.LogDebug(this, "[" + clientId + "] Disconnected");
            this.DeletePlayer(clientId, true);
        }

        /// <summary>
        /// Enqueue new player input
        /// </summary>
        /// <param name="package"></param>
        /// <param name="peer"></param>
        private void OnPlayerInput(PlayerInputCommand package, NetPeer peer)
        {
            var clientId = peer.Id;
            if (this._players.ContainsKey(clientId))
            {
                var player = this._players[clientId] as ServerPlayer;
                package.Inputs = package.Inputs.Select(df => df.DeserliazeWithInputKeys(player.InputProcessor.AvaiableInputs)).ToArray();
                playerInputProcessor.EnqueueInput(package, clientId, this.WorldTick);
                player.LatestInputTick = package.StartWorldTick + (uint)package.Inputs.Length - 1;
            }
        }

        /// <summary>
        /// Send an heartbeat to all players
        /// Hearbeat contains player informations, server latency, states, etc
        /// </summary>
        public void BroadcastWorldHearbeat(float dt)
        {
            //get player states
            var states = new List<PlayerState>();
            foreach (var client in this._players.Where(df => df.Value is NetworkPlayer).Select(df => df.Value as NetworkPlayer).ToArray())
            {
                states.Add(client.ToNetworkState());
            }

            //get player updates
            var heartbeatUpdateList = this._players.Where(df => df.Value.IsServer()).
                                Select(df => new PlayerUpdate
                                {
                                    Id = df.Key,
                                    Team = df.Value.Team,
                                    State = df.Value.State,
                                    RequiredComponents = (df.Value as ServerPlayer).RequiredComponents.ToArray(),
                                    RequiredPuppetComponents = (df.Value as ServerPlayer).RequiredPuppetComponents.ToArray(),
                                    Latency = df.Value.Latency
                                }).ToArray();


            //send to each player data package
            foreach (var player in this._players.Where(df => df.Value.IsServer()).Select(df => df.Value as ServerPlayer).Where(df => df.State == PlayerConnectionState.Initialized).ToArray())
            {
                var cmd = new WorldHeartbeat
                {
                    WorldTick = WorldTick,
                    YourLatestInputTick = player.LatestInputTick,
                    PlayerStates = states.ToArray(),
                    PlayerUpdates = heartbeatUpdateList.ToArray(),
                };

                this.netService.SendMessageSerialisable<WorldHeartbeat>(player.Id, cmd, LiteNetLib.DeliveryMethod.Sequenced);
            }
        }

        /// <inheritdoc />  
        internal override void InternalProcess(float delta)
        {
            base.InternalProcess(delta);

            //check if players are realy disconnected and delete them totaly
            foreach (var player in this._players.Where(df => df.Value.State == PlayerConnectionState.Disconnected).ToArray())
            {
                var serverPlayer = player.Value as ServerPlayer;
                if (serverPlayer.DisconnectTime <= 0)
                {
                    this.DeletePlayer(player.Key, false);
                }
                else
                {
                    serverPlayer.DisconnectTime -= delta;
                }
            }
        }

        private void InitializeClient(ServerInitializer package, NetPeer peer)
        {
            var clientId = peer.Id;

            if (this._players.ContainsKey(clientId))
            {
                var player = this._players[clientId];
                Logger.LogDebug(this, "[" + clientId + "] " + " Initialize player with previous state: " + player.State.ToString());

                var oldState = player.State;
                if (oldState != PlayerConnectionState.Initialized)
                {
                    player.State = PlayerConnectionState.Initialized;
                    this.OnPlayerInitilaized(player);
                }

                this.netService.SendMessageSerialisable<ClientWorldInitializer>(clientId,
                            new ClientWorldInitializer
                            {
                                PlayerId = clientId,
                                ServerVars = this.ServerVars.Vars,
                                GameTick = this.WorldTick
                            });
            }
        }

        internal void ProcessPlayerAttack(ServerPlayer player, float range = 1000)
        {
            if (player.State != PlayerConnectionState.Initialized)
                return;

            //spawn the object
            //    var obj = networkObjectManager.SpawnPlayerObject(0, type, position, orientation);

            Logger.LogDebug(this, "Get player attack for player " + player.Id);

            // First, rollback the state of all attackable entities (for now just players).
            // The world is not rolled back to the tick the players input was for, but rather
            // the tick of the server world state the player was seeing at the time of attack.
            // TODO: Clean up the whole player delegate path, it sucks.
            var remoteViewTick = player.CurrentPlayerInput.RemoteViewTick;

            // If client interp is enabled, we estimate by subtracting another tick, but I'm not sure
            // if this is correct or not, needs more work.
            if (this.ServerVars.Get<bool>("sv_interpolate", true))
            {
                remoteViewTick--;
            }

            uint bufidx = remoteViewTick % 1024;
            var head = new Dictionary<int, PlayerState>();
            foreach (var entry in this.Players)
            {
                var otherPlayer = entry.Value as ServerPlayer;
                head[entry.Key] = otherPlayer.ToNetworkState();
                var historicalState = otherPlayer.States[bufidx];
                otherPlayer.ApplyNetworkState(historicalState);
            }

            var currentState = player.ToNetworkState();


            Physics.Commands.MovementNetworkCommand currentMovementState = new Physics.Commands.MovementNetworkCommand();
            foreach (var component in player.Components.All)
            {
                if (component is IChildMovementNetworkSyncComponent)
                {
                    currentMovementState = (component as IChildMovementNetworkSyncComponent).GetNetworkState();
                }
            }

            // Now check for collisions.
            var playerObjectHit = this.CheckHit(player, currentMovementState, range);

            // Debugging.
            foreach (var entry in this.Players)
            {
                var otherPlayer = entry.Value as ServerPlayer;
                if (otherPlayer.Id != player.Id)
                {
                    // var component = otherPlayer.Components.Get
                    //  Logger.LogDebug(this, $"Other player at ${otherPlayer.GlobalTransform.origin} for remote view tick ${remoteViewTick}");
                }
            }

            // Finally, revert all the players to their head state.
            foreach (var entry in this.Players)
            {
                var otherPlayer = entry.Value as ServerPlayer;
                otherPlayer.ApplyNetworkState(head[entry.Key]);
            }

            // Apply the result of the this.
            if (playerObjectHit != null)
            {
                Logger.LogDebug(this, "Registering authoritative player hit");
                //attack.AddForceToPlayer(playerObjectHit.GetComponent<CPMPlayerController>());
                // return true;

            }
            else
            {
                Logger.LogDebug(this, "Cant found player hit");
            }

            //   return false;
        }

        internal object CheckHit(ServerPlayer player, Physics.Commands.MovementNetworkCommand command, float range)
        {
            var currentTransform = new Godot.Transform3D(command.Rotation, command.Position);
            var attackPosition = currentTransform.origin + Vector3.Up * player.PlayerHeadHeight + currentTransform.basis.x * 0.2f;
            var attackTransform = new Godot.Transform3D(command.Rotation, attackPosition);

            var raycast = new PhysicsRayQueryParameters3D();
            raycast.From = attackTransform.origin;
            raycast.To = attackTransform.origin + -attackTransform.basis.z * range;

            var result = GetWorld3d().DirectSpaceState.IntersectRay(raycast);
            if (result != null && result.Contains("position"))
            {
                if (this.ServerVars.Get<bool>("sv_raycast", true))
                {
                    var raycastHit = new RaycastTest { from = attackTransform.origin, to = (Vector3)result["position"] };
                    Logger.LogDebug(this, "Found raycast at " + raycastHit.from + " => " + raycastHit.to);
                    this.netService.SentMessageToAllSerialized<RaycastTest>(raycastHit);
                }

            }
            return result;
        }

        /// <inheritdoc />  
        internal override void InternalTick(float interval)
        {
            this._activeGameRule?.Tick(interval);

            var now = DateTime.Now;

            // Apply inputs to each player.
            unprocessedPlayerIds.Clear();
            unprocessedPlayerIds.UnionWith(this.Players.Where(df => df.Value.State ==
            PlayerConnectionState.Initialized && df.Value.IsServer()).Select(df => df.Key).ToArray());

            var tickInputs = this.playerInputProcessor.DequeueInputsForTick(WorldTick);

            foreach (var tickInput in tickInputs)
            {
                if (this._players.ContainsKey(tickInput.PlayerId))
                {
                    var serverPlayer = this._players[tickInput.PlayerId] as ServerPlayer;

                    //decompose but with what?

                    ///IChildInputComponent
                    serverPlayer.SetPlayerInputs(tickInput.Inputs);
                    serverPlayer.CurrentPlayerInput = tickInput;
                    unprocessedPlayerIds.Remove(tickInput.PlayerId);

                    // Mark the player as synchronized.
                    serverPlayer.IsSynchronized = true;
                }
            }

            // Any remaining players without inputs have their latest input command repeated,
            // but we notify them that they need to fast-forward their simulation to improve buffering.
            foreach (var playerId in unprocessedPlayerIds)
            {
                // If the player is not yet synchronized, this isn't an error.
                if (!this._players.ContainsKey(playerId) ||
                    !(this._players[playerId] as ServerPlayer).IsSynchronized)
                {
                    continue;
                }

                var serverPlayer = this._players[playerId] as ServerPlayer;
                if (serverPlayer != null)
                {
                    ++missedInputs;
                    Logger.SetDebugUI("sv missed inputs", missedInputs.ToString());

                    TickInput latestInput;
                    if (playerInputProcessor.TryGetLatestInput(playerId, out latestInput))
                    {
                        serverPlayer.SetPlayerInputs(latestInput.Inputs);
                    }
                    else
                    {
                        Logger.LogDebug(this, $"No inputs for player #{playerId} and no history to replay.");
                    }
                }
            }

            // Advance the world simulation.
            SimulateWorld(interval);

            //increase the world tick
            ++WorldTick;

            // Snapshot everything.
            var bufidx = WorldTick % 1024;

            foreach (var player in this._players.Where(df => df.Value.State == PlayerConnectionState.Initialized && df.Value.IsServer())
            .Select(df => df.Value as ServerPlayer).ToArray())
            {
                player.States[bufidx] = player.ToNetworkState();
            }

            // Update post-tick timers.
            worldStateBroadcastTimer.Update(interval);

            this.Tick(interval);
        }

        /// <inheritdoc />
        internal override void OnLevelInternalAddToScene()
        {
            applyGlow(false);
            applySDFGI(false);
            applySSAO(false);
            applySSIL(false);

            base.OnLevelInternalAddToScene();
        }

        /// <inheritdoc />
        private void SimulateWorld(float dt)
        {
            foreach (var player in this._players.
                Where(df => df.Value.State == PlayerConnectionState.Initialized).
                Select(df => df.Value).ToArray())
            {
                (player as Player).InternalTick(dt);
            }
        }

        private IGameRule _activeGameRule = null;

        /// <summary>
        /// Set or get the active game rule
        /// </summary>
        public IGameRule ActiveGameRule
        {
            get
            {
                return _activeGameRule;
            }
            set
            {
                this.activateGameRule(value);
            }
        }

        /// <inheritdoc />
        internal override void Init(VarsCollection serverVars, uint initalWorldTick)
        {
            base.Init(serverVars, initalWorldTick);

            this.ServerVars.OnChange += (key, value) =>
            {
                foreach (var player in Players.Where(df => df.Value.State == PlayerConnectionState.Initialized))
                {
                    this.netService.SendMessageSerialisable<ServerVarUpdate>(player.Key,
                                       new ServerVarUpdate
                                       {
                                           ServerVars = this.ServerVars.Vars,
                                           GameTick = this.WorldTick
                                       });
                }
            };
        }

        private void activateGameRule(IGameRule rule)
        {
            Logger.LogDebug(this, "Activate game rule " + rule.GetType().Name.ToString());

            this._activeGameRule = rule;
            foreach (var player in _players.Where(df => df.Value is Player).Select(df => df.Value as Player))
            {
                //clear previous components
                player.Components.Clear();
                (player as ServerPlayer).RequiredPuppetComponents = new int[0];
                (player as ServerPlayer).RequiredComponents = new int[0];

                if (this._activeGameRule != null)
                {
                    rule.OnNewPlayerJoined(player);
                }
            }
        }
    }
}
