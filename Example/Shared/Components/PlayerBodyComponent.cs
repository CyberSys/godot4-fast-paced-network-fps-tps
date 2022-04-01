using Framework;
using Framework.Network;
using Framework.Physics;
using Godot;
using System;
using Framework.Network.Commands;
using Framework.Game;

namespace Shooter.Shared.Components
{
    public partial class PlayerBodyComponent : CharacterBody3D, IChildMovementNetworkSyncComponent
    {
        public IBaseComponent BaseComponent { get; set; }
        public IMovementProcessor MovementProcessor { get; set; } = new DefaultMovementProcessor();

        public string GetComponentName()
        {
            return "body";
        }

        [Export]
        public NodePath ColliderPath = null;

        [Export]
        public NodePath MeshBodyPaths = null;

        public Vector3 previousPosition = Vector3.Zero;

        private float originalHeight = 0f;

        private CollisionShape3D shape;

        public float originalYPosition = 0f;

        public float getCrouchingHeight()
        {
            return this.shape.Transform.origin.y;
        }

        public float shapeHeight = 2.0f;
        public const float crouchHeight = 1.3f;
        private float previousCrouchLevel = 0f;
        private float currentCouchLevel = 0f;

        public void ApplyNetworkState(MovementBodyPackage state)
        {
            var transform = this.Transform;
            transform.origin = state.Position;
            transform.basis = new Basis(state.Rotation);
            this.Transform = transform;
            this.Velocity = state.Velocity;
            MovementProcessor.Velocity = state.Velocity;
        }

        public MovementBodyPackage GetNetworkState()
        {
            return new MovementBodyPackage
            {
                Position = this.Transform.origin,
                Rotation = this.Transform.basis.GetRotationQuaternion(),
                Velocity = this.Velocity,
                Grounded = this.IsOnGround(),
            };
        }

        public override void _EnterTree()
        {
            base._EnterTree();

            this.previousPosition = this.Transform.origin;
            this.shape = this.GetNode<CollisionShape3D>(ColliderPath);
            this.originalHeight = this.shape.Scale.y;
            this.originalYPosition = this.shape.Transform.origin.y;

            var shape = this.shape.Shape as CapsuleShape3D;

            this.shapeHeight = shape.Height;
            this.currentCouchLevel = shape.Height;
            this.previousCrouchLevel = shape.Height;
        }

        private bool _isOnGround = false;

        public bool IsOnGround()
        {
            return this.IsOnFloor();
        }

        public override void _Process(float delta)
        {
            base._Process(delta);

            var shadowMode = GeometryInstance3D.ShadowCastingSetting.On;
            var camera = this.BaseComponent.Components.Get<PlayerCameraComponent>();

            if (camera != null && camera.IsInsideTree() && camera.cameraMode == CameraMode.FPS)
            {
                shadowMode = GeometryInstance3D.ShadowCastingSetting.ShadowsOnly;
            }

            this.GetNode<MeshInstance3D>(MeshBodyPaths).CastShadow = shadowMode;
        }

        public void Move(float delta, Vector3 velocity)
        {
            this.MoveAndSlide();
        }

        public override void _PhysicsProcess(float delta)
        {
            base._PhysicsProcess(delta);

            if (this.previousCrouchLevel != this.currentCouchLevel)
            {

                var yPos = (this.shapeHeight - this.currentCouchLevel) / 2;
                yPos = yPos * -1;

                var transform = this.shape.Transform;
                transform.origin.y = this.originalYPosition + yPos;
                transform.origin.y = Mathf.Clamp(transform.origin.y, (this.shapeHeight / 2) * -1f, this.originalYPosition);
                this.shape.Transform = transform;

                var shape = this.shape.Shape as CapsuleShape3D;
                shape.Height = currentCouchLevel;
                this.shape.Shape = shape;

                this.previousCrouchLevel = this.currentCouchLevel;
            }
        }

        public void setCrouchingLevel(float crouchValue)
        {
            this.currentCouchLevel += MathF.Round(crouchValue, 2);
            this.currentCouchLevel = Mathf.Clamp(this.currentCouchLevel, crouchHeight, this.shapeHeight);
        }
    }
}
