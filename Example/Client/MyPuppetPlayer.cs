using Framework.Game.Client;
using Framework.Game;
using Shooter.Shared.Components;

namespace Shooter.Client
{
    public partial class MyPuppetPlayer : PuppetPlayer
    {
        public MyPuppetPlayer() : base()
        {
            this.AddAvaiableComponent<PlayerBodyComponent>("res://Assets/Player/PlayerBody.tscn");
            this.AddAvaiableComponent<PlayerFootstepComponent>();
        }
    }
}