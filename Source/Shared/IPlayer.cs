namespace Shooter.Shared
{
    public enum ClientState
    {
        Unknown,
        Connected,
        Initialized,
        Disconnected
    }
    public interface IPlayer
    {
        public string Name { get; set; }
        public int Latency { get; set; }
        public ClientState State { get; set; }
        public PlayerTeam Team { get; set; }
    }
}
