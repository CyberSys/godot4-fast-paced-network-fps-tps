using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shooter.Shared.Network.Packages;
using Shooter.Client.Simulation.Components;

namespace Shooter.Shared
{
    public abstract partial class NetworkPlayerSimulation : Node
    {
        public ComponentRegistry<NetworkPlayerSimulation> Components { get; set; }
        public GameWorld GameWorld { get; set; }
        protected PlayerInputs inputs;

        public void SetPlayerInputs(PlayerInputs inputs)
        {
            this.inputs = inputs;
        }

        public NetworkPlayerSimulation() : base()
        {
            this.Components = new ComponentRegistry<NetworkPlayerSimulation>(this);
        }

        public virtual PlayerState ToNetworkState()
        {
            return new PlayerState
            {
                Id = int.Parse(this.Name),
            };
        }

        public virtual void ApplyNetworkState(PlayerState state)
        {

        }

        public virtual void Simulate(float delta)
        {
        }
    }
}
