namespace Shooter.Shared.Network.Packages
{
    public class ClientInitializationPackage
    {
    }

    public class ServerInitializationPackage
    {
        public uint GameTick { get; set; }
        public int PlayerId { get; set; }
    }
}
