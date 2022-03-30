using Framework.Game.Server;

namespace Shooter.Server
{
    public partial class MyServerWorld : ServerWorld
    {
        public override void OnPlayerConnected(int clientId)
        {
            this.AddPlayer<MyServerPlayer>(clientId);
        }
    }
}