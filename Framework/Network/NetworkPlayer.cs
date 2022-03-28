using Godot;
using Framework.Network.Commands;
using Framework;
using Framework.Game;
using Framework.Input;
using Framework.Physics;

namespace Framework.Network
{
    public abstract partial class NetworkPlayer : Player, IBaseComponent
    {
        /// <inheritdoc />
        public NetworkPlayer(int id, IWorld world) : base(id, world)
        {
        }

        protected IPlayerInput inputs;

        public void SetPlayerInputs(IPlayerInput inputs)
        {
            this.inputs = inputs;
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
    }
}
