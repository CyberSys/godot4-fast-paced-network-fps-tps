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
using System;
using Framework.Input;

using System.Collections.Generic;
using Framework.Game.Server;

namespace Framework.Physics
{
    public interface IMovementCalculator
    {
        /// <summary>
        /// Calls on each tick for produce movement
        /// </summary>
        /// <param name="component"></param>
        /// <param name="serverVars"></param>
        /// <param name="inputs"></param>
        /// <param name="dt"></param>
        public void Tick(IMoveable component, ServerVars serverVars, IPlayerInput inputs, float dt);

        /// <summary>
        /// The current velocity of the moveable object
        /// </summary>
        /// <value></value>
        public Vector3 Velocity { get; set; }
    }

    /// <summary>
    /// An default movement calculator
    /// Handles friction, air control, jumping and accelerate
    /// </summary>
    public class DefaultMovementCalculator : IMovementCalculator
    {
        private bool wishJump = false;

        private ServerVars serverVars;

        private Vector3 _velocity = Vector3.Zero;

        /// <inheritdoc />
        public Vector3 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        private IMoveable component;
        private IPlayerInput inputs;
        private float attackCooldownTimer = 0f;
        private float crouchTime = 0f;

        /// <inheritdoc />
        internal void Execute(float delta, Vector3 velocity)
        {
            //set crouching
            float couchLevel = 1.0f;
            if (this.serverVars.Get<bool>("sv_crouching", true))
            {
                couchLevel = delta * (inputs.GetInput("Crouch") ?
                    this.serverVars.Get<float>("sv_crouching_down_speed", 8.0f) :
                    this.serverVars.Get<float>("sv_crouching_up_speed", 4.0f)); ;
            }

            component.setCrouchingLevel(inputs.GetInput("Crouch") ? couchLevel * -1 : couchLevel);
            component.Velocity = velocity;
            component.Move(delta, velocity);
        }

        /// <inheritdoc />
        public void Tick(IMoveable component, ServerVars serverVars, IPlayerInput inputs, float dt)
        {
            if (component == null || inputs == null)
            {
                return;
            }

            this.inputs = inputs;
            this.component = component;
            this.serverVars = serverVars;

            var transform = component.Transform;
            // Set orientation based on the view direction.
            if (inputs.ViewDirection == null || inputs.ViewDirection == new Quaternion(0, 0, 0, 0))
            {
                inputs.ViewDirection = Quaternion.Identity;
            }

            var euler = inputs.ViewDirection.GetEuler();
            transform.basis = new Basis(new Quaternion(new Vector3(0, euler.y, 0)));
            component.Transform = transform;

            // Process movement.
            this.QueueJump();

            if (component.isOnGround())
            {
                this.GroundMove(dt);
            }
            else if (!component.isOnGround())
                this.AirMove(dt);

            // Apply the final velocity to the character controller.
            this.Execute(dt, Velocity);



            transform = component.Transform;

            // Process attacks.
            attackCooldownTimer -= dt;
            if (inputs.GetInput("Fire") && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = 1f;
                // attackDelegate(
                //    NetworkObjectType.HITSCAN_ATTACK, AttackPosition, inputs.ViewDirection);
            }

            // HACK: Reset to zero when falling off the edge for now.

            if (transform.origin.y < -100)
            {
                transform.origin = Vector3.Zero;
                Velocity = Vector3.Zero;
            }

            component.Transform = transform;
        }

        /// <inheritdoc />
        private void ApplyFriction(float t, float dt)
        {
            Vector3 vec = Velocity; // Equivalent to: VectorCopy();
            float speed;
            float newspeed;
            float control;
            float drop;

            vec.y = 0.0f;
            speed = vec.Length();
            drop = 0.0f;

            /* Only if the player is on the ground then apply friction */
            if (this.component.isOnGround())
            {
                var deaccl = inputs.GetInput("Crouch") && this.serverVars.Get<bool>("sv_crouching", true)
                ? this.serverVars.Get<float>("sv_crouching_deaccel", 4.0f) : this.serverVars.Get<float>("sv_walk_deaccel", 10f);

                control = speed < deaccl ? deaccl : speed;

                var friction = (inputs.GetInput("Crouch") && this.serverVars.Get<bool>("sv_crouching", true) ?
                this.serverVars.Get<float>("sv_crouching_friction", 3.0f) : this.serverVars.Get<float>("sv_walk_friction", 6.0f));

                drop = control * friction * dt * t;
            }

            newspeed = speed - drop;
            if (newspeed < 0)
                newspeed = 0;
            if (speed > 0)
                newspeed /= speed;

            _velocity.x *= newspeed;
            _velocity.z *= newspeed;
        }

        /// <inheritdoc />
        internal void QueueJump()
        {
            wishJump = inputs.GetInput("Jump");
        }

