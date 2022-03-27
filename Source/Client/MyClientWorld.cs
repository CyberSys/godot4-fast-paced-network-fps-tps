using Framework.Game.Client;
using Framework.Game;

namespace Shooter.Client
{
    public partial class MyClientWorld : ClientWorld
    {
        public override LocalPlayer CreateLocalPlayer(int id, IWorld world)
        {
            return new MyLocalPlayer(id, world);
        }
        public override PuppetPlayer CreatePuppetPlayer(int id, IWorld world)
        {
            return new MyPuppetPlayer(id, world);
        }
    }
}