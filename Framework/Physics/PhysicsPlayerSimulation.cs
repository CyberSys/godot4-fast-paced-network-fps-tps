using Godot;
using Framework.Network.Commands;
using Framework.Network;
using Framework.Input;

namespace Framework.Physics
{
    public abstract partial class PhysicsPlayerSimulation : NetworkPlayerSimulation
    {
        protected MovementCalculator calculator = new MovementCalculator();

        public override void _EnterTree()
        {
            base._EnterTree();
        }

        public override PlayerState ToNetworkState()
        {
            if (Body != null)
            {
                return new PlayerState
                {
                    Id = int.Parse(this.Name),
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
                    Id = int.Parse(this.Name),
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
