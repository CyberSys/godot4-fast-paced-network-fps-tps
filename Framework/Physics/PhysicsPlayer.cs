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
using Framework.Network.Commands;
using Framework.Network;
using Framework.Input;
using Framework.Game;
using System;

namespace Framework.Physics
{
    /// <summary>
    /// The base class for physics based players (kinematic, rigid..)
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
        public IMovementProcessor MovementProcessor { get; set; } = new DefaultMovementProcessor();

        /// <summary>
        /// The body attachted to the physics player simulation (eg.  kinematic, rigid..)
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
