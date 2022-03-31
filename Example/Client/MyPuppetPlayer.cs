using Framework.Game.Client;
using Framework.Game;
using Shooter.Shared.Components;

namespace Shooter.Client
{
    public partial class MyPuppetPlayer : PuppetPlayer
    {
        public MyPuppetPlayer() : base()
        {
            this.AvaiablePlayerComponents.Add("body", new AssignedComponent(
                typeof(PlayerBodyComponent), "res://Assets/Player/PlayerBody.tscn"
            ));

            this.AvaiablePlayerComponents.Add("footsteps", new AssignedComponent(
                typeof(PlayerFootstepComponent)
            ));
        }
    }
}