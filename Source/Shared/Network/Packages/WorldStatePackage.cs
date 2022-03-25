
namespace Shooter.Shared.Network.Packages
{
    public class WorldStatePackage
    {
        // The world tick this data represents.
        public uint WorldTick { get; set; }

        // The last world tick the server acknowledged for you.
        // The client should use this to determine the last acked input, as well as to compute
        // its relative simulation offset.
        public uint YourLatestInputTick { get; set; }

        // States for all active players.
        public PlayerStatePackage[] PlayerStates { get; set; }
    }
}