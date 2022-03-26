using Godot;
using Shooter.Shared;
using System;
using System.Collections.Generic;
using Shooter.Shared.Network.Packages;
using Shooter.Client.Simulation.Components;

namespace Shooter.Client.Simulation
{
    public partial class PuppetPlayerSimulation : NetworkPlayerSimulation
    {
        private Queue<PlayerState> stateQueue = new Queue<PlayerState>();
        private PlayerState? lastState = null;
        private float stateTimer = 0;

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);
            var body = this.Components.Get<PlayerBodyComponent>();
            if (!bool.Parse(this.GameWorld.ServerVars["sv_interpolate"]))
            {
                return;
            }

            stateTimer += delta;

            float SimulationTickRate = 1f / (float)this.GetPhysicsProcessDeltaTime();
            float ServerSendRate = SimulationTickRate / 2;
            float ServerSendInterval = 1f / ServerSendRate;

            if (stateTimer > ServerSendInterval)
            {
                stateTimer -= ServerSendInterval;
                if (stateQueue.Count > 1)
                {
                    lastState = stateQueue.Dequeue();
                }
            }

            // We can only interpolate if we have a previous and next world state.
            if (!lastState.HasValue || stateQueue.Count < 1)
            {
                Logger.LogDebug(this, "RemotePlayer: not enough states to interp");
                return;
            }

            // DebugUI.ShowValue("RemotePlayer q size", stateQueue.Count);
            var nextState = stateQueue.Peek();
            float theta = stateTimer / ServerSendInterval;
            //var a = Quaternion.Euler(0, lastState.Value.Rotation.y, 0);
            //var b = Quaternion.Euler(0, nextState.Rotation.y, 0);
            var a = lastState.Value.Rotation;
            var b = nextState.Rotation;

            if (body != null)
            {
                var transform = body.Transform;
                transform.origin = lastState.Value.Position.Lerp(nextState.Position, theta);
                transform.basis = new Basis(a.Slerp(b, theta));
                body.Transform = transform;
            }
        }
        public override void ApplyNetworkState(PlayerState state)
        {
            if (bool.Parse(this.GameWorld.ServerVars["sv_interpolate"]))
            {
                // TODO: This whole thing needs to be simplified a bit more, but at least make sure
                // we're not buffering more than we should be.
                while (stateQueue.Count >= 2)
                {
                    stateQueue.Dequeue();
                }
                stateQueue.Enqueue(state);
            }
            else
            {
                var body = this.Components.Get<PlayerBodyComponent>();
                if (body != null)
                {
                    var transform = body.Transform;
                    transform.origin = state.Position;
                    transform.basis = new Basis(state.Rotation);
                    body.Transform = transform;

                    return;
                }
            }
        }
    }
}
