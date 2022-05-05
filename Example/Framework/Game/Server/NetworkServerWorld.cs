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
using Godot;

namespace Framework.Game.Server
{
    /// <summary>
    /// Base class for an server world
    /// </summary>
    public partial class NetworkServerWorld : NetworkWorld
    {
        /// <summary>
        /// Time after player totaly deleted (0 means directly)
        /// </summary>
        [Export]
        public float DeleteTimeForPlayer = 10;

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

            this.netService = this.GameInstance.Services.Get<ServerNetworkService>();
            this.netService.ClientConnected += this.OnPlayerConnectedInternal;
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
                var serverPlayer = this._players[clientId];

                serverPlayer.PreviousState = serverPlayer.State;
                serverPlayer.State = PlayerConnectionState.Disconnected;

                if (withDelay)
                {
                    serverPlayer.DisconnectTime = DeleteTimeForPlayer;
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
        /// <param name="clientId">The id of the new client</param>
        /// <param name="resourcePath">The resource path (.res) to the scene package</param>
        /// <param name="scriptPath">The script path used by the scene</param>
        public void AddPlayer(int clientId, string resourcePath, string scriptPath)
        {
            GD.Load<CSharpScript>(scriptPath);

            Framework.Utils.AsyncLoader.Loader.LoadResource(resourcePath, (res) =>
            {
                if (!this._players.ContainsKey(clientId))
                {
                    NetworkCharacter serverPlayer = (res as PackedScene).Instantiate<NetworkCharacter>();
                    serverPlayer.Mode = NetworkMode.SERVER;
                    serverPlayer.Name = clientId.ToString();
                    serverPlayer.ResourcePath = resourcePath;
                    serverPlayer.ScriptPath = scriptPath;
                    serverPlayer.Id = clientId;
                    serverPlayer.GameWorld = this;
                    serverPlayer.State = PlayerConnectionState.Connected;

                    this.playerHolder.AddChild(serverPlayer);
                    this._players.Add(clientId, serverPlayer);

                    this.ActiveGameRule?.OnNewPlayerJoined(serverPlayer);
                }
                else
                {
                    var serverPlayer = this._players[clientId];
                    serverPlayer.State = serverPlayer.PreviousState;

                    this.ActiveGameRule?.OnPlayerRejoined(serverPlayer);
                }

                var message = new ClientWorldLoader();
                message.WorldName = ResourceWorldPath;
                message.ScriptPath = ResourceWorldScriptPath;
                message.WorldTick = this.WorldTick;

                this.netService.SendMessageSerialisable<ClientWorldLoader>(clientId, message);
            });
        }

        /// <summary>
        /// Event called after client is connected to server
        /// </summary>
        /// <param name="clientId"></param>
        public virtual void OnPlayerConnected(int clientId)
        {
        }

        internal void OnPlayerConnectedInternal(int clientId)
        {
            Logger.LogDebug(this, "[" + clientId + "] Connected");
            this.OnPlayerConnected(clientId);
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
                var player = this._players[clientId];
                var input = player.Components.Get<NetworkInput>();
                if (input != null)
                {
                    package.Inputs = package.Inputs.Select(df => df.DeserliazeWithInputKeys(input.InputProcessor.AvaiableInputs)).ToArray();
                    playerInputProcessor.EnqueueInput(package, clientId, this.WorldTick);
                    player.LatestInputTick = package.StartWorldTick + (uint)package.Inputs.Length - 1;
                }
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
            foreach (var client in this._players.Where(df => df.Value.IsServer()).Select(df => df.Value).ToArray())
            {
                states.Add(client.ToNetworkState());
            }

            //get player updates
            var heartbeatUpdateList = this._players.Where(df => df.Value.IsServer()).
                                Select(df => new PlayerUpdate
                                {
                                    Id = df.Key,
                                    State = df.Value.State,
                                    ResourcePath = df.Value.ResourcePath,
                                    ScriptPath = df.Value.ScriptPath,
                                    RequiredComponents = (df.Value).RequiredComponents.ToArray(),
                                    RequiredPuppetComponents = (df.Value).RequiredPuppetComponents.ToArray(),
                                    Latency = df.Value.Latency
                                }).ToArray();

            //send to each player data package
            foreach (var player in this._players.Where(df => df.Value.IsServer()).Select(df => df.Value).Where(df => df.State == PlayerConnectionState.Initialized).ToArray())
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
                var serverPlayer = player.Value;
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

        internal void ProcessPlayerAttack(NetworkCharacter player, float range = 1000)
        {
            if (player.State != PlayerConnectionState.Initialized)
                return;

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

            uint bufidx = remoteViewTick % MaxTicks;
            var head = new Dictionary<int, PlayerState>();
            foreach (var entry in this.Players)
            {
                var otherPlayer = entry.Value;
                head[entry.Key] = otherPlayer.ToNetworkState();
                var historicalState = otherPlayer.States[bufidx];
                otherPlayer.ApplyNetworkState(historicalState);
            }

            var currentState = player.ToNetworkState();

            // Now check for collisions.
            var playerObjectHit = player.DetechtHit(range);
            if (playerObjectHit != null)
            {
                if (this.ServerVars.Get<bool>("sv_raycast", true))
                {
                    var raycastHit = new RaycastTest { from = playerObjectHit.From, to = playerObjectHit.To };
                    Logger.LogDebug(this, "Found raycast at " + raycastHit.from + " => " + raycastHit.to);
                    this.netService.SentMessageToAllSerialized<RaycastTest>(raycastHit);
                }

                if (playerObjectHit.PlayerDestination != null
                    && playerObjectHit.PlayerDestination is NetworkCharacter)
                {
                    Logger.LogDebug(this, $"Player ${player.Id} for remote view tick ${remoteViewTick} was hit Player ${playerObjectHit.PlayerDestination.Id}");
                    (playerObjectHit.PlayerDestination as NetworkCharacter).OnHit(playerObjectHit);
                }
                else if (playerObjectHit.Collider != null)
                {
                    Logger.LogDebug(this, $"Player ${player.Id} for remote view tick ${remoteViewTick} was hit ${playerObjectHit.Collider.Name}");
                }

                ActiveGameRule?.OnHit(playerObjectHit);
            }

            // Finally, revert all the players to their head state.
            foreach (var entry in this.Players)
            {
                var otherPlayer = entry.Value;
                otherPlayer.ApplyNetworkState(head[entry.Key]);
            }
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
                    var serverPlayer = this._players[tickInput.PlayerId];

                    //decompose but with what?
                    var input = serverPlayer.Components.Get<NetworkInput>();
                    if (input != null)
                    {
                        input.SetPlayerInputs(tickInput.Inputs);
                    }

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
                    !(this._players[playerId]).IsSynchronized)
                {
                    continue;
                }

                var serverPlayer = this._players[playerId];
                if (serverPlayer != null)
                {
                    ++missedInputs;
                    Logger.SetDebugUI("sv missed inputs", missedInputs.ToString());

                    TickInput latestInput;
                    if (playerInputProcessor.TryGetLatestInput(playerId, out latestInput))
                    {
                        var input = serverPlayer.Components.Get<NetworkInput>();
                        if (input != null)
                        {
                            input.SetPlayerInputs(latestInput.Inputs);
                        }
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
            var bufidx = WorldTick % MaxTicks;

            foreach (var player in this._players.Where(df => df.Value.State == PlayerConnectionState.Initialized && df.Value.IsServer())
            .Select(df => df.Value).ToArray())
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
                player.InternalTick(dt);
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
            private set
            {
                this.activateGameRule(value);
            }
        }


        /// <summary>
        /// Create and init a new game rule
        /// </summary>
        /// <param name="name"></param>
        /// <typeparam name="T"></typeparam>
        public void SetGameRule<T>(string name) where T : GameRule
        {
            T gameRule = (T)Activator.CreateInstance(typeof(T));

            gameRule.GameWorld = this;
            gameRule.RuleName = name;

            this.ActiveGameRule = gameRule;
        }

        /// <inheritdoc />
        internal override void Init(VarsCollection serverVars, uint initalWorldTick)
        {
            base.Init(serverVars, initalWorldTick);

            this.ServerVars.OnChange += (state, key, value) =>
            {
                foreach (var player in Players.Where(df => df.Value.State == PlayerConnectionState.Initialized))
                {
                    this.netService.SendMessageSerialisable<ServerVarUpdate>(player.Key,
                                       new ServerVarUpdate
                                       {
                                           ServerVars = this.ServerVars.Vars
                                       });
                }
            };
        }

        private void activateGameRule(IGameRule rule)
        {
            Logger.LogDebug(this, "Activate game rule: " + rule.GetType().Name);
            this._activeGameRule = rule;
            this._activeGameRule.OnGameRuleActivated();

            foreach (var player in _players.Select(df => df.Value))
            {
                //clear previous components
                player.Components.Clear();

                (player).RequiredPuppetComponents = new int[0];
                (player).RequiredComponents = new int[0];

                if (this._activeGameRule != null)
                {
                    rule.OnNewPlayerJoined(player);
                }
            }
        }
    }
}
