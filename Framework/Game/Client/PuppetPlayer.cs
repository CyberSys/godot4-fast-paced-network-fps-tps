using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;
using Framework.Network;
using System;
using Godot;
using Framework.Physics;
using System.Collections.Generic;

namespace Framework.Game.Client
{

    /// <summary>
    /// The base class of an puppet player
    /// </summary>
    public class PuppetPlayer : PhysicsPlayer
    {
        /// <inheritdoc />
        public PuppetPlayer(int id, IWorld world) : base(id, world)
        {
        }

        private Queue<PlayerState> stateQueue = new Queue<PlayerState>();
        private PlayerState? lastState = null;
        private float stateTimer = 0;

        /// <inheritdoc />
        internal override void InternalTick(float delta)
        {
            base.InternalTick(delta);

            if (!this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
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

            if (this.Body != null)
            {
                var transform = this.Body.Transform;
                transform.origin = lastState.Value.Position.Lerp(nextState.Position, theta);
                transform.basis = new Basis(a.Slerp(b, theta));
                this.Body.Transform = transform;
            }
        }

        /// <inheritdoc />
        public override void ApplyNetworkState(PlayerState state)
        {
            if (this.GameWorld.ServerVars.Get<bool>("sv_interpolate", true))
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
                if (this.Body != null)
                {
                    var transform = this.Body.Transform;
                    transform.origin = state.Position;
                    transform.basis = new Basis(state.Rotation);
                    this.Body.Transform = transform;

                    return;
                }
            }
        }
    }
}
