using Shooter.Shared;
using Shooter.Shared.Network.Packages;

namespace Shooter.Client.World
{
    public class LocalPlayer : Player
    {
        public PlayerInputs[] localPlayerInputsSnapshots = new PlayerInputs[1024];
        public PlayerState[] localPlayerStateSnapshots = new PlayerState[1024];
        public uint[] localPlayerWorldTickSnapshots = new uint[1024];
    }
}
