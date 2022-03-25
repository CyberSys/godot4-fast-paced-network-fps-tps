using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Shooter.Shared.Network.Packages;
using Shooter.Client.Simulation.Components;

namespace Shooter.Shared
{
    public abstract partial class PlayerSimulation : Node
    {
        public ComponentRegistry<PlayerSimulation> Components { get; set; }
        public GameWorld GameWorld { get; set; }

        protected PlayerInputs inputs;

        public void SetPlayerInputs(PlayerInputs inputs)
        {
            this.inputs = inputs;
        }

        public PlayerSimulation() : base()
        {
            this.Components = new ComponentRegistry<PlayerSimulation>(this);
        }

        public virtual PlayerStatePackage ToNetworkState()
        {
            return new PlayerStatePackage
            {
                Id = int.Parse(this.Name),
            };
        }

        public virtual void ApplyNetworkState(PlayerStatePackage state)
        {

        }
    }
}
