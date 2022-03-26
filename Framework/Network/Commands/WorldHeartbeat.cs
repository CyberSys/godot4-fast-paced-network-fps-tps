
namespace Framework.Network.Commands
{
    public class WorldHeartbeat
    {
        // The world tick this data represents.
        public uint WorldTick { get; set; }

        // The last world tick the server acknowledged for you.
        // The client should use this to determine the last acked input, as well as to compute
        // its relative simulation offset.
        public uint YourLatestInputTick { get; set; }

        // States for all active players.
        public PlayerState[] PlayerStates { get; set; }

        public PlayerUpdate[] PlayerUpdates { get; set; }
    }
}