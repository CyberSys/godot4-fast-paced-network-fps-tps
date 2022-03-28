using Framework.Game.Client;
using Framework.Game;

namespace Shooter.Client
{
    public partial class MyClientWorld : ClientWorld
    {
        public override LocalPlayer CreateLocalPlayer(int id)
        {
            return new MyLocalPlayer();
        }
        public override PuppetPlayer CreatePuppetPlayer(int id)
        {
            return new MyPuppetPlayer();
        }
    }
}