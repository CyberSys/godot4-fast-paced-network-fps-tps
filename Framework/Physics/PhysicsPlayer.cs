using Godot;
using Framework.Network.Commands;
using Framework.Network;
using Framework.Input;
using Framework.Game;

namespace Framework.Physics
{
    public abstract partial class PhysicsPlayer : NetworkPlayer
    {
        public PhysicsPlayer(int id, IWorld world) : base(id, world)
        {
        }

        protected MovementCalculator calculator = new MovementCalculator();
        public IMoveable Body { get; set; }

        public override void _EnterTree()
        {
            base._EnterTree();
        }

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

        public override PlayerState ToNetworkState()
        {
            if (Body != null)
            {
                return new PlayerState
                {
                    Id = this.Id,
                    Position = Body.Transform.origin,
                    Rotation = Body.Transform.basis.GetRotationQuaternion(),
                    Velocity = calculator.playerVelocity,
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

            calculator.playerVelocity = state.Velocity;
        }

        public override void Simulate(float delta)
        {
            if (Body != null)
            {
                this.calculator.Simulate(Body, this.inputs, delta);
            }
        }
    }
}