        /// <inheritdoc />
        internal void AirControl(Vector3 wishdir, float wishspeed, float dt)
        {
            float zspeed;
            float speed;
            float dot;
            float k;

            // Can't control movement if not moving forward or backward
            if (Mathf.Abs(inputs.ForwardBackwardAxis) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
                return;
            zspeed = _velocity.y;
            _velocity.y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            speed = _velocity.Length();
            _velocity = _velocity.Normalized();

            dot = _velocity.Dot(wishdir);
            k = 32;
            k *= this.serverVars.Get<float>("sv_air_control", 0.3f) * dot * dot * dt;

            // Change direction while slowing down
            if (dot > 0)
            {
                _velocity.x = _velocity.x * speed + wishdir.x * k;
                _velocity.y = _velocity.y * speed + wishdir.y * k;
                _velocity.z = _velocity.z * speed + wishdir.z * k;

                _velocity = _velocity.Normalized();
            }

            _velocity.x *= speed;
            _velocity.y = zspeed; // Note this line
            _velocity.z *= speed;
        }

        /// <inheritdoc />
        internal void AirMove(float dt)
        {

            Vector3 wishdir;
            float wishvel = this.serverVars.Get<float>("sv_air_accel", 12f);
            float accel;

            wishdir = new Vector3(inputs.LeftRightAxis, 0, inputs.ForwardBackwardAxis);
            wishdir = component.Transform.basis.Xform(wishdir);

            float wishspeed = wishdir.Length();
            float speed = inputs.GetInput("Crouch") && this.serverVars.Get<bool>("sv_crouching", true)
                ? this.serverVars.Get<float>("sv_crouching_speed", 4.0f) : this.serverVars.Get<float>("sv_walk_speed", 7.0f);
            wishspeed *= speed;

            wishdir = wishdir.Normalized();

            // CPM: Aircontrol
            float wishspeed2 = wishspeed;
            if (_velocity.Dot(wishdir) < 0)
                accel = this.serverVars.Get<float>("sv_air_deaccel", 2f);
            else
                accel = this.serverVars.Get<float>("sv_air_accel", 12f);

            // If the player is ONLY strafing left or right
            if (inputs.ForwardBackwardAxis == 0 && inputs.LeftRightAxis != 0)
            {
                if (wishspeed > this.serverVars.Get<float>("sv_strafe_speed", 1.0f))
                    wishspeed = this.serverVars.Get<float>("sv_strafe_speed", 1.0f);

                accel = this.serverVars.Get<float>("sv_strafe_accel", 50.0f);
            }

            Accelerate(wishdir, wishspeed, accel, dt);

            if (this.serverVars.Get<float>("sv_air_control", 0.3f) > 0)
                AirControl(wishdir, wishspeed2, dt);
            // !CPM: Aircontrol

            // Apply gravity
            _velocity.y -= this.serverVars.Get<float>("sv_gravity", 20f) * dt;
        }

        /// <inheritdoc />
        internal void Accelerate(Vector3 wishdir, float wishspeed, float accel, float dt)
        {
            float addspeed;
            float accelspeed;
            float currentspeed;

            currentspeed = _velocity.Dot(wishdir);
            addspeed = wishspeed - currentspeed;
            if (addspeed <= 0)
                return;
            accelspeed = accel * dt * wishspeed;
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            _velocity.x += accelspeed * wishdir.x;
            _velocity.z += accelspeed * wishdir.z;
        }

        /// <inheritdoc />
        internal void GroundMove(float dt)
        {
            Vector3 wishdir;

            // Do not apply friction if the player is queueing up the next jump
            if (!wishJump)
                ApplyFriction(1.0f, dt);
            else
                ApplyFriction(0, dt);

            wishdir = new Vector3(inputs.LeftRightAxis, 0, inputs.ForwardBackwardAxis);
            wishdir = component.Transform.basis.Xform(wishdir);
            wishdir = wishdir.Normalized();

            var wishspeed = wishdir.Length();

            float moveSpeed = inputs.GetInput("Crouch") && this.serverVars.Get<bool>("sv_crouching", true)
             ? this.serverVars.Get<float>("sv_crouching_speed", 4.0f) : this.serverVars.Get<float>("sv_walk_speed", 7.0f);

            wishspeed *= moveSpeed;

            var accel = inputs.GetInput("Crouch") && this.serverVars.Get<bool>("sv_crouching", true)
        ? this.serverVars.Get<float>("sv_crouching_accel", 4.0f) : this.serverVars.Get<float>("sv_walk_accel", 14f);

            Accelerate(wishdir, wishspeed, accel, dt);

            // Reset the gravity velocity
            _velocity.y = -this.serverVars.Get<float>("sv_gravity", 20f) * dt;

            if (wishJump)
            {
                _velocity.y = this.serverVars.Get<float>("sv_jumpspeed", 6.5f);
                wishJump = false;
            }
        }
    }
}
