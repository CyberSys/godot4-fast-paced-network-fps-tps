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
using Framework.Input;
using Framework.Game;
using Framework.Network;

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
        /// <param name="inputs"></param>
        /// <param name="dt"></param>
        public Vector3 Simulate(NetworkCharacter component, GeneralPlayerInput inputs, float dt);

        /// <summary>
        /// The current velocity of the moveable object
        /// </summary>
        /// <value></value>
        public Vector3 Velocity { get; set; }



        /// <summary>
        /// Set server related vars
        /// </summary>
        /// <param name="vars"></param>
        public void SetServerVars(VarsCollection vars);

        /// <summary>
        /// Get maximium speed
        /// </summary>
        /// <returns></returns>
        public float GetMovementSpeedFactor();

        /// <summary>
        /// Get the walking speed
        /// </summary>
        /// <returns></returns>
        public float GetWalkingSpeed();


        /// <summary>
        /// Get the forward backward axis
        /// </summary>
        /// <returns></returns>
        public float ForwardBackwardAxis { get; }

        /// <summary>
        /// Get the left right axis
        /// </summary>
        /// <returns></returns>
        public float LeftRightAxis { get; }
    }

    /// <summary>
    /// An default movement calculator
    /// Handles friction, air control, jumping and accelerate
    /// </summary>
    public class DefaultMovementProcessor : IMovementProcessor
    {
        private bool wishJump = false;

        private VarsCollection serverVars;
        private VarsCollection clientVars;

        private Vector3 _velocity = Vector3.Zero;
        private NetworkCharacter component;
        private IPlayerInput inputs;
        private float attackCooldownTimer = 0f;
        private float crouchTime = 0f;

        /// <inheritdoc />
        public Vector3 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }



        /// <inheritdoc />
        public void SetServerVars(VarsCollection vars)
        {
            serverVars = vars;
        }


        /// <inheritdoc />
        public Vector3 Simulate(NetworkCharacter component, GeneralPlayerInput inputs, float dt)
        {
            if (component == null)
            {
                return Vector3.Zero;
            }

            this.inputs = inputs;
            this.component = component;

            //Set rotation
            var comp = this.component.Transform;
            comp.basis = new Basis(new Vector3(0, inputs.ViewDirection.y, 0));
            this.component.Transform = comp;

            // Process movement.
            this.QueueJump();

            if (component.IsOnGround())
            {
                _velocity = this.GroundMove(dt, _velocity);
            }
            else if (!component.IsOnGround())
            {
                _velocity = this.AirMove(dt, _velocity);
            }

            float couchLevel = 1.0f;
            if (this.CanCrouch())
            {
                couchLevel = dt * (inputs.GetInput("Crouch") ?
                    this.serverVars.Get<float>("sv_crouching_down_speed", 8.0f) :
                    this.serverVars.Get<float>("sv_crouching_up_speed", 4.0f));
            }

            component.SetCrouchingLevel(inputs.GetInput("Crouch") ? couchLevel * -1 : couchLevel);
            component.Move(dt, _velocity);

            // HACK: Reset to zero when falling off the edge for now.
            if (this.component.GlobalTransform.origin.y < -100)
            {
                var gt = this.component.GlobalTransform;
                gt.origin = Vector3.Zero;
                this.component.GlobalTransform = gt;
                this.component.Velocity = Vector3.Zero;
                _velocity = Vector3.Zero;
            }

            return _velocity;
        }

        /// <inheritdoc />
        internal Vector3 ApplyFriction(Vector3 _velocity, float t, float dt)
        {
            Vector3 vec = _velocity; // Equivalent to: VectorCopy();
            float speed;
            float newspeed;
            float control;
            float drop;

            vec.y = 0.0f;
            speed = vec.Length();
            drop = 0.0f;

            /* Only if the player is on the ground then apply friction */
            if (this.component.IsOnGround())
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

            _velocity.x *= newspeed;
            _velocity.z *= newspeed;

            return _velocity;
        }

        /// <inheritdoc />
        internal void QueueJump()
        {
            wishJump = inputs.GetInput("Jump");
        }

        /// <inheritdoc />
        internal Vector3 AirControl(Vector3 _velocity, Vector3 wishdir, float wishspeed, float dt)
        {
            float zspeed;
            float speed;
            float dot;
            float k;

            // Can't control movement if not moving forward or backward
            if (Mathf.Abs(this.ForwardBackwardAxis) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
                return _velocity;

            zspeed = _velocity.y;
            _velocity.y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            speed = _velocity.Length();
            _velocity = _velocity.Normalized();

            dot = _velocity.Dot(wishdir);
            k = 32;
            k *= this.GetAirControl() * dot * dot * dt;

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
            return _velocity;
        }

        /// <inheritdoc />
        internal Vector3 Accelerate(Vector3 _velocity, Vector3 wishdir, float wishspeed, float accel, float dt)
        {
            float addspeed;
            float accelspeed;
            float currentspeed;

            currentspeed = _velocity.Dot(wishdir);
            addspeed = wishspeed - currentspeed;

            if (addspeed <= 0)
                return _velocity;

            accelspeed = accel * dt * wishspeed;

            if (accelspeed > addspeed)
                accelspeed = addspeed;

            _velocity.x += accelspeed * wishdir.x;
            _velocity.z += accelspeed * wishdir.z;

            return _velocity;
        }

        /// <summary>
        /// Check if player allows to crouch
        /// </summary>
        /// <returns></returns>
        public virtual bool CanCrouch()
        {
            return this.serverVars.Get<bool>("sv_crouching", true);
        }

        /// <summary>
        /// Get the friction for crouching
        /// </summary>
        /// <returns></returns>
        public virtual float GetGroundFriction()
        {
            return ((inputs.GetInput("Crouch") && this.CanCrouch())
                    || inputs.GetInput("Shifting"))
                  ?
                    this.serverVars.Get<float>("sv_crouching_friction", 3.0f) : this.serverVars.Get<float>("sv_walk_friction", 6.0f);
        }

        /// <summary>
        /// Get the default movement speed
        /// </summary>
        /// <returns></returns>
        public virtual float GetMovementSpeed()
        {
            return ((inputs.GetInput("Crouch") && this.CanCrouch())
                    || inputs.GetInput("Shifting"))
                  ? this.serverVars.Get<float>("sv_crouching_speed", 4.0f) : this.GetWalkingSpeed();
        }

        /// <summary>
        /// Get the movement speed factor (current speed / walking speed)
        /// </summary>
        /// <returns></returns>
        public virtual float GetMovementSpeedFactor()
        {
            var vel = this._velocity;
            vel.y = 0;
            return vel.Length() / this.GetWalkingSpeed();
        }

        /// <summary>
        /// Get the default walking speed
        /// </summary>
        /// <returns></returns>
        public virtual float GetWalkingSpeed()
        {
            if (this.serverVars == null)
                return 0;
            return this.serverVars.Get<float>("sv_walk_speed", 7.0f);
        }

        /// <summary>
        /// Get the default ground deaccel factor
        /// </summary>
        /// <returns></returns>
        public virtual float GetGroundDeaccelerationFactor()
        {
            return ((inputs.GetInput("Crouch") && this.CanCrouch())
                    || inputs.GetInput("Shifting"))
                  ? this.serverVars.Get<float>("sv_crouching_deaccel", 14.0f) : this.serverVars.Get<float>("sv_walk_deaccel", 10f);
        }

        /// <summary>
        /// Get the default ground accel factor
        /// </summary>
        /// <returns></returns>
        public virtual float GetGroundAccelerationFactor()
        {
            return ((inputs.GetInput("Crouch") && this.CanCrouch())
                    || inputs.GetInput("Shifting"))
                  ? this.serverVars.Get<float>("sv_crouching_accel", 20.0f) : this.serverVars.Get<float>("sv_walk_accel", 7.5f);
        }

        /// <summary>
        /// Get the default air accel factor
        /// </summary>
        /// <returns></returns>
        public virtual float GetAirAcceleration()
        {
            return this.serverVars.Get<float>("sv_air_accel", 12f);
        }

        /// <summary>
        /// Get the default air deaccel factor
        /// </summary>
        /// <returns></returns>
        public virtual float GetAirDecceleration()
        {
            return this.serverVars.Get<float>("sv_air_deaccel", 2f);
        }

        /// <summary>
        /// Get the default gravity
        /// </summary>
        /// <returns></returns>
        public virtual float GetGravity()
        {
            return this.serverVars.Get<float>("sv_gravity", 20f);
        }

        /// <summary>
        /// The maximum air speed
        /// </summary>
        /// <returns></returns>
        public virtual float MaxAirSpeed()
        {
            return this.serverVars.Get<float>("sv_max_air_speed", 1.3f);
        }

        /// <summary>
        /// Get the default gravity multiplier
        /// </summary>
        /// <returns></returns>
        public virtual float GetGravityMultiplier()
        {
            return this.serverVars.Get<float>("sv_gravity_multiplier", 1.6f);
        }


        /// <summary>
        /// Check if player is on ground
        /// </summary>
        /// <returns></returns>
        public bool isOnGround()
        {
            return component.IsOnGround();
        }

        /// <summary>
        /// Get the current air control value
        /// </summary>
        /// <returns></returns>
        public virtual float GetAirControl()
        {
            return this.serverVars.Get<float>("sv_air_control", 0.3f);
        }

        /// <summary>
        /// Get left and right axis (currently use input keys "Forward" and "Back")
        /// </summary>
        /// <value></value>
        public virtual float LeftRightAxis
        {
            get
            {
                if (this.inputs == null)
                    return 0;
                return this.inputs.GetInput("Right") ? 1f : this.inputs.GetInput("Left") ? -1f : 0f;
            }
        }

        /// <summary>
        /// Get forward and back axis (currently use input keys "Forward" and "Back")
        /// </summary>
        /// <value></value>
        public virtual float ForwardBackwardAxis
        {
            get
            {
                if (this.inputs == null)
                    return 0;

                return this.inputs.GetInput("Back") ? -1f : this.inputs.GetInput("Forward") ? 1f : 0f;
            }
        }

        /// <inheritdoc />
        internal Vector3 GroundMove(float dt, Vector3 _velocity)
        {
            Vector3 wishdir;

            // Do not apply friction if the player is queueing up the next jump
            if (!wishJump)
                _velocity = ApplyFriction(_velocity, 1.0f, dt);
            else
                _velocity = ApplyFriction(_velocity, 0, dt);

            wishdir = new Vector3(this.LeftRightAxis / 10, 0, (this.ForwardBackwardAxis * -1) / 10);
            wishdir = (component as Node3D).GlobalTransform.basis.Xform(wishdir);
            wishdir = wishdir.Normalized();

            var wishspeed = wishdir.Length();
            wishspeed *= GetMovementSpeed();

            _velocity = Accelerate(_velocity, wishdir, wishspeed, this.GetGroundAccelerationFactor(), dt);

            // Reset the gravity velocity
            _velocity.y = -this.GetGravity() * dt;

            if (wishJump)
            {
                _velocity.y = this.serverVars.Get<float>("sv_jumpspeed", 9f);
                wishJump = false;
            }

            return _velocity;
        }

        /// <inheritdoc />
        internal Vector3 AirMove(float dt, Vector3 _velocity)
        {
            Vector3 wishdir;
            float wishvel = this.GetAirAcceleration();
            float accel;

            wishdir = new Vector3(this.LeftRightAxis, 0, this.ForwardBackwardAxis * -1);
            wishdir = (component as Node3D).GlobalTransform.basis.Xform(wishdir);

            float wishspeed = wishdir.Length();
            wishspeed *= this.GetMovementSpeed();

            if (wishspeed > this.MaxAirSpeed())
                wishspeed = this.MaxAirSpeed();

            wishdir = wishdir.Normalized();

            // CPM: Aircontrol
            float wishspeed2 = wishspeed;
            if (_velocity.Dot(wishdir) < 0)
                accel = this.GetAirDecceleration();
            else
                accel = this.GetAirAcceleration();

            // If the player is ONLY strafing left or right
            if (this.ForwardBackwardAxis == 0 && this.LeftRightAxis != 0)
            {
                if (wishspeed > this.serverVars.Get<float>("sv_strafe_speed", 1.0f))
                    wishspeed = this.serverVars.Get<float>("sv_strafe_speed", 1.0f);

                accel = this.serverVars.Get<float>("sv_strafe_accel", 50.0f);
            }

            _velocity = Accelerate(_velocity, wishdir, wishspeed, accel, dt);

            if (this.GetAirControl() > 0)
                _velocity = AirControl(_velocity, wishdir, wishspeed2, dt);

            _velocity.y -= this.GetGravity() * this.GetGravityMultiplier() * dt;

            return _velocity;
        }

    }
}
