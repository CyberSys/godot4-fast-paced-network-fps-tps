using Godot;
using Shooter.Shared;
using Shooter.Client.Services;
using Shooter.Client.Simulation;
using Shooter.Client.Simulation.Components;
using System;
using Shooter.Shared.Network.Packages;
using System.Collections.Generic;
using LiteNetLib;

namespace Shooter.Client
{
    public partial class ClientGameWorld : GameWorld
    {
        protected ClientNetworkService netService = null;
        public ClientSimulationAdjuster clientSimulationAdjuster;


        private uint[] localPlayerWorldTickSnapshots = new uint[1024];
        private PlayerInputs[] localPlayerInputsSnapshots = new PlayerInputs[1024];
        private PlayerStatePackage[] localPlayerStateSnapshots = new PlayerStatePackage[1024];
        private Queue<WorldStatePackage> worldStateQueue = new Queue<WorldStatePackage>();

        public LocalPlayerSimulation localPlayer;

        public uint lastServerWorldTick = 0;
        private int replayedStates;

        public override void _EnterTree()
        {
            base._EnterTree();

            this.netService = this.gameInstance.Services.Get<ClientNetworkService>();
            this.netService.Subscribe<WorldStatePackage>(HandleWorldState);
            this.netService.Subscribe<ServerInitializationPackage>(InitWorld);
        }

        private void InitWorld(ServerInitializationPackage cmd, NetPeer peer)
        {
            Logger.LogDebug(this, "Init world with server user id " + cmd.PlayerId);
            this.netService.MyId = cmd.PlayerId;
            this?.Init(cmd.GameTick);
        }

        private void HandleWorldState(WorldStatePackage cmd, NetPeer peed)
        {
            worldStateQueue.Enqueue(cmd);
        }

        public override void Init(uint initalWorldTick)
        {
            base.Init(0);

            var simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();

            lastServerWorldTick = initalWorldTick;

            this.simulationAdjuster = clientSimulationAdjuster = new ClientSimulationAdjuster(simTickRate / 2);
            WorldTick = clientSimulationAdjuster.GuessClientTick((float)this.GetPhysicsProcessDeltaTime(), initalWorldTick, this.netService.ping);
        }

        private MovingAverage excessWorldStateAvg = new MovingAverage(10);

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

        public override void Tick(float interval)
        {
            if (this.localPlayer != null)
            {
                float simTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
                var serverSendRate = simTickRate / 2;

                var MaxStaleServerStateTicks = (int)MathF.Ceiling(
                    Settings.MaxStaleServerStateAgeMs / serverSendRate);

                var inputHandler = this.localPlayer?.Components.Get<PlayerInputComponent>();
                var inputs = (inputHandler != null && !this.gameInstance.GuiDisableInput) ? inputHandler.GetPlayerInput() : new PlayerInputs();

                var lastTicks = WorldTick - lastServerWorldTick;
                if (Settings.FreezeClientOnStaleServer && lastTicks >= MaxStaleServerStateTicks)
                {
                    Logger.LogDebug(this, "Server state is too old (is the network connection dead?) - max ticks " + MaxStaleServerStateTicks + " - currentTicks => " + lastTicks);
                    inputs = new PlayerInputs();
                }

                // Update our snapshot buffers.
                uint bufidx = WorldTick % 1024;
                localPlayerInputsSnapshots[bufidx] = inputs;
                localPlayerStateSnapshots[bufidx] = this.localPlayer.ToNetworkState();
                localPlayerWorldTickSnapshots[bufidx] = lastServerWorldTick;

                // Send a command for all inputs not yet acknowledged from the server.
                var unackedInputs = new List<PlayerInputs>();
                var clientWorldTickDeltas = new List<short>();
                // TODO: lastServerWorldTick is technically not the same as lastAckedInputTick, fix this.
                for (uint tick = lastServerWorldTick; tick <= WorldTick; ++tick)
                {
                    unackedInputs.Add(localPlayerInputsSnapshots[tick % 1024]);
                    clientWorldTickDeltas.Add((short)(tick - localPlayerWorldTickSnapshots[tick % 1024]));
                }

                var command = new PlayerInputCommand
                {
                    StartWorldTick = lastServerWorldTick,
                    Inputs = unackedInputs.ToArray(),
                    ClientWorldTickDeltas = clientWorldTickDeltas.ToArray(),
                };

                //send to server => command
                this.netService.SendMessageSerialisable(this.netService.ServerPeer, command, LiteNetLib.DeliveryMethod.Sequenced);

                //SetPlayerInputs
                this.localPlayer?.SetPlayerInputs(inputs);

                // SimulateWorld(dt);
                this.localPlayer?.Simulate(interval);
            }

            //increase worldTick
            ++this.WorldTick;

            if (this.localPlayer != null)
            {
                ProcessServerWorldState();
            }
        }


        private void ProcessServerWorldState()
        {
            if (worldStateQueue.Count < 1)
            {
                return;
            }

            var incomingState = worldStateQueue.Dequeue();
            lastServerWorldTick = incomingState.WorldTick;

            // Calculate our actual tick lead on the server perspective. We add one because the world
            // state the server sends to use is always 1 higher than the latest input that has been
            // processed.

            if (incomingState.YourLatestInputTick > 0)
            {
                int actualTickLead = (int)incomingState.YourLatestInputTick - (int)lastServerWorldTick + 1;

                this.clientSimulationAdjuster.NotifyActualTickLead(actualTickLead, false);
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

            // Lookup the historical state for the world tick we got.
            uint bufidx = incomingState.WorldTick % 1024;
            var stateSnapshot = localPlayerStateSnapshots[bufidx];

            // Locate the data for our local player.
            PlayerStatePackage incomingLocalPlayerState = new PlayerStatePackage();
            foreach (var playerState in incomingState.PlayerStates)
            {
                if (playerState.Id == int.Parse(this.localPlayer.Name))
                {
                    incomingLocalPlayerState = playerState;
                }
                else
                {
                    var player = this.playerHolder.GetNodeOrNull<PuppetPlayerSimulation>(playerState.Id.ToString());
                    player.ApplyNetworkState(playerState);
                }
            }
            if (default(PlayerStatePackage).Equals(incomingLocalPlayerState))
            {
                Logger.LogDebug(this, "No local player state found!");
            }

            // Compare the historical state to see how off it was.
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
                this.localPlayer.ApplyNetworkState(incomingLocalPlayerState);

                // Loop through and replay all captured input snapshots up to the current tick.
                uint replayTick = incomingState.WorldTick;

                while (replayTick < WorldTick)
                {
                    // Grab the historical input.
                    bufidx = replayTick % 1024;
                    var inputSnapshot = localPlayerInputsSnapshots[bufidx];

                    // Rewrite the historical sate snapshot.
                    localPlayerStateSnapshots[bufidx] = this.localPlayer.ToNetworkState();

                    // Apply inputs to the associated player controller and simulate the world.
                    this.localPlayer.SetPlayerInputs(inputSnapshot);
                    this.localPlayer.Simulate((float)this.GetPhysicsProcessDeltaTime());

                    ++replayTick;
                }
            }
        }

    }
}
