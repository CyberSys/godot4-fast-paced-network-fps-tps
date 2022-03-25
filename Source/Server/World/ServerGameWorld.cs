using System.Linq;
using Godot;
using Shooter.Server.Simulation;
using Shooter.Shared;
using Shooter.Server.Services;
using System;
using System.Collections.Generic;
using Shooter.Shared.Network.Packages;

namespace Shooter.Server
{
	public class PlayerConnectionInfo
	{
		// Whether the player is synchronized yet.
		public bool synchronized;

		// The latest (highest) input tick from the player.
		public uint latestInputTick;
	}

	public partial class ServerGameWorld : GameWorld
	{
		private ServerGameLogic serverLogic;
		protected ServerNetworkService netService = null;
		private FixedTimer worldStateBroadcastTimer;

		private HashSet<int> unprocessedPlayerIds = new HashSet<int>();

		// Snapshot buffers for player state history, used for attack rollbacks.
		private Dictionary<int, PlayerStatePackage[]> playerStateSnapshots = new Dictionary<int, PlayerStatePackage[]>();

		// Current input struct for each player.
		// This is only needed because the ProcessAttack delegate flow is a bit too complicated.
		// TODO: Simplify this.
		private Dictionary<int, TickInput> currentPlayerInput = new Dictionary<int, TickInput>();

		private Dictionary<int, PlayerConnectionInfo> playerConnectionInfo = new Dictionary<int, PlayerConnectionInfo>();

		public PlayerInputProcessor playerInputProcessor = new PlayerInputProcessor();
		private int missedInputs;

		public override void _EnterTree()
		{
			base._EnterTree();

			this.serverLogic = this.gameInstance as ServerGameLogic;
			this.netService = this.gameInstance.Services.Get<ServerNetworkService>();

			float SimulationTickRate = 1 / (float)this.GetPhysicsProcessDeltaTime();
			float ServerSendRate = SimulationTickRate / 2;

			worldStateBroadcastTimer = new FixedTimer(ServerSendRate, BroadcastWorldState);
			worldStateBroadcastTimer.Start();
		}

		private void BroadcastWorldState(float dt)
		{
			List<PlayerStatePackage> states = new List<PlayerStatePackage>();
			foreach (var player in this.playerHolder.GetChildren())
			{
				if (player is PlayerSimulation)
				{
					states.Add((player as PlayerSimulation).ToNetworkState());
				}
			}

			foreach (var player in this.playerHolder.GetChildren())
			{
				if (player is PlayerSimulation)
				{
					var p = player as PlayerSimulation;
					var cmd = new WorldStatePackage
					{
						WorldTick = WorldTick,
						YourLatestInputTick = playerConnectionInfo[int.Parse(p.Name)].latestInputTick,
						PlayerStates = states.ToArray(),
					};

					this.netService.SendMessage<WorldStatePackage>(int.Parse(p.Name), cmd, LiteNetLib.DeliveryMethod.Sequenced);
				}
			}
		}

		public void EnqueneInput(int clientId, PlayerInputCommand package)
		{
			playerInputProcessor.EnqueueInput(package, clientId, this.WorldTick);
			playerConnectionInfo[clientId].latestInputTick = package.StartWorldTick + (uint)package.Inputs.Length - 1;
		}
		public override void Tick(float interval)
		{
			var now = DateTime.Now;

			// Apply inputs to each player.
			unprocessedPlayerIds.Clear();
			unprocessedPlayerIds.UnionWith(this.serverLogic.Players.Select(df => df.Key));
			var tickInputs = this.playerInputProcessor.DequeueInputsForTick(WorldTick);

			foreach (var tickInput in tickInputs)
			{
				var player = this.playerHolder.GetNodeOrNull<ServerPlayerSimulation>(tickInput.PlayerId.ToString());
				if (player != null)
				{
					player.SetPlayerInputs(tickInput.Inputs);
					currentPlayerInput[tickInput.PlayerId] = tickInput;
					unprocessedPlayerIds.Remove(tickInput.PlayerId);

					// Mark the player as synchronized.
					playerConnectionInfo[tickInput.PlayerId].synchronized = true;
				}
			}

			// Any remaining players without inputs have their latest input command repeated,
			// but we notify them that they need to fast-forward their simulation to improve buffering.
			foreach (var playerId in unprocessedPlayerIds)
			{
				// If the player is not yet synchronized, this isn't an error.
				if (!playerConnectionInfo.ContainsKey(playerId) ||
					!playerConnectionInfo[playerId].synchronized)
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

			++WorldTick;

			// Snapshot everything.
			var bufidx = WorldTick % 1024;

			foreach (var child in this.playerHolder.GetChildren())
			{
				if (child is ServerPlayerSimulation)
				{
					var simulation = child as ServerPlayerSimulation;
					playerStateSnapshots[int.Parse(simulation.Name)][bufidx] = simulation.ToNetworkState();
				}
			}

			// Update post-tick timers.
			worldStateBroadcastTimer.Update(interval);
		}

		public void SimulateWorld(float dt)
		{
			foreach (var player in this.playerHolder.GetChildren())
			{
				if (player is ServerPlayerSimulation)
				{
					(player as ServerPlayerSimulation).Simulate(dt);
				}
			}
		}

		public void InitializePlayerState(int id)
		{
			playerConnectionInfo[id] = new PlayerConnectionInfo();
			playerStateSnapshots[id] = new PlayerStatePackage[1024];
		}
	}
}
