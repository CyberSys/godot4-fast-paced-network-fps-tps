namespace Shooter.Shared.Network.Packages
{
    public class PlayerListUpdatePackage
    {
        public PlayerUpdatePackage[] Players { get; set; }
        public PlayerStatePackage[] PlayerStates { get; set; }
        public uint WorldTick { get; set; }
    }
}
