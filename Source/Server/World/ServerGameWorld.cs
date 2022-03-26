using System.Linq;
using Shooter.Server.Simulation;
using Shooter.Shared;
using Shooter.Server.Services;
using System;
using System.Collections.Generic;
using Shooter.Shared.Network.Packages;
using Shooter.Client.Simulation.Components;
using LiteNetLib;

namespace Shooter.Server.World
{
	public partial class ServerGameWorld : GameWorld
	{
		private const float deleteTimeForPlayer = 10;

		private ServerGameLogic serverLogic;
		private ServerNetworkService netService = null;
		private FixedTimer worldStateBroadcastTimer;
		private HashSet<int> unprocessedPlayerIds = new HashSet<int>();
		public PlayerInputProcessor playerInputProcessor = new PlayerInputProcessor();

		private int missedInputs;

		public string worldPath = null;

		/// <summary>
		/// Instancing the server game world
		/// </summary>
		public override void _EnterTree()
		{
			base._EnterTree();

			this.serverLogic = this.gameInstance as ServerGameLogic;
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
		/// Event called after client is disconnected from server
		/// </summary>
		/// <param name="clientId"></param>
		private void OnPlayerDisconnect(int clientId, DisconnectReason reason)
		{
			if (this._players.ContainsKey(clientId))
			{
				var serverPlayer = this._players[clientId] as ServerPlayer;

				serverPlayer.PreviousState = serverPlayer.State;
				serverPlayer.State = ClientState.Disconnected;
				serverPlayer.DisconnectTime = deleteTimeForPlayer;
			}
		}

		/// <summary>
		/// Event called after client is connected to server
		/// </summary>
		/// <param name="clientId"></param>
		private void OnPlayerConnected(int clientId)
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
				var serverPlayer = this._players[clientId] as ServerPlayer;
				serverPlayer.State = serverPlayer.PreviousState;
			}

			var message = new Shared.Network.Packages.ClientWorldInitializer();
			message.WorldName = worldPath;
			message.WorldTick = this.WorldTick;

			this.netService.SendMessageSerialisable<Shared.Network.Packages.ClientWorldInitializer>(clientId, message);
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
				playerInputProcessor.EnqueueInput(package, clientId, this.WorldTick);

