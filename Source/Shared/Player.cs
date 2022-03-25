namespace Shooter.Shared
{
    public abstract class Player : IPlayer
    {
        public string Name { get; set; }
        public int Latency { get; set; }
        public ClientState State { get; set; }
        public ClientState PreviousState { get; set; }
        public PlayerTeam Team { get; set; }
    }
}
