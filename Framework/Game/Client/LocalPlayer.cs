using Framework.Game;
using Framework.Network.Commands;
using Framework.Input;

namespace Framework.Game.Client
{
    public class LocalPlayer : Player<LocalPlayerSimulation>
    {
        public PlayerInputs[] localPlayerInputsSnapshots = new PlayerInputs[1024];
        public PlayerState[] localPlayerStateSnapshots = new PlayerState[1024];
        public uint[] localPlayerWorldTickSnapshots = new uint[1024];
    }
}
