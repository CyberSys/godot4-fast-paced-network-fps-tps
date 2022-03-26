using Godot;
using Framework.Network.Commands;
using Framework;
using Framework.Game;
using Framework.Input;
using Framework.Physics;

namespace Framework.Network
{
    public abstract partial class NetworkPlayerSimulation : Node, IBaseComponent
    {
        private readonly ComponentRegistry _components;
        public ComponentRegistry Components => _components;

        public IWorld GameWorld { get; set; }

        protected PlayerInputs inputs;
        public IMoveable Body { get; set; }

        public void SetPlayerInputs(PlayerInputs inputs)
        {
            this.inputs = inputs;
        }

        public NetworkPlayerSimulation() : base()
        {
            this._components = new ComponentRegistry(this);
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
