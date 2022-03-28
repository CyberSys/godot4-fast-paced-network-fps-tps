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
    /// <summary>
    /// The required interface for movement processors
    /// </summary>
    public interface IMovementProcessor
    {
        /// <summary>
        /// Calls on each tick for produce movement
        /// </summary>
        /// <param name="component"></param>
        /// <param name="serverVars"></param>
        /// <param name="inputs"></param>
        /// <param name="dt"></param>
        public Vector3 Simulate(IMoveable component, ServerVars serverVars, IPlayerInput inputs, float dt);

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
    public class DefaultMovementProcessor : IMovementProcessor
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
        internal void Execute(float delta, Vector3 executeVelocity)
        {
            //set crouching
            float couchLevel = 1.0f;
            if (this.CanCrouch())
            {
                couchLevel = delta * (inputs.GetInput("Crouch") ?
                    this.serverVars.Get<float>("sv_crouching_down_speed", 8.0f) :
                    this.serverVars.Get<float>("sv_crouching_up_speed", 4.0f)); ;
            }

            component.setCrouchingLevel(inputs.GetInput("Crouch") ? couchLevel * -1 : couchLevel);
            component.Velocity = executeVelocity;
            component.Move(delta, executeVelocity);
        }

        /// <inheritdoc />
        public Vector3 Simulate(IMoveable component, ServerVars serverVars, IPlayerInput inputs, float dt)
        {
            var processedVelocity = _velocity;

            if (component == null || inputs == null)
            {
                return Vector3.Zero;
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
                processedVelocity = this.GroundMove(dt, processedVelocity);
            }
            else if (!component.isOnGround())
            {
                processedVelocity = this.AirMove(dt, processedVelocity);
            }

            // Apply the final velocity to the character controller.
            this.Execute(dt, processedVelocity);

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
                processedVelocity = Vector3.Zero;
            }

            component.Transform = transform;
            _velocity = processedVelocity;
            return processedVelocity;
        }

        /// <inheritdoc />
        internal Vector3 ApplyFriction(Vector3 processedVelocity, float t, float dt)
        {
            Vector3 vec = processedVelocity; // Equivalent to: VectorCopy();
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
                var deaccl = this.GetGroundDeaccelerationFactor();

                control = speed < deaccl ? deaccl : speed;
                var friction = this.GetGroundFriction();
                drop = control * friction * dt * t;
            }

            newspeed = speed - drop;
            if (newspeed < 0)
                newspeed = 0;
            if (speed > 0)
                newspeed /= speed;

            processedVelocity.x *= newspeed;
            processedVelocity.z *= newspeed;

            return processedVelocity;
        }

        /// <inheritdoc />
        internal void QueueJump()
        {
            wishJump = inputs.GetInput("Jump");
        }

        /// <inheritdoc />
        internal Vector3 AirControl(Vector3 processedVelocity, Vector3 wishdir, float wishspeed, float dt)
        {
            float zspeed;
            float speed;
            float dot;
            float k;

            // Can't control movement if not moving forward or backward
            if (Mathf.Abs(inputs.ForwardBackwardAxis) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
                return processedVelocity;

            zspeed = processedVelocity.y;
            processedVelocity.y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            speed = processedVelocity.Length();
            processedVelocity = processedVelocity.Normalized();

            dot = processedVelocity.Dot(wishdir);
            k = 32;
            k *= this.GetAirControl() * dot * dot * dt;

            // Change direction while slowing down
            if (dot > 0)
            {
                processedVelocity.x = processedVelocity.x * speed + wishdir.x * k;
                processedVelocity.y = processedVelocity.y * speed + wishdir.y * k;
                processedVelocity.z = processedVelocity.z * speed + wishdir.z * k;

                processedVelocity = processedVelocity.Normalized();
            }

            processedVelocity.x *= speed;
            processedVelocity.y = zspeed; // Note this line
            processedVelocity.z *= speed;
            return processedVelocity;
        }

        /// <inheritdoc />
        internal Vector3 Accelerate(Vector3 processedVelocity, Vector3 wishdir, float wishspeed, float accel, float dt)
        {
            float addspeed;
            float accelspeed;
            float currentspeed;

            currentspeed = processedVelocity.Dot(wishdir);
            addspeed = wishspeed - currentspeed;
            if (addspeed <= 0)
                return processedVelocity;
            accelspeed = accel * dt * wishspeed;
            if (accelspeed > addspeed)
                accelspeed = addspeed;

            processedVelocity.x += accelspeed * wishdir.x;
            processedVelocity.z += accelspeed * wishdir.z;

            return processedVelocity;
        }

        public virtual bool CanCrouch()
        {
            return this.serverVars.Get<bool>("sv_crouching", true);
        }

        public virtual float GetGroundFriction()
        {
            return (inputs.GetInput("Crouch") && this.CanCrouch() ?
                    this.serverVars.Get<float>("sv_crouching_friction", 3.0f) : this.serverVars.Get<float>("sv_walk_friction", 6.0f));
        }
        public virtual float GetMovementSpeed()
        {
            return inputs.GetInput("Crouch") && this.CanCrouch()
                  ? this.serverVars.Get<float>("sv_crouching_speed", 4.0f) : this.serverVars.Get<float>("sv_walk_speed", 7.0f);

        }

        public virtual float GetGroundDeaccelerationFactor()
        {
            return inputs.GetInput("Crouch") && this.CanCrouch()
                 ? this.serverVars.Get<float>("sv_crouching_deaccel", 4.0f) : this.serverVars.Get<float>("sv_walk_deaccel", 10f);
        }

        public virtual float GetGroundAccelerationFactor()
        {
            return inputs.GetInput("Crouch") && this.CanCrouch()
                ? this.serverVars.Get<float>("sv_crouching_accel", 4.0f) : this.serverVars.Get<float>("sv_walk_accel", 14f);
        }

        public virtual float GetAirAcceleration()
        {
            return this.serverVars.Get<float>("sv_air_accel", 12f);
        }

        public virtual float GetAirDecceleration()
        {
            return this.serverVars.Get<float>("sv_air_deaccel", 2f);
        }

        public virtual float GetGravity()
        {
            return this.serverVars.Get<float>("sv_gravity", 20f);
        }

        public bool isOnGround()
        {
            return component.isOnGround();
        }

        public virtual float GetAirControl()
        {
            return this.serverVars.Get<float>("sv_air_control", 0.3f);
        }

        /// <inheritdoc />
        internal Vector3 GroundMove(float dt, Vector3 processedVelocity)
        {
            Vector3 wishdir;

            // Do not apply friction if the player is queueing up the next jump
            if (!wishJump)
                processedVelocity = ApplyFriction(processedVelocity, 1.0f, dt);
            else
                processedVelocity = ApplyFriction(processedVelocity, 0, dt);

            wishdir = new Vector3(inputs.LeftRightAxis, 0, inputs.ForwardBackwardAxis);
            wishdir = component.Transform.basis.Xform(wishdir);
            wishdir = wishdir.Normalized();

            var wishspeed = wishdir.Length();
            var moveSpeed = GetMovementSpeed();

            wishspeed *= moveSpeed;
            processedVelocity = Accelerate(processedVelocity, wishdir, wishspeed, this.GetGroundAccelerationFactor(), dt);

            // Reset the gravity velocity
            processedVelocity.y = -this.GetGravity() * dt;

            if (wishJump)
            {
                processedVelocity.y = this.serverVars.Get<float>("sv_jumpspeed", 6.5f);
                wishJump = false;
            }

            return processedVelocity;
        }

        /// <inheritdoc />
        internal Vector3 AirMove(float dt, Vector3 processedVelocity)
        {
            Vector3 wishdir;
            float wishvel = this.GetAirAcceleration();
            float accel;

            wishdir = new Vector3(inputs.LeftRightAxis, 0, inputs.ForwardBackwardAxis);
            wishdir = component.Transform.basis.Xform(wishdir);

            float wishspeed = wishdir.Length();
            wishspeed *= this.GetMovementSpeed();
            wishdir = wishdir.Normalized();

            // CPM: Aircontrol
            float wishspeed2 = wishspeed;
            if (processedVelocity.Dot(wishdir) < 0)
                accel = this.GetAirDecceleration();
            else
                accel = this.GetAirAcceleration();

            // If the player is ONLY strafing left or right
            if (inputs.ForwardBackwardAxis == 0 && inputs.LeftRightAxis != 0)
            {
                if (wishspeed > this.serverVars.Get<float>("sv_strafe_speed", 1.0f))
                    wishspeed = this.serverVars.Get<float>("sv_strafe_speed", 1.0f);

                accel = this.serverVars.Get<float>("sv_strafe_accel", 50.0f);
            }

            processedVelocity = Accelerate(processedVelocity, wishdir, wishspeed, accel, dt);

            if (this.GetAirControl() > 0)
                processedVelocity = AirControl(processedVelocity, wishdir, wishspeed2, dt);
            // !CPM: Aircontrol

            // Apply gravity
            processedVelocity.y -= this.GetGravity() * dt;

            return processedVelocity;
        }

    }
}