				if (this._players.ContainsKey(clientId))
				{
					var player = this._players[clientId] as ServerPlayer;
					player.latestInputTick = package.StartWorldTick + (uint)package.Inputs.Length - 1;
				}
			}
		}

		/// <summary>
		/// Send an heartbeat to all players
		/// Hearbeat contains player informations, server latency, states, etc
		/// </summary>
		private void BroadcastWorldHearbeat(float dt)
		{
			//get player states
			var states = new List<PlayerState>();
			foreach (var client in this._players.ToArray())
			{
				if (client.Value.Simulation != null)
				{
					states.Add(client.Value.Simulation.ToNetworkState());
				}
			}

			//get player updates
			var heartbeatUpdateList = this._players.
								Select(df => new PlayerUpdate
								{
									Id = df.Key,
									Team = df.Value.Team,
									State = df.Value.State,
									Latency = df.Value.Latency
								}).ToArray();

			//send to each player data package
			foreach (var player in this._players.ToArray())
			{
				var serverPlayer = player.Value as ServerPlayer;

				var cmd = new WorldHeartbeat
				{
					WorldTick = WorldTick,
					YourLatestInputTick = serverPlayer.latestInputTick,
					PlayerStates = states.ToArray(),
					PlayerUpdates = heartbeatUpdateList,
				};

				this.netService.SendMessage<WorldHeartbeat>(player.Key, cmd, LiteNetLib.DeliveryMethod.Sequenced);
			}
		}

		public override void _Process(float delta)
		{
			base._Process(delta);

			//check if players are realy disconnected and delete them totaly
			foreach (var player in this._players.Where(df => df.Value.State == ClientState.Disconnected).ToArray())
			{
				var serverPlayer = player.Value as ServerPlayer;
				if (serverPlayer.DisconnectTime <= 0)
				{
					//Free spawn point
					var spawnpoint = serverPlayer.SpawnPoint;
					if (spawnpoint != null)
					{
						spawnpoint.inUsage = false;
					}

					//free player simulation node
					if (serverPlayer.Simulation != null)
					{
						this.RemovePlayerSimulation(serverPlayer.Simulation);
						serverPlayer.Simulation = null;
					}

					var id = player.Key;
					this._players.Remove(id);
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
				var oldState = this._players[clientId].State;
				if (oldState != ClientState.Initialized)
				{
					Logger.LogDebug(this, "[" + clientId + "] " + " Initialize player.");

					var serverPlayer = this._players[clientId] as ServerPlayer;
					if (serverPlayer.Simulation == null)
					{
						var simulation = this.AddPlayerSimulation<ServerPlayerSimulation>(clientId);
						serverPlayer.Simulation = simulation;

						var spawnPoint = serverPlayer.SpawnPoint;
						if (spawnPoint == null)
						{
							spawnPoint = this.Level.GetFreeSpawnPoint();
						}

						if (spawnPoint != null)
						{
							spawnPoint.inUsage = true;
							Logger.LogDebug(this, "[" + clientId + "] " + " Set spawnpoint to " + spawnPoint.Transform.origin);

							var component = simulation.Components.AddComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
							var playerServerCamera = simulation.Components.AddComponent<PlayerCameraComponent>();
							playerServerCamera.cameraMode = CameraMode.Server;

							var gt = component.GlobalTransform;
							gt.origin = spawnPoint.GlobalTransform.origin;
							component.GlobalTransform = gt;

							this._players[clientId].State = ClientState.Initialized;
							serverPlayer.SpawnPoint = spawnPoint;

							//  this.SendHeartbeatToPlayers();
						}
						else
						{
							Logger.LogDebug(this, "[" + clientId + "] " + " Cant find free spawn point");
						}
					}
				}
			}

			this.netService.SendMessageSerialisable<ClientInitializer>(clientId,
						new ClientInitializer
						{
							PlayerId = clientId,
							ServerVars = (this.gameInstance as ServerGameLogic).ServerVars,
							GameTick = this.WorldTick
						});
		}

		public override void Tick(float interval)
		{
			var now = DateTime.Now;

			// Apply inputs to each player.
			unprocessedPlayerIds.Clear();
			unprocessedPlayerIds.UnionWith(this.Players.Where(df => df.Value.State == ClientState.Initialized).Select(df => df.Key).ToArray());

			var tickInputs = this.playerInputProcessor.DequeueInputsForTick(WorldTick);

			foreach (var tickInput in tickInputs)
			{
				if (this._players.ContainsKey(tickInput.PlayerId))
				{
					var serverPlayer = this._players[tickInput.PlayerId] as ServerPlayer;
					if (serverPlayer.Simulation != null)
					{
						serverPlayer.Simulation.SetPlayerInputs(tickInput.Inputs);
						serverPlayer.currentPlayerInput = tickInput;
						unprocessedPlayerIds.Remove(tickInput.PlayerId);

						// Mark the player as synchronized.
						serverPlayer.synchronized = true;
					}
				}
			}

			// Any remaining players without inputs have their latest input command repeated,
			// but we notify them that they need to fast-forward their simulation to improve buffering.
			foreach (var playerId in unprocessedPlayerIds)
			{
				// If the player is not yet synchronized, this isn't an error.
				if (!this._players.ContainsKey(playerId) ||
					!(this._players[playerId] as ServerPlayer).synchronized)
				{
					continue;
				}

				var player = this.playerHolder.GetNodeOrNull<ServerPlayerSimulation>(playerId.ToString());
				if (player != null)
				{
					++missedInputs;
					Logger.SetDebugUI("sv missed inputs", missedInputs.ToString());

					TickInput latestInput;
					if (playerInputProcessor.TryGetLatestInput(playerId, out latestInput))
					{
						player.SetPlayerInputs(latestInput.Inputs);
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

			foreach (var player in this._players.ToArray())
			{
				var serverPlayer = player.Value as ServerPlayer;
				if (serverPlayer.Simulation != null)
				{
					serverPlayer.states[bufidx] = serverPlayer.Simulation.ToNetworkState();
				}
			}

			// Update post-tick timers.
			worldStateBroadcastTimer.Update(interval);
		}

		public void SimulateWorld(float dt)
		{
			foreach (var player in this._players.ToArray())
			{
				var serverPlayer = player.Value as ServerPlayer;
				if (serverPlayer.Simulation != null)
				{
					serverPlayer.Simulation.Simulate(dt);
				}
			}
		}

	}
}
