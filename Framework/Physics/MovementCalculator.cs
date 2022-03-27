using Godot;
using System;
using Framework.Input;

namespace Framework.Physics
{
    public interface IMovementCalculator
    {
        public void Execute(float delta, Vector3 velocity);
        public void Tick(IMoveable component, PlayerInputs inputs, float dt);

        public Vector3 Velocity { get; set; }
    }
    public class MovementCalculator : IMovementCalculator
    {        // Frame occuring factors
        public float gravity = 20.0f;
        public float friction = 6;
        public float airFriction = 0.4f;

        /* Movement stuff */
        public float moveSpeed = 7.0f;                // Ground move speed
        public float crouchSpeed = 4f;                // Ground crouch speed
        public float runAcceleration = 14.0f;         // Ground accel
        public float crouchAcceleration = 8f;         // Ground accel
        public float crouchDeaceleration = 4f;         // Ground accel
        public float crouchFrinction = 3f;         // Ground accel
        public float crouchUpSpeed = 9.0f;         // Ground accel crouch
        public float crouchDownSpeed = 4.0f;         // Ground accel crouch
        public float runDeacceleration = 10.0f;       // Deacceleration that occurs when running on the ground
        public float airAcceleration = 12.0f;          // Air accel
        public float airDecceleration = 2.0f;         // Deacceleration experienced when ooposite strafing
        public float airControl = 0.4f;               // How precise air control is
        public float sideStrafeAcceleration = 50.0f;  // How fast acceleration occurs to get up to sideStrafeSpeed when
        public float sideStrafeSpeed = 1.0f;          // What the max speed to generate when side strafing
        public float jumpSpeed = 6.5f;                // The speed at which the character's up axis gains when hitting jump
        public bool holdJumpToBhop = false;           // When enabled allows player to just hold jump button to keep on bhopping
        private bool wishJump = false;

        public Vector3 _velocity = Vector3.Zero;
        public Vector3 Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }
        private IMoveable component;
        private PlayerInputs inputs;

        private float attackCooldownTimer = 0f;

        private float crouchTime = 0f;


        public void Execute(float delta, Vector3 velocity)
        {
            //set crouching
            var couchLevel = delta * (inputs.Crouch ? crouchDownSpeed : crouchUpSpeed);
            component.setCrouchingLevel(inputs.Crouch ? couchLevel * -1 : couchLevel);

            component.Velocity = velocity;
            component.Move(delta, velocity);
        }

        public void Tick(IMoveable component, PlayerInputs inputs, float dt)
        {

            if (component == null)
            {
                return;
            }

            this.inputs = inputs;
            this.component = component;

            var transform = component.Transform;
            // Set orientation based on the view direction.
            if (inputs.ViewDirection == new Quaternion(0, 0, 0, 0))
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
            if (inputs.Fire && attackCooldownTimer <= 0)
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

        /**
	   * Applies friction to the player, called in both the air and on the ground
	   */
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
                var deaccl = inputs.Crouch ? crouchDeaceleration : runDeacceleration;

                control = speed < deaccl ? deaccl : speed;
                drop = control * (inputs.Crouch ? crouchFrinction : friction) * dt * t;
            }

            newspeed = speed - drop;
            if (newspeed < 0)
                newspeed = 0;
            if (speed > 0)
                newspeed /= speed;

            _velocity.x *= newspeed;
            _velocity.z *= newspeed;
        }
        private void QueueJump()
        {
            wishJump = inputs.Jump;
        }

        /**
		 * Air control occurs when the player is in the air, it allows
		 * players to move side to side much faster rather than being
		 * 'sluggish' when it comes to cornering.
		 */
        private void AirControl(Vector3 wishdir, float wishspeed, float dt)
        {
            float zspeed;
            float speed;
            float dot;
            float k;

            // Can't control movement if not moving forward or backward
            if (Mathf.Abs(inputs.ForwardAxis) < 0.001 || Mathf.Abs(wishspeed) < 0.001)
                return;
            zspeed = _velocity.y;
            _velocity.y = 0;
            /* Next two lines are equivalent to idTech's VectorNormalize() */
            speed = _velocity.Length();
            _velocity = _velocity.Normalized();

            dot = _velocity.Dot(wishdir);
            k = 32;
            k *= airControl * dot * dot * dt;

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
        /**
		 * Execs when the player is in the air
		*/
        private void AirMove(float dt)
        {

            Vector3 wishdir;
            float wishvel = airAcceleration;
            float accel;

            wishdir = new Vector3(inputs.RightAxis, 0, inputs.ForwardAxis);
            wishdir = component.Transform.basis.Xform(wishdir);

            float wishspeed = wishdir.Length();
            wishspeed *= (inputs.Crouch) ? crouchSpeed : moveSpeed;

            wishdir = wishdir.Normalized();

            // CPM: Aircontrol
            float wishspeed2 = wishspeed;
            if (_velocity.Dot(wishdir) < 0)
                accel = airDecceleration;
            else
                accel = airAcceleration;
            // If the player is ONLY strafing left or right
            if (inputs.ForwardAxis == 0 && inputs.RightAxis != 0)
            {
                if (wishspeed > sideStrafeSpeed)
                    wishspeed = sideStrafeSpeed;
                accel = sideStrafeAcceleration;
            }

            Accelerate(wishdir, wishspeed, accel, dt);
            if (airControl > 0)
                AirControl(wishdir, wishspeed2, dt);
            // !CPM: Aircontrol

            // Apply gravity
            _velocity.y -= gravity * dt;
        }

        private void Accelerate(Vector3 wishdir, float wishspeed, float accel, float dt)
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

        private void GroundMove(float dt)
        {
            Vector3 wishdir;

            // Do not apply friction if the player is queueing up the next jump
            if (!wishJump)
                ApplyFriction(1.0f, dt);
            else
                ApplyFriction(0, dt);

            wishdir = new Vector3(inputs.RightAxis, 0, inputs.ForwardAxis);
            wishdir = component.Transform.basis.Xform(wishdir);
            wishdir = wishdir.Normalized();

            var wishspeed = wishdir.Length();
            wishspeed *= (inputs.Crouch) ? crouchSpeed : moveSpeed;

            Accelerate(wishdir, wishspeed, (inputs.Crouch) ? crouchAcceleration : runAcceleration, dt);

            // Reset the gravity velocity
            _velocity.y = -gravity * dt;

            if (wishJump)
            {
                _velocity.y = jumpSpeed;
                wishJump = false;
            }
        }
    }
}
