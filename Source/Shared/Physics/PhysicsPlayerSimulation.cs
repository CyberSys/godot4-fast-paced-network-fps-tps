using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shooter.Shared.Network.Packages;
using Shooter.Client.Simulation.Components;

namespace Shooter.Shared
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
            var component = this.Components.Get<PlayerBodyComponent>();
            if (component != null)
            {
                return new PlayerState
                {
                    Id = int.Parse(this.Name),
                    Position = component.Transform.origin,
                    Rotation = component.Transform.basis.GetRotationQuaternion(),
                    Velocity = calculator.playerVelocity,
                    Grounded = component.isOnGround(),
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
            var component = this.Components.Get<PlayerBodyComponent>();
            if (component != null)
            {
                component.activateColliderShape(false);

                var transform = component.Transform;
                transform.origin = state.Position;
                transform.basis = new Basis(state.Rotation);
                component.Transform = transform;
                component.Velocity = state.Velocity;

                component.activateColliderShape(true);
            }

            calculator.playerVelocity = state.Velocity;
        }

        public override void Simulate(float delta)
        {
            var component = this.Components.Get<PlayerBodyComponent>();
            if (component != null)
            {
                this.calculator.Simulate(component, this.inputs, delta);
            }
        }
    }
}
