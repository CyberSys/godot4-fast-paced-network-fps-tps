using Godot;
using Framework.Network.Commands;
using Framework.Network;
using Framework.Input;
using Framework.Game;
using System;

namespace Framework.Physics
{
    /// <summary>
    /// The base class for physics based players (kinetmatic, rigid..)
    /// </summary>
    public abstract partial class PhysicsPlayer : NetworkPlayer
    {
        /// <inheritdoc />
        public PhysicsPlayer(int id, IWorld world) : base(id, world)
        {
        }

        /// <summary>
        /// The movement processor for physics player movement
        /// </summary>
        /// <returns></returns>
        public IMovementCalculator MovementProcessor { get; set; } = new DefaultMovementCalculator();

        /// <summary>
        /// The body attachted to the physics player simulation (eg.  kinetmatic, rigid..)
        /// </summary>
        /// <value></value>
        public IMoveable Body { get; set; }


        /// <summary>
        /// Teleport player to an given position
        /// </summary>
        /// <param name="origin">New position of the player</param>
        public void DoTeleport(Godot.Vector3 origin)
        {
            if (Body != null)
            {
                var bodyNode = Body as Node3D;
                var gt = bodyNode.GlobalTransform;
                gt.origin = origin;
                bodyNode.GlobalTransform = gt;
            }
        }

        /// <summary>
        /// Get the current network state
        /// </summary>
        /// <returns></returns>
        public override PlayerState ToNetworkState()
        {
            if (Body != null)
            {
                return new PlayerState
                {
                    Id = this.Id,
                    Position = Body.Transform.origin,
                    Rotation = Body.Transform.basis.GetRotationQuaternion(),
                    Velocity = MovementProcessor.Velocity,
                    Grounded = Body.isOnGround(),
                };
            }
            else
            {
                return new PlayerState
                {
                    Id = this.Id,
                };
            }
        }

        /// <summary>
        /// Apply an network state
        /// </summary>
        /// <param name="state">The network state to applied</param>
        public override void ApplyNetworkState(PlayerState state)
        {
            if (Body != null)
            {
                Body.activateColliderShape(false);

                var transform = Body.Transform;
                transform.origin = state.Position;
                transform.basis = new Basis(state.Rotation);
                Body.Transform = transform;
                Body.Velocity = state.Velocity;

                Body.activateColliderShape(true);
            }

            MovementProcessor.Velocity = state.Velocity;
        }

        /// <inheritdoc />
        internal override void InternalTick(float delta)
        {
            base.InternalTick(delta);

            if (Body != null)
            {
                this.MovementProcessor.Tick(Body, this.GameWorld.ServerVars, this.inputs, delta);
            }
        }
    }
}
